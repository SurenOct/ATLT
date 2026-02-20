using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using System.Windows;
using CMATCTUI.Models;
using CMATLTLoger;
using Newtonsoft.Json;

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
            LogLibrary.LogUIInfo("DashBoardViewModel initialization started");
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
                LogLibrary.LogUIInfo("Data folder does not Exist, creating it now" + destinationFolder);
                //MessageBox.Show("Data folder does not Exist");
                DirectoryInfo info = Directory.CreateDirectory(destinationFolder);
                if (info.Exists)
                {
                    LogLibrary.LogUIInfo("Data folder created successfully at: " + destinationFolder);
                }
                else
                {
                    LogLibrary.LogUIError("Failed to create Data folder at: " + destinationFolder + "Try to create it manually and start Service again.");
                    return;
                }
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
                LogLibrary.LogUIInfo("Files with today's date found: " + filesWithDate.Count());
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
            else
            {
                LogLibrary.LogUIInfo("No files with today's date found in: " + destinationFolder);

            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


    }
}
