using CMATCTUI.Commands;
using CMATCTUI.Generic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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
            var client = new NamedPipeClientStream(".", "CMATLTService", PipeDirection.Out);
            client.Connect(2000); // Wait max 2 sec

            var writer = new StreamWriter(client) { AutoFlush = true };
            writer.WriteLine("RUN_NOW");

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
                    }
                }
            }
        }

        /// <summary>
        /// Service Time Updated
        /// </summary>
        private void ServiceTimeUpdated()
        {
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
                MessageBox.Show("Please enter valid service time");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    }
}
