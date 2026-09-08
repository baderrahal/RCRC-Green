using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace RcrcGreen.Revit
{
    /// <summary>
    /// A small window with a bar and a Cancel button, shown while a command reads the model.
    ///
    /// A Revit command runs on the same thread as the interface, so the only way this window
    /// repaints and the only way its button can be clicked is to let the message queue run
    /// during the read. That is what <see cref="Application.DoEvents"/> does here. It is the
    /// usual way to do this in an add-in and it is not free of risk, because it lets other
    /// clicks through as well. It is used only while reading, never while a transaction is
    /// open, so nothing half written can be reached this way.
    /// </summary>
    internal sealed class ScanProgressWindow : IScanWatcher, IDisposable
    {
        /// <summary>
        /// Pumping the queue on every element would cost more than the read itself. A tenth of
        /// a second is often enough for a bar to look alive and a button to answer.
        /// </summary>
        private static readonly TimeSpan BetweenPumps = TimeSpan.FromMilliseconds(100);

        private readonly Form _window;
        private readonly ProgressBar _bar;
        private readonly Label _caption;
        private readonly Stopwatch _sinceLastPump;
        private bool _cancelled;
        private bool _closed;

        public ScanProgressWindow(IntPtr revitWindow, string title, string caption)
        {
            _bar = new ProgressBar
            {
                Left = 12,
                Top = 40,
                Width = 356,
                Height = 20,
                Minimum = 0,
                Maximum = 100,
                Style = ProgressBarStyle.Continuous
            };

            _caption = new Label
            {
                Left = 12,
                Top = 12,
                Width = 356,
                Height = 20,
                Text = caption
            };

            var stop = new Button
            {
                Left = 293,
                Top = 70,
                Width = 75,
                Height = 25,
                Text = "Cancel"
            };
            stop.Click += (sender, e) =>
            {
                _cancelled = true;
                stop.Enabled = false;
                _caption.Text = "Stopping.";
            };

            _window = new Form
            {
                Text = title,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterScreen,
                MinimizeBox = false,
                MaximizeBox = false,
                ShowInTaskbar = false,
                ClientSize = new Size(380, 108)
            };
            _window.Controls.Add(_caption);
            _window.Controls.Add(_bar);
            _window.Controls.Add(stop);

            // Closing the window with the cross means the same thing as the button. The guard
            // matters because Dispose closes the window on a normal finish too, and without it
            // every completed read would end up marked as cancelled.
            _window.FormClosing += (sender, e) =>
            {
                if (!_closed) _cancelled = true;
            };

            if (revitWindow == IntPtr.Zero)
            {
                _window.Show();
            }
            else
            {
                // Without an owner this can slide behind the Revit window, where a progress
                // bar is worse than none because the user cannot see it or reach Cancel.
                _window.Show(new RevitWindow(revitWindow));
            }

            _sinceLastPump = Stopwatch.StartNew();
            Application.DoEvents();
        }

        public bool Cancelled
        {
            get { return _cancelled; }
        }

        public void Report(int done, int total)
        {
            if (_closed) return;
            if (_sinceLastPump.Elapsed < BetweenPumps) return;

            _sinceLastPump.Restart();

            if (total > 0)
            {
                int percent = (int)(100L * done / total);
                _bar.Value = percent < 0 ? 0 : (percent > 100 ? 100 : percent);
            }

            if (!_cancelled)
            {
                _caption.Text = done.ToString("N0") + " of " + total.ToString("N0");
            }

            Application.DoEvents();
        }

        public void Dispose()
        {
            if (_closed) return;
            _closed = true;

            _window.Close();
            _window.Dispose();
        }

        private sealed class RevitWindow : IWin32Window
        {
            public RevitWindow(IntPtr handle)
            {
                Handle = handle;
            }

            public IntPtr Handle { get; }
        }
    }
}
