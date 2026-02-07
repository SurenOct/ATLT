using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using CMATLTLoger;

namespace CMATCTUI.ChildWindows
{
    public partial class LogViewerWindow : Window
    {
        private DispatcherTimer _timer;
        private long _lastPosition = 0;

        public LogViewerWindow()
        {
            InitializeComponent();
            LoadLogs();
            StartAutoRefresh();
        }

        private void LoadLogs()
        {
            try
            {
                string logPath = Path.Combine(LogLibrary.FolderPath, "ui.log");

                if (File.Exists(logPath))
                {
                    using (var fs = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        fs.Seek(_lastPosition, SeekOrigin.Begin);
                        using (var sr = new StreamReader(fs))
                        {
                            string newContent = sr.ReadToEnd();
                            if (!string.IsNullOrEmpty(newContent))
                            {
                                LogTextBox.AppendText(newContent);
                                LogTextBox.ScrollToEnd();
                            }
                        }
                        _lastPosition = fs.Position;
                    }
                }
            }
            catch (Exception ex)
            {
                LogTextBox.AppendText($"\nLog Viewer [This window] exception while loading logs: {ex.Message}\n");
            }
        }

        private void StartAutoRefresh()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _timer.Tick += (s, e) => LoadLogs();
            _timer.Start();
        }

        protected override void OnClosed(EventArgs e)
        {
            _timer?.Stop();
            base.OnClosed(e);
        }
    }
}