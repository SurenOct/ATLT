using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CMATLTLoger;
using Newtonsoft.Json;
using Thermo.Chromeleon.Sdk.Common;
using Thermo.Chromeleon.Sdk.Interfaces.Data;
using Thermo.Chromeleon.Sdk.Interfaces.Instruments;
using Thermo.Chromeleon.Sdk.Interfaces.Instruments.Collections;

namespace CMATLTService
{

#if TROUBLESHOOTING
    public partial class TroubleShootingService : ATLTService
    {
        private ManualResetEventSlim _debugStopEvent;

        public void StartForDebugging()
        {
            try
            {
                _debugStopEvent = new ManualResetEventSlim(false);
                //_cmSdkScope = new CmSdkScope();
                using (new CmSdkScope())
                {
                    CmSdk.Logon.DoLogon();
                    if (CmSdk.Logon.IsLoggedOn)
                    {
                        TriggerActionFlow();
                        Task.Run(() => ListenForTrigger());
                        LogLibrary.Log("Service started. Listening for Trigger to start collecting logs.");
                        Console.WriteLine("Service started successfully in debug mode.");
                    }
                    else
                    {
                        Console.WriteLine("Failed to log on to CmSdk.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogLibrary.LogError("Service start error: " + ex.Message);
                Console.WriteLine($"Error starting service: {ex.Message}");
            }
        }

        public void WaitForShutdown()
        {
            _debugStopEvent?.Wait();
        }

        public void StopForDebugging()
        {
            Console.WriteLine("Stopping service...");

            _timer?.Dispose();

            // Wait for semaphore
            _semaphore?.Wait(TimeSpan.FromSeconds(5));
            _semaphore?.Dispose();

            //_cmSdkScope?.Dispose();

            _debugStopEvent?.Set();
            _debugStopEvent?.Dispose();

            Console.WriteLine("Service stopped.");
        }
    }
#endif
    public partial class ATLTService : ServiceBase
    {
        int schedularTime = Convert.ToInt32(ConfigurationManager.AppSettings["ThreadTime"]);

        //private string serviceLogPath;
        //private Thread logThread;
        //private bool keepRunning = true;
        //private Thread workerThread;
        //private bool stopRequested = false;
        //string msiPath;
        //string installLogDirectory;
        //string installLogPath;
#if TROUBLESHOOTING
        protected Timer _timer;
#else
        private Timer _timer;

#endif
#if TROUBLESHOOTING

        protected readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

#else
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
#endif

        public CmSdkScope cmSdkScope { get; private set; }
        public ATLTService()
        {

            //LogLibrary.LogError("ATLTService Start intialise: ");
            InitializeComponent();
            //LogLibrary.LogError("After ATLTService Start intialise: ");
        }

        protected override void OnStart(string[] args)
        {

            //LogLibrary.LogError("OnStart: ");
            Thread.Sleep(TimeSpan.FromMinutes(1));
            try
            {
                // using (new CmSdkScope())
                if (cmSdkScope == null)
                    cmSdkScope = new CmSdkScope();
                //  {
                // LogLibrary.LogError("CmSdkScope: ");
                CmSdk.Logon.DoLogon();
                if (CmSdk.Logon.IsLoggedOn)
                {
                    //  LogLibrary.LogUIInfo("CM ATLT Service Logged On Success ");
                    TriggerActionFlow();
                    //LogLibrary.LogUIInfo("CM ATLT Service Started Task for fetching log ");
                    Task.Run(() => ListenForTrigger());
                    //LogLibrary.LogUIInfo("CM ATLT Service Started to listen for UI Commands ");
                }
                else
                {
                    LogLibrary.LogError("Failed to IsLoggedOn to CmSdk may be chromeleon services are stopped. Date is:" + DateTime.Now);
                }
                //}

            }
            catch (Exception ex)
            {
                var asm = typeof(CmSdkScope).Assembly;
                LogLibrary.LogError("CmSdkScope GAC Name:" + asm.GlobalAssemblyCache.ToString() + ", Location=" + asm.Location);
                LogLibrary.LogError("Service start error: " + ex.Message);
            }
            var assembly = typeof(CmSdkScope).Assembly;
            LogLibrary.LogError("CmSdkScope GAC Name:" + assembly.GlobalAssemblyCache.ToString() + ", Location=" + assembly.Location);
            LogLibrary.LogService("CM ATLT Service started...");
            //workerThread = new Thread(new ThreadStart(WorkerLoop));
            // Library.WriteErrorLog("Test window service started");
            //workerThread.IsBackground = true;   // service will stop even if thread is running
            //workerThread.Start();

        }

#if TROUBLESHOOTING
        public void TriggerActionFlow()
#else
        private void TriggerActionFlow()
#endif
        {
            LogLibrary.LogService("log Created : " + DateTime.Now.ToString());
            _timer = new Timer(
            async _ => await ExecuteTaskAsync(),
            null,
            TimeSpan.Zero,
            TimeSpan.FromMinutes(2));
        }

        private async Task ExecuteTaskAsync()
        {
            //if (!await _semaphore.WaitAsync(0))
            //    return;

            await _semaphore.WaitAsync();
            try
            {
                await DoWorkAsync();


            }
            finally
            {
                _semaphore.Release();
            }

        }

        private async Task DoWorkAsync()
        {
            if (cmSdkScope == null)
                cmSdkScope = new CmSdkScope();
            //using (new CmSdkScope())
            //{
            CmSdk.Logon.DoLogon();
            if (CmSdk.Logon.IsLoggedOn)
            {

                LogLibrary.LogService("CM ATLT Service trying to fetch CM Data..." + DateTime.Now);
                // LogLibrary.WriteErrorLog("log writing : " + DateTime.Now.ToString());

                try
                {
                    foreach (IInstrument instr in GetInstrument())
                    {
                        //string sequenceUri = null;
                        //string sequenceUri1 = instr?
                        //                     .QueueControl?
                        //                     .QueueItems?
                        //                     .FirstOrDefault()?
                        //                     .SequenceUri?
                        //                     .OriginalString;


                        LogLibrary.LogService("Instruments Loop Entered." + DateTime.Now);
                        //iinstrument instr = (iinstrument)getinstrument();
                        IInstrument selectedinstument = instr;
                        try
                        {
                            LogLibrary.LogService("Selected Instrument is :" + selectedinstument + ". Date is:" + DateTime.Now);
                            bool result = await ConnectingToInstumentasync(selectedinstument);
                            LogLibrary.LogService("Instrument Status :" + result + ". Date is:" + DateTime.Now);
                            if (result)
                            {
                                //WriteOnlineAuditDataDuplicateintoFile(selectedinstument.OnlineAuditTrail, selectedinstument.Name);
                                LogLibrary.LogService("Instrument Status :" + result + ". Date is:" + DateTime.Now);
                                //messagebox.show("connected successfully to " + selectedinstument.name);
                                LogLibrary.LogService("Ready to Fetching AuditTrails Data :" + DateTime.Now);
                                //FetchOnlineAuditrailsData(selectedinstument, selectedinstument.Name);

                                if (selectedinstument.OnlineAuditTrail.Count() > 0)
                                {

                                    FetchOnlineAuditrailsData(selectedinstument, selectedinstument.Name);

                                    //sequenceUri = instr.QueueControl.QueueItems.First().SequenceUri.OriginalString.ToString();



                                    //ExportSequenceData(sequenceUri, selectedinstument.Name);


                                }
                                else
                                {
                                    LogLibrary.LogService("AuditTrails Data Count is --:" + selectedinstument.OnlineAuditTrail.Count() + "And" + DateTime.Now);
                                }
                            }
                            else
                            {
                                LogLibrary.LogService("Failed to connect to Instrument :" + selectedinstument + ". Date is:" + DateTime.Now);
                            }

                        }
                        catch (Exception ex)
                        {

                            LogLibrary.LogError("Error in processing instrument " + selectedinstument.Name + ": " + ex.Message);
                        }

                    }

                    //ExportSequenceData("chrom://tf-251216232713/ChromeleonLocal/Instrument Data/MyInuvion/SeqInuvion.seq",null);
                }
                catch (Exception ex)
                {

                    LogLibrary.LogError("Error in DoWorkAsync: " + ex.Message);
                }


            }
            // }
            // Your logic here
            await Task.Delay(2000);
        }

        /// <summary>
        /// Export Sequence Data
        /// </summary>
        /// <param name="sequenceUri"></param>

        private void ExportSequenceData(string sequenceUri, string instrName)
        {
            try
            {
                if (string.IsNullOrEmpty(sequenceUri))
                {
                    LogLibrary.LogService("Sequence URI is null or empty. Date is:" + DateTime.Now);
                    return;
                }

                if (string.IsNullOrEmpty(instrName))
                {
                    LogLibrary.LogService("Instrument Name is null or empty. Date is:" + DateTime.Now);
                    return;
                }


                string seqName = sequenceUri.Split('/').LastOrDefault().Split('.')[0];
                LogLibrary.LogService("Ready to Writing Audit information to file:" + DateTime.Now);
                string destinationFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "CMATLTData", "CmbxFiles");

                // Create the folder (creates all parent folders if missing)
                Directory.CreateDirectory(destinationFolder);
                if (!Directory.Exists(destinationFolder))
                {
                    LogLibrary.LogService("Directory : " + destinationFolder + " do not exist. Date is:" + DateTime.Now);
                    return;
                }
                foreach (string file in Directory.GetFiles(destinationFolder))
                {
                    File.Delete(file);
                }

                //string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
                //string backupFile = Path.Combine(destinationFolder, $"{instrName + "_" + timestamp}.json");

                string dateString = DateTime.Now.ToString("yyyy-MM-dd");
                string backupFile = Path.Combine(destinationFolder, $"{seqName}_{dateString}.cmbx");

                //using (new CmSdkScope())
                //{
                if (cmSdkScope == null)
                    cmSdkScope = new CmSdkScope();
                CmSdk.Logon.DoLogon();
                if (CmSdk.Logon.IsLoggedOn)
                {
                    CmSdk.Logon.DoLogon();
                    if (CmSdk.Logon.IsLoggedOn)
                    {
                        var exportEngine = CmSdk.GetExportEngine();
                        var tokenSource = new CancellationTokenSource();
                        //var targetFileName = @"C:\Demo\test.cmbx";
                        var targetFileName = backupFile;
                        var progress = new Progress<double>();
                        var itemsToExport = new List<Uri> { new Uri(sequenceUri) };
                        // Create and start a new export task
                        var importTask = exportEngine.ExportToCmbxAsync(
                            itemsToExport, // The items which will be exported
                            targetFileName, // The target filename
                            "Just a short description",
                            CmbxExportOptions.IncludeAllData, // Exporting options
                            CmbxContainerFormat.V2, // Container format (currently there are two different formats)
                            progress, // Progress information
                            tokenSource.Token // Cancellation token (in order to cancel the export operation)
                            );

                        // Wait for task completion and continue in the current synchronization 
                        // context (useful for UI threads)
                        importTask.ContinueWith(t =>
                        {
                            if (t.IsFaulted)
                            {
                                // Faulted with exception
                                Exception ex = t.Exception;
                                while (ex is AggregateException && ex.InnerException != null)
                                    ex = ex.InnerException;

                            }
                            else if (t.IsCanceled)
                            {

                            }
                            else
                            {

                            }
                        });
                    }

                }
                //  }
            }
            catch (Exception ex)
            {
                LogLibrary.LogError("Error in ExportSequenceData: " + ex.Message);
                return;
            }
        }


        /// <summary>
        /// Fetch Online Auditrails Data
        /// </summary>
        /// <param name="selectedinstument"></param>
        /// <param name="name"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void FetchOnlineAuditrailsData(IInstrument selectedInstument, string instrumentName)
        {
            LogLibrary.LogService("AuditTrails Data Count is :" + selectedInstument.OnlineAuditTrail.Count() + "And Date is:" + DateTime.Now);
            string seqName = Convert.ToString(selectedInstument.QueueControl.QueueItems.First().SequenceUri.OriginalString);
            WriteOnlineAuditDataintoFile(selectedInstument.OnlineAuditTrail, instrumentName, seqName);

        }
        private void WriteOnlineAuditDataDuplicateintoFile(IAuditTrailMessages onlineAuditTrail, string instrName)
        {
            string destinationFolder = @"C:\BackupFolder\";
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
            string backupFile = Path.Combine(destinationFolder, $"{instrName + "_" + timestamp}.json");

            var jsonEntities = new List<Dictionary<string, object>>();
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                StringEscapeHandling = StringEscapeHandling.Default
            };
            foreach (var line in onlineAuditTrail)
            {
                //string[] values = line.Split(new[] { "\t" }, StringSplitOptions.None);

                var jsonEntity = new Dictionary<string, object>();
                jsonEntity["ID"] = line.Id;
                jsonEntity["Category"] = line.Category;
                jsonEntity["Device"] = line.Device;
                jsonEntity["Level"] = line.Level;
                jsonEntity["Message"] = line.Message;
                jsonEntity["RetentionTime"] = line.RetentionTime;
                jsonEntity["Time"] = line.Time;

                jsonEntities.Add(jsonEntity);
            }
            string json = JsonConvert.SerializeObject(jsonEntities, settings);

            File.WriteAllText(backupFile, json, Encoding.UTF8);
        }
        private void WriteOnlineAuditDataintoFile(IAuditTrailMessages onlineAuditTrail, string instrName, string seqName)
        {
            try
            {
                LogLibrary.LogService("Ready to Writing Audit information to file:" + DateTime.Now);
                string destinationFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "CMATLTData", "Data");

                // Create the folder (creates all parent folders if missing)
                Directory.CreateDirectory(destinationFolder);

                //string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
                //string backupFile = Path.Combine(destinationFolder, $"{instrName + "_" + timestamp}.json");

                string dateString = DateTime.Now.ToString("yyyy-MM-dd");
                string backupFile = Path.Combine(destinationFolder, $"{instrName}_{dateString}.json");
                var jsonEntities = new List<Dictionary<string, object>>();
                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    StringEscapeHandling = StringEscapeHandling.Default
                };
                var items = onlineAuditTrail.Where(a => a.Time.DateTime.Date == DateTime.Now.Date);
                //foreach (var line in onlineAuditTrail)
                foreach (var line in items)
                {
                    //string[] values = line.Split(new[] { "\t" }, StringSplitOptions.None);
                    ;
                    var jsonEntity = new Dictionary<string, object>();
                    jsonEntity["ID"] = line.Id;
                    if (Enum.TryParse<AuditTrailMessageCategory>(Convert.ToString(line.Category), true, out var category))
                    {
                        // Store the enum value as string in JSON
                        jsonEntity["Category"] = category.ToString();
                    }
                    jsonEntity["Device"] = line.Device;
                    jsonEntity["Level"] = line.Level;
                    jsonEntity["Message"] = line.Message;
                    jsonEntity["RetentionTime"] = line.RetentionTime;
                    jsonEntity["Time"] = line.Time;
                    jsonEntity["SequenceName"] = seqName;


                    jsonEntities.Add(jsonEntity);
                }
                string json = JsonConvert.SerializeObject(jsonEntities, settings);

                File.WriteAllText(backupFile, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Connect to Instrument Asynchronously
        /// </summary>
        /// <param name="selectedinstument"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task<bool> ConnectingToInstumentasync(IInstrument selectedinstument)
        {
            //using (new CmSdkScope())
            //{
            //CmSdk.Logon.DoLogon();
            //if (CmSdk.Logon.IsLoggedOn)
            //{
            LogLibrary.LogService("Connecting To Instrument:" + selectedinstument + ". Date:" + DateTime.Now);
            Progress result = await selectedinstument.ConnectAsync();
            LogLibrary.LogService("Instrument Status is:" + result + ". Date:" + DateTime.Now);
            return result == Progress.Completed;
            // }
            // }
            // return false;
        }

        /// <summary>
        /// Get Instrument Details from Chromeleon
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IInstrument> GetInstrument()
        {
            //using (new CmSdkScope())
            //{
            //CmSdk.Logon.DoLogon();
            //if (CmSdk.Logon.IsLoggedOn)
            //{
            LogLibrary.LogService("Fetching Instruments Details : " + DateTime.Now);
            var items = Thermo.Chromeleon.Sdk.Common.CmSdk.GetInstrumentAccess();
            LogLibrary.LogService("Fetched Instruments Details count is: " + items.Instruments.Count() + ". Date is:" + DateTime.Now);
            return items.Instruments;
            // }
            //}
            return Enumerable.Empty<IInstrument>();
        }

        /// <summary>
        /// // Public method to trigger the task manually
        /// </summary>
        public void TriggerNow()
        {
            Task.Run(() => ExecuteTaskAsync());
        }

#if TROUBLESHOOTING
        public void ListenForTrigger()
#else
        private void ListenForTrigger()
#endif
        {
            //var server = new System.IO.Pipes.NamedPipeServerStream("CMATLTService", System.IO.Pipes.PipeDirection.In);

            //while (true)
            //{
            //    server.WaitForConnection();
            //    var reader = new System.IO.StreamReader(server);
            //    var command = reader.ReadLine();

            //    if (command?.Equals("RUN_NOW", StringComparison.OrdinalIgnoreCase) == true)
            //    {
            //        TriggerNow(); // Call the same task on-demand
            //    }

            //    server.Disconnect();
            //}


            //PipeSecurity pipeSecurity = new PipeSecurity();

            //// Allow ALL users (installed apps run as Interactive user)
            //pipeSecurity.AddAccessRule(
            //    new PipeAccessRule(
            //        new SecurityIdentifier(WellKnownSidType.WorldSid, null),
            //        PipeAccessRights.ReadWrite,
            //        AccessControlType.Allow));

            //var server = new NamedPipeServerStream(
            //    "CMATLTService",
            //    PipeDirection.InOut,
            //    1,
            //    PipeTransmissionMode.Message,
            //    PipeOptions.None,
            //    4096,
            //    4096,
            //    pipeSecurity);

            //server.WaitForConnection();



            PipeSecurity pipeSecurity = new PipeSecurity();
            pipeSecurity.AddAccessRule(
                new PipeAccessRule(
                    new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                    PipeAccessRights.ReadWrite,
                    AccessControlType.Allow));

            while (true)
            {
                using (var server = new NamedPipeServerStream(
                    "CMATLTService",
                    PipeDirection.InOut,
                    1,
                    PipeTransmissionMode.Message,
                    PipeOptions.None,
                    4096,
                    4096,
                    pipeSecurity))
                {
                    server.WaitForConnection();

                    using (var reader = new StreamReader(server))
                    {
                        var command = reader.ReadLine();
                        var parts = command
                                   .Split(new[] { "&" }, StringSplitOptions.RemoveEmptyEntries)
                                   .Select(p => p.Trim())
                                   .ToArray();
                        var action = parts[0];
                        if (action.Equals("RUN_NOW", StringComparison.OrdinalIgnoreCase) == true)
                        {
                            TriggerNow();
                        }
                        else if (action.Equals("EXPORT_CMBX", StringComparison.OrdinalIgnoreCase) == true)
                        {
                            var seqName = parts[1];
                            TriggerCmbxFile(seqName);
                        }
                    }
                }
            }




        }

        public void TriggerCmbxFile(string seqName)
        {
            // ExportSequenceData("chrom://tf-251216232713/ChromeleonLocal/Instrument Data/MyInuvion/SeqInuvion.seq", null);
            ExportSequenceData(seqName, null);
        }

        public void ListenForTrigger1()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:5050/trigger/");
            listener.Start();

            while (true)
            {
                var context = listener.GetContext();
                var reader = new StreamReader(context.Request.InputStream);
                var command = reader.ReadToEnd();

                if (command == "RUN_NOW")
                {

                }

                context.Response.StatusCode = 200;
                context.Response.Close();
            }

        }





        //private void WorkerLoop()
        //{
        //    while (!stopRequested)
        //    {
        //        try
        //        {
        //            DoWork();
        //        }
        //        catch (Exception ex)
        //        {
        //            LogLibrary.WriteErrorLog("Error: " + ex.Message);
        //        }

        //        // EXACT 5 MINUTES
        //        Thread.Sleep(schedularTime * 60 * 1000);
        //    }
        //}

        //private void DoWork()
        //{
        //    using (var scope = new Thermo.Chromeleon.Sdk.Common.CmSdkScope())
        //    {
        //        Thermo.Chromeleon.Sdk.Common.CmSdk.Logon.DoLogon();
        //        var itemFactory = Thermo.Chromeleon.Sdk.Common.CmSdk.GetItemFactory();
        //        string computerName = Environment.MachineName;

        //        Thermo.Chromeleon.Sdk.Interfaces.Data.IDataVaultServer server;

        //        if (itemFactory.TryGetItem(new Uri($"chrom://{computerName}/"), out server))
        //        {
        //            foreach (Thermo.Chromeleon.Sdk.Interfaces.Data.IDataVault dataVault in server.DataVaults)
        //            {
        //                LogLibrary.WriteErrorLog("Service is working fine at dataVault.Name: " + dataVault.Name.ToString() + DateTime.Now.ToString());
        //            }
        //        }
        //    }

        //    LogLibrary.WriteErrorLog("Service is working fine at " + DateTime.Now.ToString());

        //}

        protected override void OnStop()
        {
            _timer?.Dispose();
            _semaphore?.Dispose();
        }
    }
}
