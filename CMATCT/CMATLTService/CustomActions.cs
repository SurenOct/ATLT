using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.EnterpriseServices.Internal;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Security.Policy;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace CMATLTService
{
    [RunInstaller(true)]
    public class CustomActions : Installer
    {
        public override void Commit(IDictionary savedState)
        {
            base.Commit(savedState);
            //Debugger.Launch();
            //Debugger.Break();

           

            //Start the Services
            string ServiceName = "CMATLTService";
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
                        TimeSpan.FromSeconds(30) // increase timeout
                    );
                }
            }



            string servicePath = Assembly.GetExecutingAssembly().Location; // Full path of exe
            string serviceDir = Path.GetDirectoryName(servicePath);
            //string targetDir = "C:\\Program Files (x86)\\Thermo Fisher Scientific, Inc\\Setup2\\"; // MSI passes TARGETDIR
            //string exePath = Path.Combine(targetDir, "MyServiceTrayApp.exe");
            string exePath = Path.Combine(serviceDir, "CMATLTServiceTrayApp.exe");
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

            #region //Register the name


            //Debugger.Launch();
            //Debugger.Break();
            //string installDir = Context.Parameters["INSTALLDIR"];
            //string instr = Context.Parameters["assemblypath"];
            //string exePath1 = Path.Combine(instr, "TrayApp.exe");

            //using (var key = Registry.CurrentUser.OpenSubKey(
            //       @"Software\Microsoft\Windows\CurrentVersion\Run", true))
            //{
            //    key.SetValue(
            //        "ATLTTRAY_APP",
            //        $"\"{instr}\"",
            //        RegistryValueKind.String
            //    );
            //}

            #endregion

        }
        public override void Install(IDictionary stateSaver)
        {
            try
            {
                base.Install(stateSaver);
                List<string> test = new List<string>();
                test.Add("Thermo.Chromeleon.Sdk.dll");
                test.Add("Thermo.Chromeleon.Sdk.Implementation.dll");
                test.Add("Thermo.Chromeleon.Sdk.Addins.dll");
                test.Add("Thermo.Chromeleon.SdkControls.dll");

                foreach (string file in test)
                {

                    //Debugger.Launch();   // 👈 REQUIRED
                    //Debugger.Break();

                    Directory.CreateDirectory(@"C:\TestService");

                    //MSI provides this automatically(DO NOT SET IT)
                    string exePath = Context.Parameters["assemblypath"];

                    if (string.IsNullOrEmpty(exePath))
                    {
                        File.WriteAllText(@"C:\Temp\ERROR.txt", "assemblypath is NULL");
                        return;
                    }
                    // Installed folder (dynamic on every machine)
                    string installDir = Path.GetDirectoryName(exePath);

                    // DLL deployed by MSI (must be inside Application Folder)
                    string dllPath = Path.Combine(installDir, file);

                    File.WriteAllText(@"C:\TestService\PATH.txt", dllPath);

                    // Install into GAC
                    GacHelper.Install(dllPath);
                }


            }
            catch (Exception ex)
            {
                File.WriteAllText(@"C:\Temp\EXCEPTION.txt", "Installation " + ex.ToString());
            }
        }

        public override void Uninstall(IDictionary savedState)
        {

            base.Uninstall(savedState);
            List<string> test = new List<string>();
            test.Add("Thermo.Chromeleon.Sdk.dll");
            test.Add("Thermo.Chromeleon.Sdk.Implementation.dll");
            test.Add("Thermo.Chromeleon.Sdk.Addins.dll");
            test.Add("Thermo.Chromeleon.SdkControls.dll");
            foreach (string file in test)
            {
                //Debugger.Launch();   // 👈 REQUIRED
                //Debugger.Break();
                Directory.CreateDirectory(@"C:\TestService1");

                try
                {

                    string exePath = Context.Parameters["assemblypath"];

                    if (string.IsNullOrEmpty(exePath))
                    {
                        File.WriteAllText(@"C:\Temp\ERROR.txt", "assemblypath is NULL");
                        return;
                    }

                    // Installed folder (dynamic on every machine)
                    string installDir = Path.GetDirectoryName(exePath);

                    // DLL deployed by MSI (must be inside Application Folder)
                    string dllPath = Path.Combine(installDir, file);
                    AssemblyName asmName = AssemblyName.GetAssemblyName(dllPath);
                    string fullName = asmName.FullName;
                    // Optional: remove DLL from GAC
                    // GacHelper.Uninstall(dllPath);
                    GacHelper.Uninstall(fullName);
                    // GacHelper.Uninstall(@"C:\Users\chaitanyakumar.svs\source\repos\ClassLibrary1\ClassLibrary1\bin\Debug\Thermo.Chromeleon.Sdk.Implementation.dll");
                }
                catch (Exception ex)
                {
                    File.WriteAllText(@"C:\TestService1\EXCEPTION.txt", "Installation " + ex.ToString());
                }
            }

            string exeName = "CMATLTServiceTrayApp.exe"; // without path
            foreach (var proc in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(exeName)))
            {
                try
                {
                    proc.Kill();
                    proc.WaitForExit();
                    Console.WriteLine($"{exeName} closed successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error closing process: " + ex.Message);
                }
            }

        }

        public static class GacHelper
        {
            public static void Install(string assemblyPath)
            {
                Publish publish = new Publish();
                publish.GacInstall(assemblyPath);
            }
            public static void Uninstall(string assemblyPath)
            {
                Publish publish = new Publish();
                publish.GacRemove(assemblyPath);
            }
        }
    }
}
