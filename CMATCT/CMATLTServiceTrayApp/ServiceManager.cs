using System;
using System.ServiceProcess;
using System.Windows;
using CMATLTLoger;
using Microsoft.Win32;

namespace CMATLTServiceTrayApp
{
    public static class ServiceManager
    {
        private const string ServiceName = "CMATLTService";

        public static void StartService()
        {
            LogLibrary.LogUIInfo("Attempting to start service from tray ...");
            using (ServiceController sc = new ServiceController(ServiceName))
            {
                if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    sc.Start();
                }

                if (sc.Status == ServiceControllerStatus.StartPending)
                {
                    sc.WaitForStatus(
                        ServiceControllerStatus.Running,
                        TimeSpan.FromSeconds(60) // increase timeout
                    );
                    LogLibrary.LogUIInfo("Service started successfully from tray.");
                }
            }
        }
        public static void SetTrayAppAutoStart(bool enable)
        {
            LogLibrary.LogUIInfo($"Setting tray app auto-start to {(enable ? "enabled" : "disabled")} ...");
            try
            {
                string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                const string keyName = "MyServiceTrayApp";

                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

                if (enable)
                    registryKey.SetValue(keyName, $"\"{exePath}\"");
                else
                    registryKey.DeleteValue(keyName, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to set startup registry: {ex.Message}");
            }
        }
        public static void StopService()
        {
            LogLibrary.LogUIInfo("Attempting to stop service from tray ...");
            using (ServiceController sc = new ServiceController(ServiceName))
            {
                if (sc.Status == ServiceControllerStatus.Running)
                {
                    sc.Stop();
                }

                if (sc.Status == ServiceControllerStatus.StopPending)
                {
                    sc.WaitForStatus(
                        ServiceControllerStatus.Stopped,
                        TimeSpan.FromSeconds(60) // increase timeout
                    );
                    LogLibrary.LogUIInfo("Service stopped successfully from tray.");
                }
            }
        }

        public static ServiceControllerStatus GetStatus()
        {
            LogLibrary.LogUIInfo("Checking service status from tray ...");
            var sc = new ServiceController(ServiceName);
            return sc.Status;
        }
    }
}
