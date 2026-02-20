using CMATCTUI.Commands;
using CMATCTUI.Generic;
using CMATLTLoger;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace CMATCTUI.ViewModels
{
    internal class SettingsViewModel : INotifyPropertyChanged
    {

        public ICommand ServiceUpdatedCommand { get; }

        public ICommand ServiceTriggerCommand { get; }

        private int _serviceTime;
        public int ServiceTime
        {
            get => _serviceTime;
            set
            {
                if (_serviceTime != value)
                {
                    _serviceTime = value;
                    if (value < 0)
                    {
                        _serviceTime = 0;
                    }
                    else
                    {
                        _serviceTime = value;
                    }

                    OnPropertyChanged(nameof(ServiceTime));
                }
            }
        }
        public XDocument doc { get; set; }
        public XElement root { get; set; }
        //private Visibility _timeBasedValue = Visibility.Visible;
        //public Visibility TimeBasedValue
        //{
        //    get => _timeBasedValue;
        //    set
        //    {
        //        if (_timeBasedValue != value)
        //        {
        //            _timeBasedValue = value;
        //            OnPropertyChanged(nameof(TimeBasedValue));
        //        }
        //    }
        //}

        public SettingsViewModel()
        {
            LogLibrary.LogUIInfo("SettingsViewModel Initialized");
            ServiceUpdatedCommand = new RelayCommand(ServiceTimeUpdated);
            ServiceTriggerCommand=new RelayCommand(ServiceTrigger);
            LoadCurrentScanTimeDetails();
        }
        /// <summary>
        /// Service Trigger Method
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void ServiceTrigger()
        {
            if (!CMServicesStatusList())
            {
                LogLibrary.LogUIError("One or more CM services are not running.");
                MessageBox.Show("One or more CM services are not running.");
                return;
            }

            //var client = new NamedPipeClientStream(".", "CMATLTService", PipeDirection.Out);
            //client.Connect(2000); // Wait max 2 sec

            //var writer = new StreamWriter(client) { AutoFlush = true };
            //writer.WriteLine("RUN_NOW");
            try
            {
                //var client = new NamedPipeClientStream(".", "CMATLTService", PipeDirection.Out);
                //MessageBox.Show("Command sent");
                //client.Connect(2000); // Wait max 2 sec
                //MessageBox.Show("Command coonected");
                //var writer = new StreamWriter(client) { AutoFlush = true };
                //writer.WriteLine("RUN_NOW");


                 var client = new NamedPipeClientStream(
    ".",
    "CMATLTService",
    PipeDirection.InOut
);
                
                client.Connect(5000);
                
                var writer = new StreamWriter(client) { AutoFlush = true };
                writer.WriteLine("RUN_NOW");


                MessageBox.Show("Command sent");
                                LogLibrary.LogUIInfo("Command sent to CMATLTService");
            }
            catch(Exception ex)
            {
                LogLibrary.LogUIError("Failed to send command: " + ex.Message);
                MessageBox.Show("Failed to send command: " + ex.Message);
            }

        }

        /// <summary>
        /// Current Scan Time Details
        /// </summary>
        private void LoadCurrentScanTimeDetails()
        {
            //ServiceTime
                
            string destinationFolderTimeXml = System.IO.Path.Combine(
              Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
              "CMATLTData");
            string[] timeFiles = Directory.GetFiles(destinationFolderTimeXml);
            if (timeFiles.Length > 0 &&
             Path.GetFileNameWithoutExtension(timeFiles[0])
             .Equals("TimeConfigFile", StringComparison.OrdinalIgnoreCase))
            {
                doc = XDocument.Load(timeFiles[0]);
                root = doc.Element("TimeSettings");
                if (root != null)
                {
                    foreach (XElement item in root.Elements("TimeIntervel"))
                    {
                        ServiceTime = int.Parse(item.Value);
                        LogLibrary.LogUIInfo("Loaded service time: " + ServiceTime);
                    }
                }
            }
        }
        /// <summary>
        /// CM Services Status Check
        /// </summary>
        /// <returns></returns>
        private bool CMServicesStatusList()
        {
           LogLibrary.LogUIInfo("Checking CM services status...");
            string[] serviceNames = {
        "ChromeleonLicenseServer",
        "ChromeleonUserServer",
        "ChromeleonDiscoveryServer",
        "ChromeleonDiscoveryLocalCache",
        "ChromeleonDataVaultServer",
        "ChromeleonRawFileServerOutOfProcx86",
        "ChromeleonDataTransferService",
        "ChromeleonSchedulerServer",
        "ChromeleonUpdaterService",
        "ChromeleonCacheService",
        "ChromeleonApplicationProcessingServer",
        "ChromeleonDataProcessingServer",
        "ChromeleonHardwareMonitoringService",
        "ChromeleonInstrumentServer"
    };
            
            foreach (string serviceName in serviceNames)
            {
                try
                {
                    using (ServiceController sc = new ServiceController(serviceName))
                    {
                        if (sc.Status != ServiceControllerStatus.Running)
                        {
                            LogLibrary.LogUIError("Service not running: " + serviceName);
                            return false;   // If any service is not running
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    LogLibrary.LogUIError("Service not found: " + serviceName);
                    // Service not found
                    return false;
                }
            }

            return true; // All services are running
        }
        /// <summary>
        /// Service Time Updated
        /// </summary>
        private void ServiceTimeUpdated()
        {
            LogLibrary.LogUIInfo("Updating service time...");

            if (ServiceTime > 0)
            {
                string destinationFolderTimeXml = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "CMATLTData");
                string filePath = Directory
                    .GetFiles(destinationFolderTimeXml, "TimeConfigFile.xml")
                    .FirstOrDefault();

                XDocument doc = XDocument.Load(filePath);

                XElement timeElement = doc.Root.Element("TimeIntervel");

                if (timeElement != null)
                {
                    timeElement.Value = ServiceTime.ToString();
                    doc.Save(filePath);
                }
            }
            else
            {
                    LogLibrary.LogUIError("Invalid service time entered: " + ServiceTime);
                MessageBox.Show("Please enter valid service time");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    }
}
