using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CMATLTServiceTrayApp
{
    public static class ServiceManager
    {
        private const string ServiceName = "CMATLTService";

        public static void StartService()
        {
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
                }
            }
        }
        public static void SetTrayAppAutoStart(bool enable)
        {
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
            using (ServiceController sc = new ServiceController(ServiceName))
            {
                if (sc.Status == ServiceControllerStatus.Running)
                {
                    sc.Stop();
                }

                //if (sc.Status == ServiceControllerStatus.StartPending)
                //{
                //    sc.WaitForStatus(
                //        ServiceControllerStatus.Running,
                //        TimeSpan.FromSeconds(60) // increase timeout
                //    );
                //}
            }
            //var sc = new ServiceController(ServiceName);
            //if (sc.Status == ServiceControllerStatus.Running)
            //{
            //    sc.Stop();
            //    sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
            //}
        }

        public static ServiceControllerStatus GetStatus()
        {
            var sc = new ServiceController(ServiceName);
            return sc.Status;
        }
    }
}
