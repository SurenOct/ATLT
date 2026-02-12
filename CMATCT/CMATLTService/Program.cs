using System.ServiceProcess;
#if TROUBLESHOOTING
using System;
using System.Threading;
#else
using System.ServiceProcess;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endif

namespace CMATLTService
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if TROUBLESHOOTING
            bool bIsTroubleshooting = true;
            // Run as console app in DEBUG for quicker debugging
            Console.WriteLine("Starting service in TROUBLESHOOTING mode...");

            var svc = new TroubleShootingService();
            svc.StartForDebugging();

            Console.WriteLine("Service is running. Press Ctrl+C or Enter to exit...");
            while (bIsTroubleshooting)
            {
                System.Threading.Thread.Sleep(100000);
            }
            // Set up Ctrl+C handler
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                Console.WriteLine("\nShutdown requested...");
                svc.StopForDebugging();
            };

            // Wait on a separate thread so Console.ReadLine doesn't block
            var readLineThread = new Thread(() =>
            {
                Console.ReadLine();
                Console.WriteLine("Shutting down...");
                svc.StopForDebugging();
            });
            readLineThread.IsBackground = false;
            readLineThread.Start();

            // Wait for service to signal shutdown
            svc.WaitForShutdown();

            Console.WriteLine("Service stopped. Press Enter to close...");
            Console.ReadLine();
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new ATLTService()
            };
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
