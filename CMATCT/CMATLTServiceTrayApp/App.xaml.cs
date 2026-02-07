using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using CMATCTUI.ChildWindows;
using CMATLTLoger;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;

namespace CMATLTServiceTrayApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private TaskbarIcon _trayIcon;

        protected override void OnStartup(StartupEventArgs e)
        {

            base.OnStartup(e);
            //MessageBox.Show("hit2");
            ////StartPipeServer();
            //StartFileWatcher();
            ////AutoStartHelper.SetTrayAppAutoStart(true);
            //MessageBox.Show("Tray App Started");



            _trayIcon = (TaskbarIcon)FindResource("TrayIcon");


            _trayIcon.ToolTipText = "ATLT Tool";
            _trayIcon.Visibility = Visibility.Visible;

            // RegisterTray();

        }

        private void RegisterTray()
        {

            string servicePath = Assembly.GetExecutingAssembly().Location; // Full path of exe
            string serviceDir = Path.GetDirectoryName(servicePath);
            //string targetDir = "C:\\Program Files (x86)\\Thermo Fisher Scientific, Inc\\Setup2\\"; // MSI passes TARGETDIR
            //string exePath = Path.Combine(targetDir, "MyServiceTrayApp.exe");
            //string exePath = Path.Combine(serviceDir, "CMATLTServiceTrayApp.exe");

            using (var key = Registry.CurrentUser.OpenSubKey(
                     @"Software\Microsoft\Windows\CurrentVersion\Run", true))
            {
                key.SetValue(
                    "ATLTTRAY_APP",
                    $"\"{"C:test"}\"",
                    RegistryValueKind.String
                );
            }

        }

        protected override void OnExit(ExitEventArgs e)
        {
            _trayIcon?.Dispose();
            base.OnExit(e);
        }

        private void StartService_Click(object sender, RoutedEventArgs e)
        {
            LogLibrary.LogUIInfo("StartService_Click invoked");
            try
            {
                ServiceManager.StartService();
                ShowNotification("Service Control", "Service started successfully", BalloonIcon.Info);
            }
            catch (Exception ex)
            {
                LogLibrary.LogUIError($"Failed to start service: {ex.Message}");
                ShowNotification("Service Control", $"Failed to start service: {ex.Message}", BalloonIcon.Error);
            }
        }

        private void StopService_Click(object sender, RoutedEventArgs e)
        {
            LogLibrary.LogUIInfo("StopService_Click invoked");
            try
            {
                ServiceManager.StopService();
                ShowNotification("Service Control", "Service stopped", BalloonIcon.Info);
            }
            catch (Exception ex)
            {
                LogLibrary.LogUIError($"Failed to stop service: {ex.Message}");
                ShowNotification("Service Control", $"Failed to stop service: {ex.Message}", BalloonIcon.Error);
            }
        }

        private void StatusService_Click(object sender, RoutedEventArgs e)
        {
            LogLibrary.LogUIInfo("StatusService_Click invoked");
            try
            {
                var status = ServiceManager.GetStatus();
                ShowNotification("Service Status", $"Current status: {status}", BalloonIcon.Info);
            }
            catch (Exception ex)
            {
                LogLibrary.LogUIError($"Failed to get status: {ex.Message}");
                ShowNotification("Service Status", $"Failed to get status: {ex.Message}", BalloonIcon.Error);
            }
        }

        private void OpenUI_Click(object sender, RoutedEventArgs e)
        {
            LogLibrary.LogUIInfo("OpenUI_Click invoked");
            try
            {
                string installPath = Path.GetDirectoryName(
                                     System.Reflection.Assembly.GetExecutingAssembly().Location);
                string exePath = Path.Combine(installPath, "CMATLTUI.exe");

                if (File.Exists(exePath))
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = exePath,
                        UseShellExecute = true,  // needed for GUI apps
                        CreateNoWindow = false
                    };
                    Process.Start(psi);
                }
                else
                {
                    ShowNotification("Error", "CMATLTUI.exe not found", BalloonIcon.Error);
                }
            }
            catch (Exception ex)
            {
                LogLibrary.LogUIError($"Failed to open UI: {ex.Message}");
                ShowNotification("Error", $"Failed to open UI: {ex.Message}", BalloonIcon.Error);
            }
        }

        private void ViewLogs_Click(object sender, RoutedEventArgs e)
        {
            LogLibrary.LogUIInfo("ViewLogs_Click invoked");
            try
            {
                var logViewer = new LogViewerWindow();
                logViewer.Show();
            }
            catch (Exception ex)
            {
                LogLibrary.LogUIError($"Failed to open logs: {ex.Message}");
                ShowNotification("Error", $"Failed to open logs: {ex.Message}", BalloonIcon.Error);
            }
        }

        private void ExitTray_Click(object sender, RoutedEventArgs e)
        {
            LogLibrary.LogUIInfo("ExitTray_Click invoked");
            Shutdown();
        }

        /// <summary>
        /// Shows a balloon tip notification that auto-closes after 3 seconds
        /// </summary>
        private void ShowNotification(string title, string message, BalloonIcon icon = BalloonIcon.Info)
        {
            _trayIcon?.ShowBalloonTip(title, message, icon);
        }
    }
}
