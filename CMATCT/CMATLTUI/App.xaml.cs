using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CMATLTLoger;
using Microsoft.Win32;

namespace CMATCTUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex _mutexForUI;

        protected override void OnStartup(StartupEventArgs e)
        {
            //RegisterTrayAsync();
            const string appName = "CMATLTUI";
            _mutexForUI = new Mutex(true, appName, out bool isNew);

            if (!isNew)
            {
                Shutdown();
                return;
            }

            base.OnStartup(e);

        }
        private void RegisterTrayAsync()
        {

            //       MessageBox.Show(
            //System.Security.Principal.WindowsIdentity.GetCurrent().Name,
            //"Running As");

            Task.Run(() =>
            {
                try
                {
                    string servicePath = Assembly.GetExecutingAssembly().Location;
                    string serviceDir = Path.GetDirectoryName(servicePath);
                    string exePath = Path.Combine(serviceDir, "CMATLTServiceTrayApp.exe");

                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                             @"Software\Microsoft\Windows\CurrentVersion\Run", true))
                    {
                        key.SetValue("ATLTTRAY_APP", $"\"{"C:\\Program Files (x86)\\Thermo Fisher Scientific, Inc\\CMATLTSetup\\Bin\\CMATLTServiceTrayApp.exe"}\"", RegistryValueKind.String);
                        key.SetValue("ATLTTRAY_APP", $"\"{"C:\\Program Files (x86)\\Thermo Fisher Scientific, Inc\\CMATLTSetup\\Bin\\CMATLTServiceTrayApp.exe"}\"");

                        key.SetValue(
                                     "ATLTTRAY_APP",
                                     "\"C:\\Program Files\\CMATLT\\TrayApp.exe\"",
                                        RegistryValueKind.String
                                    );
                    }
                    //using (RegistryKey key = Registry.CurrentUser.CreateSubKey(
                    //    @"Software\Microsoft\Windows\CurrentVersion\Run", true))
                    //{
                    //    key?.SetValue("CMATLTServiceTrayApp", $"\"{exePath}\"");
                    //}
                }
                catch (Exception ex)
                {
                    // log exception (don’t show MessageBox from background thread)
                    LogLibrary.LogError($"Failed to register tray app: {ex.Message}");
                }
            });
        }
    }
}
