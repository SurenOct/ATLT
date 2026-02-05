using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
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


            _trayIcon.ToolTipText = "Tray icon loaded";
            _trayIcon.Visibility = Visibility.Visible;

            // RegisterTray();

        }

        private void RegisterTray()
        {

            string servicePath = Assembly.GetExecutingAssembly().Location; // Full path of exe
            string serviceDir = Path.GetDirectoryName(servicePath);
            //string targetDir = "C:\\Program Files (x86)\\Thermo Fisher Scientific, Inc\\Setup2\\"; // MSI passes TARGETDIR
            //string exePath = Path.Combine(targetDir, "MyServiceTrayApp.exe");
            string exePath = Path.Combine(serviceDir, "CMATLTServiceTrayApp.exe");

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
            ServiceManager.StartService();
            System.Windows.Forms.MessageBox.Show("Service Started");
        }

        private void StopService_Click(object sender, RoutedEventArgs e)
        {
            ServiceManager.StopService();
            System.Windows.Forms.MessageBox.Show("Service Stopped");
        }

        private void StatusService_Click(object sender, RoutedEventArgs e)
        {
            var status = ServiceManager.GetStatus();
            System.Windows.Forms.MessageBox.Show($"Service Status: {status}");
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
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
        }

        //private void Exit_Click(object sender, RoutedEventArgs e)
        //{
        //    Shutdown();
        //}
    }
}
