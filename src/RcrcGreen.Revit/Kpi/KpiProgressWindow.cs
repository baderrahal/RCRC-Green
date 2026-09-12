using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using RcrcGreen.Core.Kpi;

namespace RcrcGreen.Revit.Kpi
{
    /// <summary>
    /// The window a press of Create shows while it runs. A 123 second read behind a docked
    /// pane is a tool that looks dead, and the status line alone cannot be seen when the pane
    /// is behind something else.
    ///
    /// **Modeless, and owned by the Revit main window.** Owned, so it stays over Revit rather
    /// than behind it and goes away with it, and so Windows treats it as part of Revit rather
    /// than as a second top level window. Modeless, so it never blocks the thread the run is
    /// on. **It is never opened from inside Execute**: the pane opens it before it raises the
    /// external event and closes it when the run's last answer comes back, so nothing here
    /// runs inside the handler's own call frame.
    ///
    /// It shows what the status line shows, the same words out of ProgressWords, because two
    /// wordings for one run is two records of one fact.
    ///
    /// **There is no Cancel and that is deliberate.** Cancelling mid read would leave a half
    /// read set of plots that the reconciliation would count as plots read, which is the one
    /// state the rest of the tool would trust and should not. A cancel that lies is worse
    /// than no cancel, and an honest one is a cancellation threaded through every reader and
    /// a discard of everything read, which is its own round. The log records it as left out.
    /// </summary>
    internal sealed class KpiProgressWindow : Window
    {
        private readonly TextBlock _line = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Margin = PanelMetrics.Row
        };

        private KpiProgressWindow()
        {
            Title = ProgressWords.WindowTitle;
            Width = PanelMetrics.ProgressWindowWidth;
            SizeToContent = SizeToContent.Height;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            PanelTheme theme = PanelTheme.Current();
            Background = theme.Background;
            Foreground = theme.Foreground;
            FontSize = PanelMetrics.Body;

            var inside = new StackPanel { Margin = PanelMetrics.StripInside };
            inside.Children.Add(_line);
            Content = inside;
        }

        /// <summary>
        /// Opened on the interface thread before the external event is raised, never from
        /// inside Execute. Owned by the Revit main window through its handle, because a
        /// window owned by nobody sits over every application on the machine and leaves
        /// Revit's own ribbon live behind it.
        /// </summary>
        public static KpiProgressWindow Opened(string firstLine)
        {
            var window = new KpiProgressWindow();
            window.Moved(firstLine);

            IntPtr revit = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            if (revit != IntPtr.Zero)
            {
                new WindowInteropHelper(window) { Owner = revit };
            }

            window.Show();
            return window;
        }

        public void Moved(string what)
        {
            _line.Text = what ?? string.Empty;
        }

        /// <summary>
        /// Closing a window that is already closed throws, and the run ending twice is
        /// ordinary: a refusal says its piece through Told and the press ends again after it.
        /// </summary>
        public void Done()
        {
            if (!IsLoaded) return;

            Close();
        }
    }
}
