using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using CMATLTLoger;

namespace CMATCTUI.ChildWindows
{
    public partial class LogViewerWindow : Window
    {
        private DispatcherTimer _timer;
        private long _lastPosition = 0;
        private string _logPath;

        public LogViewerWindow()
        {
            InitializeComponent();

            //Initialize log path once
            _logPath = Path.Combine(LogLibrary.FolderPath, "UI.log"); // Case-sensitive fix

            LoadLogs();
            StartAutoRefresh();
        }

        private void LoadLogs()
        {
            FileStream fs = null;
            StreamReader sr = null;
            try
            {
                // Check if file exists first
                if (!File.Exists(_logPath))
                {
                    if (_lastPosition == 0) // Only show once
                    {
                        LogTextBox.AppendText($"Log file not found: {_logPath}\n");
                        LogTextBox.AppendText("Waiting for logs to be generated...\n\n");
                    }
                    return;
                }
                // Get file info to check if it's accessible
                FileInfo fileInfo = new FileInfo(_logPath);
                if (fileInfo.Length < _lastPosition)
                {
                    // File was truncated/recreated, reset position
                    _lastPosition = 0;
                    LogTextBox.Clear();
                    LogTextBox.AppendText("=== Log file was reset ===\n\n");
                }

                // Open with proper sharing flags
                fs = new FileStream(
                    _logPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete, // Allow delete too
                    4096, // Buffer size
                    FileOptions.SequentialScan // Optimize for sequential reading
                );

                // Seek to last position
                if (_lastPosition > 0 && _lastPosition < fs.Length)
                {
                    fs.Seek(_lastPosition, SeekOrigin.Begin);
                }
                else if (_lastPosition == 0)
                {
                    fs.Seek(0, SeekOrigin.Begin);
                }

                sr = new StreamReader(fs, Encoding.UTF8);
                string newContent = sr.ReadToEnd();

                if (!string.IsNullOrEmpty(newContent))
                {
                    LogTextBox.AppendText(newContent);
                    LogTextBox.ScrollToEnd();
                }

                // Update position AFTER successful read
                _lastPosition = fs.Position;

                // Old Code start   
                //string logPath = Path.Combine(LogLibrary.FolderPath, "UI.log");

                //if (File.Exists(logPath))
                //{
                //    using (var fs = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                //    {
                //        fs.Seek(_lastPosition, SeekOrigin.Begin);
                //        using (var sr = new StreamReader(fs))
                //        {
                //            string newContent = sr.ReadToEnd();
                //            if (!string.IsNullOrEmpty(newContent))
                //            {
                //                LogTextBox.AppendText(newContent);
                //                LogTextBox.ScrollToEnd();
                //            }
                //        }
                //        _lastPosition = fs.Position;
                //    }
                //}
                // Old Code end
            }
            catch (FileNotFoundException fnfEx)
            {
                if (_lastPosition == 0)
                {
                    LogTextBox.AppendText($"\n[Log Viewer] File not found: {fnfEx.Message}\n");
                }
            }
            catch (IOException ioEx)
            {
                // Handle file locked/closed gracefully
                LogTextBox.AppendText($"\n[Log Viewer] File access error (will retry): {ioEx.Message}\n");
                // Don't reset _lastPosition, retry on next tick
            }
            catch (UnauthorizedAccessException uaEx)
            {
                LogTextBox.AppendText($"\n[Log Viewer] Access denied: {uaEx.Message}\n");
            }
            catch (Exception ex)
            {
                LogTextBox.AppendText($"\n[Log Viewer] Unexpected error: {ex.GetType().Name} - {ex.Message}\n");
                LogLibrary.LogUIError($"LogViewerWindow error: {ex}");
            }
            finally
            {
                // Ensure proper cleanup
                sr?.Dispose();
                fs?.Dispose();
            }

            // Old Code start   
            //catch (Exception ex)
            //{
            //    LogTextBox.AppendText($"\nLog Viewer [This window] exception while loading logs: {ex.Message}\n");
            //}
            // Old Code end
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
            // old code start
            //_timer?.Stop();
            //base.OnClosed(e);
            // old code end
            try
            {
                _timer?.Stop();
                _timer = null;
            }
            catch (Exception ex)
            {
                // Silently handle cleanup errors
                System.Diagnostics.Debug.WriteLine($"Error stopping timer: {ex.Message}");
            }
            finally
            {
                base.OnClosed(e);
            }

        }
    }
}