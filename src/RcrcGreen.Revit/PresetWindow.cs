using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using RcrcGreen.Core;

namespace RcrcGreen.Revit
{
    /// <summary>
    /// The two windows behind Save as and Manage.
    ///
    /// **Modal, and owned by the Revit main window.** Owned, so it sits over Revit rather than
    /// behind it and Windows treats it as part of Revit. Modal, because both are one short
    /// question and the panel has nothing to do until it is answered. No Revit API call is made
    /// in either, so neither goes through the external event: a preset file is not a document.
    ///
    /// Everything the windows say about a preset comes out of Core, so the picker, the panel
    /// and these agree by construction rather than by two people writing the same sentence.
    /// </summary>
    internal sealed class PresetWindow : Window
    {
        private PresetWindow(string title)
        {
            Title = title;
            Width = PanelMetrics.ProgressWindowWidth;
            SizeToContent = SizeToContent.Height;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            PanelTheme theme = PanelTheme.Current();
            Background = theme.Background;
            Foreground = theme.Foreground;
            FontSize = PanelMetrics.Body;
            Faint = theme.Faint;
        }

        private System.Windows.Media.Brush Faint { get; }

        /// <summary>
        /// Asks what to call this answer to steps 2 and 4, and hands back the name, or empty
        /// when the window was closed without one.
        ///
        /// Saving over a name already in use is allowed and is said before it happens, because
        /// correcting a preset is the ordinary reason to save a second time under one name.
        /// </summary>
        public static string AskForAName(Presets held, string suggested, string whatIsBeingSaved)
        {
            var window = new PresetWindow("Save as a preset");

            var name = new TextBox
            {
                Text = suggested ?? string.Empty,
                Margin = PanelMetrics.Row
            };

            var over = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                Foreground = window.Faint,
                Margin = PanelMetrics.Row
            };

            var inside = new StackPanel { Margin = PanelMetrics.StripInside };
            inside.Children.Add(new TextBlock
            {
                Text = whatIsBeingSaved ?? string.Empty,
                TextWrapping = TextWrapping.Wrap,
                Margin = PanelMetrics.Row
            });
            inside.Children.Add(new TextBlock { Text = "Name", Margin = PanelMetrics.Row });
            inside.Children.Add(name);
            inside.Children.Add(over);

            string answer = string.Empty;
            var save = new Button
            {
                Content = PaneLabel.Escaped("Save"),
                Margin = PanelMetrics.Gap,
                Padding = PanelMetrics.CellPad,
                IsDefault = true
            };

            var cancel = new Button
            {
                Content = PaneLabel.Escaped("Cancel"),
                Margin = PanelMetrics.Gap,
                Padding = PanelMetrics.CellPad,
                IsCancel = true
            };

            Action told = () =>
            {
                string wanted = name.Text == null ? string.Empty : name.Text.Trim();
                save.IsEnabled = wanted.Length > 0;

                Preset already = held == null ? null : held.Named(wanted);
                over.Text = already == null
                    ? string.Empty
                    : "A preset called " + already.Name + " is already saved, from "
                        + already.WhereItCameFrom() + ". Saving replaces it with this one.";
            };

            name.TextChanged += (sender, e) => told();
            told();

            save.Click += (sender, e) =>
            {
                answer = name.Text == null ? string.Empty : name.Text.Trim();
                window.DialogResult = true;
            };

            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = PanelMetrics.Row
            };
            buttons.Children.Add(save);
            buttons.Children.Add(cancel);
            inside.Children.Add(buttons);

            window.Content = inside;
            name.Focus();
            name.SelectAll();

            return window.Asked() == true ? answer : string.Empty;
        }

        /// <summary>
        /// Shows every saved preset and takes one away. It hands back the name deleted, or
        /// empty when nothing was.
        ///
        /// A shipped preset has no Delete. Nothing here writes the shipped file, so a button
        /// that looked like it took one away and did not would be the worst of both.
        /// </summary>
        public static string Manage(Presets held, IReadOnlyList<Preset> shipped)
        {
            var window = new PresetWindow("Manage presets");

            string deleted = string.Empty;
            var inside = new StackPanel { Margin = PanelMetrics.StripInside };

            if (held == null || held.Count == 0)
            {
                inside.Children.Add(new TextBlock
                {
                    Text = "No preset is saved. Describe steps 2 and 4 and press Save as.",
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = window.Faint,
                    Margin = PanelMetrics.Row
                });
            }
            else
            {
                foreach (Preset one in held.All)
                {
                    Preset which = one;
                    var line = new DockPanel
                    {
                        Margin = PanelMetrics.Row,
                        LastChildFill = true
                    };

                    if (which.Source == PresetSource.UserFile)
                    {
                        var remove = new Button
                        {
                            Content = PaneLabel.Escaped("Delete"),
                            Margin = PanelMetrics.Gap,
                            Padding = PanelMetrics.CellPad,
                            ToolTip = held.WhatDeletingDoes(which.Name, shipped)
                        };

                        remove.Click += (sender, e) =>
                        {
                            MessageBoxResult answer = MessageBox.Show(
                                held.WhatDeletingDoes(which.Name, shipped),
                                "Delete " + which.Name,
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question,
                                MessageBoxResult.No);

                            if (answer != MessageBoxResult.Yes) return;

                            deleted = which.Name;
                            window.DialogResult = true;
                        };

                        DockPanel.SetDock(remove, Dock.Right);
                        line.Children.Add(remove);
                    }

                    var said = new StackPanel();
                    said.Children.Add(new TextBlock
                    {
                        Text = which.Name,
                        TextWrapping = TextWrapping.Wrap,
                        FontWeight = FontWeights.Bold
                    });

                    said.Children.Add(new TextBlock
                    {
                        Text = which.InWords() + " From " + which.WhereItCameFrom() + ".",
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = window.Faint
                    });

                    line.Children.Add(said);
                    inside.Children.Add(line);
                }
            }

            var close = new Button
            {
                Content = PaneLabel.Escaped("Close"),
                Margin = PanelMetrics.Gap,
                Padding = PanelMetrics.CellPad,
                HorizontalAlignment = HorizontalAlignment.Right,
                IsCancel = true,
                IsDefault = true
            };

            inside.Children.Add(close);
            window.Content = inside;

            window.Asked();
            return deleted;
        }

        /// <summary>
        /// Owned by the Revit main window through its handle, because a window owned by nobody
        /// sits over every application on the machine and leaves Revit's own ribbon live
        /// behind it.
        /// </summary>
        private bool? Asked()
        {
            IntPtr revit = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            if (revit != IntPtr.Zero)
            {
                new WindowInteropHelper(this) { Owner = revit };
            }

            return ShowDialog();
        }
    }
}
