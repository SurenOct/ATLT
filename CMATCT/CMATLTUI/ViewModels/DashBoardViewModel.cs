using CMATCTUI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CMATCTUI.ViewModels
{
    public class DashBoardViewModel : INotifyPropertyChanged
    {
        private int _numberOfInstruments;
        public int NumberOfInstruments
        {
            get => _numberOfInstruments;
            set
            {
                if (_numberOfInstruments != value)
                {
                    _numberOfInstruments = value;
                    OnPropertyChanged(nameof(NumberOfInstruments));
                }
            }
        }
        private int _numberOfWarnings;
        public int NumberOfWarnings
        {
            get => _numberOfWarnings;
            set
            {
                if (_numberOfWarnings != value)
                {
                    _numberOfWarnings = value;
                    OnPropertyChanged(nameof(NumberOfWarnings));
                }
            }
        }

        private int _numberOfErrors;
        public int NumberOfErrors
        {
            get => _numberOfErrors;
            set
            {
                if (_numberOfErrors != value)
                {
                    _numberOfErrors = value;
                    OnPropertyChanged(nameof(NumberOfErrors));
                }
            }
        }

        private int _numberOfInformations;
        public int NumberOfInformations
        {
            get => _numberOfInformations;
            set
            {
                if (_numberOfInformations != value)
                {
                    _numberOfInformations = value;
                    OnPropertyChanged(nameof(NumberOfInformations));
                }
            }
        }
        private int _TotalAuditTrailsCount;
        public int TotalAuditTrailsCount
        {
            get => _TotalAuditTrailsCount;
            set
            {
                if (_TotalAuditTrailsCount != value)
                {
                    _TotalAuditTrailsCount = value;
                    OnPropertyChanged(nameof(TotalAuditTrailsCount));
                }
            }
        }

        private string _lastUpdatedFileDateAndTime;
        public string LastUpdatedFileDateAndTime
        {
            get => _lastUpdatedFileDateAndTime;
            set
            {
                if (_lastUpdatedFileDateAndTime != value)
                {
                    _lastUpdatedFileDateAndTime = value;
                    OnPropertyChanged(nameof(LastUpdatedFileDateAndTime));
                }
            }
        }
        private Visibility _isServiceStatus = Visibility.Collapsed;
        public Visibility IsServiceStatus
        {
            get => _isServiceStatus;
            set
            {
                if (_isServiceStatus != value)
                {
                    _isServiceStatus = value;
                    OnPropertyChanged(nameof(IsServiceStatus));
                }
            }
        }


        public DashBoardViewModel()
        {

            LoadingAuditLogFilesAndLogsCount();
        }

        /// <summary>
        /// Loading Audit Log Files And Logs Count
        /// </summary>
        private void LoadingAuditLogFilesAndLogsCount()
        {
            string serviceName = "CMATLTService";
            ServiceController sc = new ServiceController(serviceName);
            if (sc.Status == ServiceControllerStatus.Running)
            {
                IsServiceStatus = Visibility.Collapsed;
            }
            else
            {
                IsServiceStatus = Visibility.Visible;
            }

            string destinationFolder = System.IO.Path.Combine(
              Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
              "CMATLTData", "Data");

            string logFolder = System.IO.Path.Combine(
              Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
              "CMATLTData", "Logs");

            //  string[] files = Directory.GetFiles(destinationFolder);
            string[] logFiles = Directory.GetFiles(logFolder);
            //var filesWithDate = Directory.GetFiles(logFolder)
            //            .Select(filePath => new FileInfo(filePath))
            //            .Select(f => new
            //            {
            //                FileName = f.Name,
            //                FullPath = f.FullName,
            //                CreatedOn = f.CreationTime
            //            })
            //            .OrderByDescending(f => f.CreatedOn)
            //                 .ToList();

            if (!Directory.Exists(destinationFolder))
            {
                MessageBox.Show("Data folder is not Exist");
                return;
            }

            var filesWithDate = Directory.GetFiles(destinationFolder)
                .Select(filePath => new FileInfo(filePath))
                .Where(f => f.CreationTime.Date == DateTime.Today)
                .Select(f => new
                {
                    FileName = f.Name,
                    FullPath = f.FullName,
                    CreatedOn = f.CreationTime
                })
                .ToList();
            if (filesWithDate.Count() > 0)
            {
                LastUpdatedFileDateAndTime = "  |  Last updated:" + filesWithDate.First().CreatedOn.ToString();

            }


            if (filesWithDate.Count() > 0)
            {
                foreach (var filepath in filesWithDate)
                {
                    var jsonString = System.IO.File.ReadAllText(filepath.FullPath);

                    var criticalWarnings = JsonConvert.DeserializeObject<ObservableCollection<AuditLogItem>>(jsonString);
                    NumberOfInformations = NumberOfInformations + criticalWarnings.Where(a => a.Category.ToString().Contains("Information")).Count();
                    NumberOfWarnings = NumberOfWarnings + criticalWarnings.Where(a => a.Category.Contains("Warning")).Count();
                    NumberOfErrors = NumberOfErrors + criticalWarnings.Where(a => a.Category.Contains("Errors")).Count();
                }
                TotalAuditTrailsCount = NumberOfInformations + NumberOfWarnings + NumberOfErrors;
                NumberOfInstruments = filesWithDate.Count();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


    }
}
