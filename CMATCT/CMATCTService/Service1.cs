using System;
using System.Configuration;
using System.ServiceProcess;
using System.Threading;
using CMATCTLoger;

namespace CMATCTService
{
    public partial class Service1 : ServiceBase
    {
        int schedularTime = Convert.ToInt32(ConfigurationManager.AppSettings["ThreadTime"]);

        //private string serviceLogPath;
        //private Thread logThread;
        //private bool keepRunning = true;
        private Thread workerThread;
        private bool stopRequested = false;
        //string msiPath;
        //string installLogDirectory;
        //string installLogPath;
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            LogLibrary.Log("Service started.");
            workerThread = new Thread(new ThreadStart(WorkerLoop));
            // Library.WriteErrorLog("Test window service started");
            workerThread.IsBackground = true;   // service will stop even if thread is running
            workerThread.Start();


        }
        private void WorkerLoop()
        {
            while (!stopRequested)
            {
                try
                {
                    DoWork();
                }
                catch (Exception ex)
                {
                    LogLibrary.WriteErrorLog("Error: " + ex.Message);
                }

                // EXACT 5 MINUTES
                Thread.Sleep(schedularTime * 60 * 1000);
            }
        }

        private void DoWork()
        {
            using (var scope = new Thermo.Chromeleon.Sdk.Common.CmSdkScope())
            {
                Thermo.Chromeleon.Sdk.Common.CmSdk.Logon.DoLogon();
                var itemFactory = Thermo.Chromeleon.Sdk.Common.CmSdk.GetItemFactory();
                string computerName = Environment.MachineName;

                Thermo.Chromeleon.Sdk.Interfaces.Data.IDataVaultServer server;

                if (itemFactory.TryGetItem(new Uri($"chrom://{computerName}/"), out server))
                {
                    foreach (Thermo.Chromeleon.Sdk.Interfaces.Data.IDataVault dataVault in server.DataVaults)
                    {
                        LogLibrary.WriteErrorLog("Service is working fine at dataVault.Name: " + dataVault.Name.ToString() + DateTime.Now.ToString());
                    }
                }
            }

            LogLibrary.WriteErrorLog("Service is working fine at " + DateTime.Now.ToString());

        }

        protected override void OnStop()
        {
        }
    }
}
