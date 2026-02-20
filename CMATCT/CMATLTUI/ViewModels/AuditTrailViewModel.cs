using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using CMATCTUI.ChildWindows;
using CMATCTUI.Commands;
using CMATCTUI.Models;
using CMATLTLoger; // ✅ ADD THIS
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

//Test Commit
namespace CMATCTUI.ViewModels
{
    public class AuditTrailViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<AuditLogItem> _auditErrors;
        public ObservableCollection<AuditLogItem> AuditErrors
        {
            get => _auditErrors;
            set
            {
                if (_auditErrors != value)
                {
                    _auditErrors = value;

                    OnPropertyChanged(nameof(AuditErrors));
                }
            }

        }
        private ObservableCollection<AuditLogItem> _auditCompletedErrors;
        public ObservableCollection<AuditLogItem> AuditCompletedErrors
        {
            get => _auditCompletedErrors;
            set
            {
                if (_auditCompletedErrors != value)
                {
                    _auditCompletedErrors = value;

                    OnPropertyChanged(nameof(AuditCompletedErrors));
                }
            }

        }



        // public ObservableCollection<AuditLogItem> AuditCompletedErrors { get; set; }
        public ICollectionView ErrorsView { get; set; }

        private ObservableCollection<AuditLogItem> _selectedAuditLogs;
        public ObservableCollection<AuditLogItem> SelectedAuditLogs
        {
            get => _selectedAuditLogs;
            set
            {
                if (_selectedAuditLogs != value)
                {
                    _selectedAuditLogs = value;

                    OnPropertyChanged(nameof(SelectedAuditLogs));
                }
            }

        }



        private ObservableCollection<string> _instrumentsDetails;
        public ObservableCollection<string> InstrumentsDetails
        {
            get => _instrumentsDetails;
            set
            {
                _instrumentsDetails = value;
                OnPropertyChanged(nameof(InstrumentsDetails));
            }
        }

        private bool _isButtonEnabled = false;
        public bool IsButtonEnabled
        {
            get => _isButtonEnabled;
            set
            {
                if (_isButtonEnabled != value)
                {
                    _isButtonEnabled = value;
                    OnPropertyChanged(nameof(IsButtonEnabled));
                }
            }
        }
        private bool _isFilterStackEnabled = false;
        public bool IsFilterStackEnabled
        {
            get => _isFilterStackEnabled;
            set
            {
                if (_isFilterStackEnabled != value)
                {
                    _isFilterStackEnabled = value;
                    OnPropertyChanged(nameof(IsFilterStackEnabled));
                }
            }
        }
        private string _selectedInstrument;
        public string SelectedInstrument
        {
            get => _selectedInstrument;
            set
            {
                if (_selectedInstrument != value)
                {
                    _selectedInstrument = value;
                    OnPropertyChanged(nameof(SelectedInstrument));
                    OnInstrumentChanged(value);
                }
            }
        }
        private DateTime _minDate;
        public DateTime MinDate
        {
            get => _minDate;
            set
            {
                if (_minDate != value)
                {
                    _minDate = value;
                    OnPropertyChanged(nameof(MinDate));
                }
            }
        }

        private DateTime? _fromDate;
        public DateTime? FromDate
        {
            get => _fromDate;
            set
            {
                _fromDate = value;
                OnPropertyChanged(nameof(FromDate));
                // ErrorsView.Refresh();
                UpdateDataBasedonDateAndInstrument();
            }
        }

        private void UpdateDataBasedonDateAndInstrument()
        {
            if (this.FromDate != null)
            {
                filesWithDate = Directory.GetFiles(destinationFolder)
                 .Where(f => System.IO.File.GetCreationTime(f).Date == FromDate)
                  .ToList();
                var instrumentsDetails = this.filesWithDate.Where(a => a.ToString().Split('\\').Last().Split('_').First().ToString() == SelectedInstrument).ToList();
                if (instrumentsDetails.Count() > 0)
                {
                    DynamicJsonFileLoading(instrumentsDetails.First(), SelectedInstrument);
                    ReadTicketsRaisedForLogs();
                }
                else
                {
                    AuditErrors = new ObservableCollection<AuditLogItem>();
                }
                //else if (SelectedInstrument == "--ALL--") //Need to Load All Files Data
                //{
                //    SelectedAuditLogs = new ObservableCollection<AuditLogItem>();
                //    AuditErrors = new ObservableCollection<AuditLogItem>();
                //    foreach (var file in filesWithDate)
                //    {
                //        LoadAllJsonData(file);
                //    }


                //    ReadCompletedAuditTrailLogs();
                //}
                ReadTicketsRaisedForLogs();
                // ErrorsView.Refresh();
            }

        }

        private DateTime? _toDate;
        public DateTime? ToDate
        {
            get => _toDate;
            set
            {
                _toDate = value;
                OnPropertyChanged(nameof(ToDate));
                //ErrorsView.Refresh();
            }
        }

        public ICommand CreateTicketCommand { get; }
        //public string[] files = null;
        public List<string> filesWithDate = null;
        public string destinationFolder;
        public AuditTrailViewModel()
        {
            try
            {
                LogLibrary.LogUIInfo("AuditTrailViewModel initialization started");

                SelectedAuditLogs = new ObservableCollection<AuditLogItem>();
                //InstrumentsDetails = new ObservableCollection<string>();
                CreateTicketCommand = new RelayCommand(CreateTicket);


                destinationFolder = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "CMATLTData", "Data");

                //LogLibrary.LogUIDebug($"Destination folder: {destinationFolder}");

                // ✅ CHECK if directory exists first
                if (!Directory.Exists(destinationFolder))
                {
                    IsFilterStackEnabled = false;
                   // LogLibrary.LogUIWarning("Data folder does not exist. No audit trails available.");
                    MessageBox.Show("Data folder does not exist. No audit trails available.",
                                    "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                filesWithDate = Directory.GetFiles(destinationFolder)
                     .Where(f => System.IO.File.GetCreationTime(f).Date == DateTime.Today)
                      .ToList();

                this.FromDate = DateTime.Now.Date;
                MinDate = DateTime.Today.AddDays(-30).Date;

                LogLibrary.LogUIInfo($"Found {filesWithDate.Count} files for today");

                if (filesWithDate.Count <= 0)
                {
                    IsFilterStackEnabled = false;
                    //LogLibrary.LogUIWarning("No data files available for today");
                    MessageBox.Show("Data is Not Available in Database",
                                    "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                IsFilterStackEnabled = true;
                InstrumentsDetails = new ObservableCollection<string>();

                foreach (var file in filesWithDate)
                {
                    var fileName = Path.GetFileName(file);
                    var instrumentName = fileName.Split('_').FirstOrDefault();
                    if (!string.IsNullOrEmpty(instrumentName))
                    {
                        InstrumentsDetails.Add(instrumentName);
                    }
                }

                // InstrumentsDetails.Add("--ALL--");
                InstrumentsDetails = new ObservableCollection<string>(InstrumentsDetails.Distinct());
                LogLibrary.LogUIInfo($"Loaded {InstrumentsDetails.Count} instruments");

                if (InstrumentsDetails.Count > 0)
                {
                    SelectedInstrument = InstrumentsDetails[0];
                   // LogLibrary.LogUIDebug($"Selected instrument: {SelectedInstrument}");
                }

                /* old commented code below , need to remove after testing

                //string path = files[0].ToString();
                //var jsonString = File.ReadAllText(files[0].ToString());

                //string path = files.FirstOrDefault();

                ////DynamicJsonFileLoading(InstrumentsDetails, SelectedInstrument);
                //var jsonString = System.IO.File.ReadAllText(files.LastOrDefault());

                //var items = System.IO.File.ReadAllText(path);
                //Errors = JsonConvert.DeserializeObject<ObservableCollection<AuditLogItem>>(jsonString);
                //DateTimeOffset dto = DateTimeOffset.Parse(Errors[0].Time);
                //foreach (var db in Errors)
                //{
                //    db.Date = DateTimeOffset.Parse(db.Time).Date.ToString("yyyy-MM-dd");
                //    db.Time = DateTimeOffset.Parse(db.Time).Date.ToString(@"hh\:mm\:ss");
                //}
                //Errors.ToList().ForEach(a => a.Status = "New");

                ////AuditCompletedErrors = JsonConvert.DeserializeObject<ObservableCollection<AuditLogItem>>(jsonString);

                //ErrorsView = CollectionViewSource.GetDefaultView(Errors.Where(x => x.Status != "Completed").ToList());
                ////var item = Errors.Take(5).ToList();
                ////AuditCompletedErrors = new ObservableCollection<AuditLogItem>(item);
                ////AuditCompletedErrors.ToList().ForEach(a => a.Status = "Completed");

                //ErrorsView.Filter = FilterErrors;
                //FromDate = null;
                //ToDate = null;

                //old commented code above, need to remove after testing
                  
                 */

                LogLibrary.LogUIInfo("AuditTrailViewModel initialized successfully");
            }
            catch (Exception ex)
            {
                LogLibrary.LogUIError("initializing Audit Trai"+ex.Message);
                MessageBox.Show($"Error initializing Audit Trail: {ex.Message}",
                                "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnInstrumentChanged(string instrumentName)
        {
            if (instrumentName == null)
            {
              //  LogLibrary.LogUIWarning("OnInstrumentChanged called with null instrument name");
                return;
            }

            try
            {
                //LogLibrary.LogUIDebug($"Instrument changed to: {instrumentName}");

                if (this.filesWithDate != null && this.filesWithDate.Count() <= 0)
                {
                    MessageBox.Show("No Files Available for the selected date");
                    return;
                }

                var instrumentsDetails = this.filesWithDate
                    .Where(a => Path.GetFileName(a).Split('_').FirstOrDefault() == instrumentName)
                    .ToList();

                if (instrumentsDetails.Count > 0)
                {
                    LogLibrary.LogUIInfo($"Loading data for instrument: {instrumentName}");
                    DynamicJsonFileLoading(instrumentsDetails.First(), instrumentName);
                    ReadTicketsRaisedForLogs();
                }
                else
                {
                    //LogLibrary.LogUIWarning($"No data found for instrument: {instrumentName}");
                    AuditErrors = new ObservableCollection<AuditLogItem>();
                }

                /* old commented code below , need to remove after testing
                  
                //else if(instrumentName=="--ALL--") //Need to Load All Files Data
                //{
                //    SelectedAuditLogs = new ObservableCollection<AuditLogItem>();
                //    AuditErrors = new ObservableCollection<AuditLogItem>();
                //    foreach (var file in filesWithDate)
                //    {
                //        LoadAllJsonData(file);
                //    }

                //    ReadCompletedAuditTrailLogs();
                //}
                // old commented code above , need to remove after testing
                  
                 */

            }
            catch (Exception ex)
            {
                LogLibrary.LogUIError($"Error in OnInstrumentChanged: {ex.Message}");
                
            }
        }


        public void DynamicJsonFileLoading(string instrumentsFilePath, string selectedInstrument)
        {
            try
            {
                //LogLibrary.LogUIDebug($"Loading JSON file: {instrumentsFilePath}");

                SelectedAuditLogs = new ObservableCollection<AuditLogItem>();
                AuditErrors = new ObservableCollection<AuditLogItem>();
                LoadJsonData(instrumentsFilePath);
                ErrorsView = CollectionViewSource.GetDefaultView(AuditErrors);

                LogLibrary.LogUIInfo($"Loaded {AuditErrors.Count} audit records for {selectedInstrument}");

                /* old commented code below , need to remove after testing
                 
                //var item = Errors.Take(5).ToList();
                //AuditCompletedErrors = new ObservableCollection<AuditLogItem>(item);
                //AuditCompletedErrors.ToList().ForEach(a => a.Status = "Completed");

                //ErrorsView.Filter = FilterErrors;
                //FromDate = null;
                //ToDate = null;
                // old commented code above , need to remove after testing
                  
                 */
            }
            catch (Exception ex)
            {
                LogLibrary.LogUIError($"Error in DynamicJsonFileLoading: {ex.Message}");
                //LogLibrary.LogUIError(ex);
                MessageBox.Show($"Error loading audit data: {ex.Message}",
                                "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadJsonData(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                   LogLibrary.LogUIError($"File not found: {filePath}");
                    MessageBox.Show($"File not found: {filePath}",
                                    "File Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

               // LogLibrary.LogUIDebug($"Reading JSON from: {filePath}");
                var jsonString = System.IO.File.ReadAllText(filePath);

                AuditErrors = JsonConvert.DeserializeObject<ObservableCollection<AuditLogItem>>(jsonString);

                if (AuditErrors == null || AuditErrors.Count == 0)
                {
                  //  LogLibrary.LogUIWarning($"No data found in file: {filePath}");
                    return;
                }

                var items = AuditErrors.Where(a =>
                a.Category == "Warning" ||
                a.Category == "Error" ||
                a.Category == "abort").ToList();

                LogLibrary.LogUIInfo($"Filtered {items.Count} warning/error records from {AuditErrors.Count} total");

                AuditErrors = new ObservableCollection<AuditLogItem>(items);

                foreach (var db in AuditErrors)
                {
                    if (DateTimeOffset.TryParse(db.Time, out DateTimeOffset dto))
                    {
                        db.Date = dto.Date.ToString("yyyy-MM-dd");
                        db.Time = dto.ToString("HH:mm:ss");
                        db.InstrumentName = Path.GetFileNameWithoutExtension(filePath).Split('_').FirstOrDefault();
                    }
                    else
                    {
                 //       LogLibrary.LogUIWarning($"Failed to parse time: {db.Time}");
                    }
                }
            }
            catch (JsonException jex)
            {
                LogLibrary.LogUIError($"JSON parsing error in {filePath}: {jex.Message}");
                MessageBox.Show($"Invalid JSON in file: {filePath}\n{jex.Message}",
                                "JSON Error", MessageBoxButton.OK, MessageBoxImage.Error);
                AuditErrors = new ObservableCollection<AuditLogItem>();
            }
            catch (Exception ex)
            {
                LogLibrary.LogUIError($"Error loading JSON data: {ex.Message}");
                //LogLibrary.LogUIError(ex);
                MessageBox.Show($"Error loading data: {ex.Message}",
                                "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                AuditErrors = new ObservableCollection<AuditLogItem>();
            }
        }

        /* old commented code below , need to remove after testing
        private void LoadAllJsonData(string filePath)
        {
            var jsonString = System.IO.File.ReadAllText(filePath);
            var auditErrors = JsonConvert.DeserializeObject<ObservableCollection<AuditLogItem>>(jsonString);
            var items = auditErrors.Where(a => a.Category == "Warning" || a.Category == "Error" || a.Category == "abort");

            auditErrors = new ObservableCollection<AuditLogItem>(items);
            foreach (var db in auditErrors)
            {
                db.Date = DateTimeOffset.Parse(db.Time).Date.ToString("yyyy-MM-dd");
                db.Time = DateTimeOffset.Parse(db.Time).Date.ToString(@"hh\:mm\:ss");

                db.InstrumentName = filePath.ToString().Split('\\').Last().Split('_').First().ToString();
            }
            foreach (var db in auditErrors)
            {
                AuditErrors.Add(db);
            }

        }
        // old commented code above , need to remove after testing
                  
        */

        private void CreateTicket()
        {
            if (CMServicesStatus())
            {
                try
                {

                    LogLibrary.LogUIInfo($"Creating ticket for {SelectedAuditLogs.Count} audit logs");

                    var childWindow = new EmailFormat();
                    childWindow.ViewModel = new EmailViewModel(new ObservableCollection<AuditLogItem>(SelectedAuditLogs));
                    childWindow.Show();

                    childWindow.Closed += (s, e) =>
                    {
                        if (childWindow.ViewModel.isMailSent)
                        {
                            LogLibrary.LogUIInfo("Email sent successfully, marking logs as completed");
                            CreatedCompletedAuditLogs(SelectedAuditLogs);

                            // Refresh the ErrorsView to reflect any changes made in the child window
                            // ErrorsView.Refresh();
                        }
                    };
                }
                catch (Exception ex)
                {
                    LogLibrary.LogUIError($"Error creating ticket: {ex.Message}");
                    //LogLibrary.LogUIError(ex);
                    MessageBox.Show($"Error creating ticket: {ex.Message}",
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            else
            {
                MessageBox.Show("Chromeleon Services are not running. Please start the services and try again.", "Services Not Running", MessageBoxButton.OK, MessageBoxImage.Warning); 
                //LogLibrary.LogUIWarning("Attempted to create ticket while Chromeleon services are not running");
            }
        }
        /// <summary>
        /// CM Services Status Check
        /// </summary>
        /// <returns></returns>
        private bool CMServicesStatus()
        {
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
                            return false;   // If any service is not running
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    // Service not found
                    return false;
                }
            }

            return true; // All services are running
        }


        /// <summary>
        /// Created Completed Audit Logs
        /// </summary>
        /// <param name="selectedAuditLogs"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void CreatedCompletedAuditLogs(ObservableCollection<AuditLogItem> selectedAuditLogs)
        {
            try
            {
               // LogLibrary.LogUIDebug("Marking selected audit logs as completed");

                foreach (var log in selectedAuditLogs)
                {
                    var item = AuditErrors.FirstOrDefault(a =>
                    a.Message == log.Message &&
                    a.Time == log.Time &&
                    a.Date == log.Date);

                    if (item != null)
                    {
                        item.Status = "Completed";
                    }
                }

                WriteToTicketsRaisedForLogs(selectedAuditLogs);
                LogLibrary.LogUIInfo($"Marked {selectedAuditLogs.Count} logs as completed");
            }
            catch (Exception ex)
            {
                LogLibrary.LogUIError($"Error marking logs as completed: {ex.Message}");
                //LogLibrary.LogUIError(ex);
            }
        }

        /// <summary>
        /// write To "Tickets raised for" Logs
        /// </summary>
        /// <param name="selectedAuditLogs"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void WriteToTicketsRaisedForLogs(ObservableCollection<AuditLogItem> selectedAuditLogs)
        {
            try
            {
                //LogLibrary.LogUIDebug("Writing completed tickets to file");

                TicketsRaisedForLogs(selectedAuditLogs);
                ReadTicketsRaisedForLogs();
                IsButtonEnabled = false;
            }
            catch (Exception ex)
            {
                LogLibrary.LogUIError($"Error writing tickets: {ex.Message}");
                //LogLibrary.LogUIError(ex);
            }

        }

        /// <summary>
        /// Reading "Tickets raised for" Logs
        /// </summary>
        public void ReadTicketsRaisedForLogs()
        {
            try
            {
               // LogLibrary.LogUIDebug("Reading completed tickets from file");

                AuditCompletedErrors = new ObservableCollection<AuditLogItem>();

                string destinationFolder = System.IO.Path.Combine(
                 Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                 "CMATLTData", "CompletedAuditLogs");

                if (!Directory.Exists(destinationFolder))
                {
                   // LogLibrary.LogUIWarning("Completed audit logs folder does not exist");
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

                LogLibrary.LogUIInfo($"Found {filesWithDate.Count} completed ticket files for today");

                if (filesWithDate.Count() > 0)
                {
                    foreach (var file in filesWithDate)
                    {
                        // We need to read only selected instrument file
                        if (file.FileName.Split('_')[0].ToString() == SelectedInstrument)
                        {
                            //List<AuditLogItem> data = new List<AuditLogItem>();
                            string json = System.IO.File.ReadAllText(file.FullPath, Encoding.UTF8);

                            List<List<AuditLogItem>> nestedList = JsonConvert.DeserializeObject<List<List<AuditLogItem>>>(json);
                            if (nestedList == null)
                            {
                              //  LogLibrary.LogUIWarning($"No data found in completed ticket file: {file.FullPath}");
                                continue;
                            }
                            // Flatten to a single list if needed
                            List<AuditLogItem> flatList = nestedList.SelectMany(x => x).ToList();
                            if (flatList == null)
                            {
                               // LogLibrary.LogUIWarning($"No data found after flattening completed ticket file: {file.FullPath}");
                                continue;
                            }

                            foreach (var log in flatList)
                            {
                                var item = AuditErrors.FirstOrDefault(a => a.ID == log.ID);
                                if (item != null)
                                {
                                    item.Status = "Completed";
                                }
                                AuditCompletedErrors.Add(log);
                            }

                            foreach (var db in AuditCompletedErrors)
                            {
                                if (DateTimeOffset.TryParse(db.Time, out DateTimeOffset dto))
                                {
                                    db.Date = dto.Date.ToString("yyyy-MM-dd");
                                    db.Time = dto.ToString("HH:mm:ss");
                                }
                            }

                            LogLibrary.LogUIInfo($"Loaded {flatList.Count} completed tickets for {SelectedInstrument}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogLibrary.LogUIError($"Error reading completed tickets: {ex.Message}");
                //LogLibrary.LogUIError(ex);
            }

        }

        /// <summary>
        /// Writing "Tickets raised for" Logs
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public void TicketsRaisedForLogs(ObservableCollection<AuditLogItem> completedAuditLogs)
        {
            try
            {
               // LogLibrary.LogUIDebug("Preparing completed tickets for writing to file");

                var completedItems = completedAuditLogs as ObservableCollection<AuditLogItem>;

                string destinationFolder = System.IO.Path.Combine(
                 Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                 "CMATLTData", "CompletedAuditLogs");


                if (!Directory.Exists(destinationFolder))
                {
                    Directory.CreateDirectory(destinationFolder);
                }


                string dateString = DateTime.Now.ToString("yyyy-MM-dd");
                string backupFile = System.IO.Path.Combine(destinationFolder, $"{SelectedInstrument}_{dateString}.json");
                var jsonEntities = new List<Dictionary<string, object>>();

                foreach (var line in completedItems)
                {
                    //string[] values = line.Split(new[] { "\t" }, StringSplitOptions.None);

                    var jsonEntity = new Dictionary<string, object>();
                    // jsonEntity["ID"] = line.id;
                    //if (Enum.TryParse<AuditTrailMessageCategory>(Convert.ToString(line.Category), true, out var category))
                    //{
                    //    // Store the enum value as string in JSON
                    //    jsonEntity["Category"] = category.ToString();
                    //}
                    jsonEntity["ID"] = line.ID;
                    jsonEntity["Category"] = line.Category;
                    jsonEntity["Device"] = line.Device;
                    jsonEntity["Level"] = "";
                    jsonEntity["Message"] = line.Message;
                    jsonEntity["RetentionTime"] = line.RetentionTime;
                    jsonEntity["Time"] = line.Time;
                    jsonEntity["Status"] = "Completed";
                    jsonEntities.Add(jsonEntity);
                }

                /* old commented code below , need to remove after testing

                //var settings = new JsonSerializerSettings
                //{
                //    Formatting = Formatting.Indented,
                //    StringEscapeHandling = StringEscapeHandling.Default
                //};
                //string json = JsonConvert.SerializeObject(jsonEntities, settings);
                //var settings1 = new JsonSerializerSettings
                //{
                //    Formatting = Formatting.None   // Important for single-line JSON
                //};
                //string jsonLine = JsonConvert.SerializeObject(jsonEntities, settings1);
                //            System.IO.File.AppendAllText(
                //    backupFile,
                //    jsonLine + Environment.NewLine,
                //    Encoding.UTF8
                //);

                //var newItem = new
                //{
                //    ID = "",
                //    Category = "Information",
                //    Device = "",
                //    Level = "",
                //    Message = "New message here",
                //    RetentionTime = 0.1234,
                //    Time = DateTime.Now.ToString("HH:mm:ss")
                //};
                // old commented code above , need to remove after testing
                */

                JArray jsonArray;

                // Check if file exists
                if (System.IO.File.Exists(backupFile))
                {
                    string existing = System.IO.File.ReadAllText(backupFile, Encoding.UTF8);

                    // Parse existing JSON array
                    jsonArray = string.IsNullOrWhiteSpace(existing) ? new JArray() : JArray.Parse(existing);
                }
                else
                {
                    jsonArray = new JArray();
                }

                // Add new item
                jsonArray.Add(JToken.FromObject(jsonEntities));

                // Write back formatted JSON
                System.IO.File.WriteAllText(backupFile, jsonArray.ToString(Formatting.Indented), Encoding.UTF8);

                /* old commented code below , need to remove after testing

                //foreach (var line in System.IO.File.ReadLines(backupFile))
                //{
                //    if (!string.IsNullOrWhiteSpace(line))
                //    {
                //        var item = JsonConvert.DeserializeObject<List<AuditLogItem>>(json);
                //        data.Add(item);
                //    }
                //}


                //if (System.IO.File.Exists(backupFile))
                //{
                //    System.IO.File.WriteAllText(backupFile, json, Encoding.UTF8);
                //}
                //else
                //{
                //    System.IO.File.WriteAllText(backupFile, json, Encoding.UTF8);
                //}
                // old commented code above , need to remove after testing
                */
                LogLibrary.LogUIInfo($"Successfully wrote {completedItems.Count} completed tickets to file: {backupFile}");
            }
            catch (Exception ex)
            {
                LogLibrary.LogUIError($"Error writing completed tickets: {ex.Message}");
                //LogLibrary.LogUIError(ex);
                MessageBox.Show($"Error writing completed tickets: {ex.Message}",
                                "Write Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        private bool FilterErrors(object obj)
        {
            var item = obj as AuditLogItem;
            if (item == null)
                return false;
            DateTime date = Convert.ToDateTime(item.Date).Date;

            //DateTime date = item.Date.Date;  // normalize time

            if (FromDate != null && date < FromDate.Value.Date)
                return false;
            else if (FromDate != null && date > FromDate.Value.Date)
                return false;
            //if (ToDate != null && date > ToDate.Value.Date)
            //    return false;

            return true;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    }
    public static class DataGridSelectedItemsBehavior
    {
        public static readonly DependencyProperty BindableSelectedItemsProperty =
            DependencyProperty.RegisterAttached(
                "BindableSelectedItems",
                typeof(IList),
                typeof(DataGridSelectedItemsBehavior),
                new PropertyMetadata(null, OnBindableSelectedItemsChanged));

        public static IList GetBindableSelectedItems(DependencyObject obj) =>
            (IList)obj.GetValue(BindableSelectedItemsProperty);

        public static void SetBindableSelectedItems(DependencyObject obj, IList value) =>
            obj.SetValue(BindableSelectedItemsProperty, value);

        private static void OnBindableSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid grid)
            {
                grid.SelectionChanged += (s, ev) =>
                {
                    try
                    {
                        var bindable = GetBindableSelectedItems(grid);
                        bindable?.Clear();

                        foreach (var item in grid.SelectedItems)
                            bindable?.Add(item);

                        if (grid.DataContext is AuditTrailViewModel vm)
                        {
                            var completedItems = vm.SelectedAuditLogs.Where(a => a.Status != "Completed");
                            if (completedItems != null && completedItems.Count() > 0)
                            {
                                vm.IsButtonEnabled = grid.SelectedItems.Count > 0;
                            }
                            else
                            {
                                vm.IsButtonEnabled = false;
                            }

                            //LogLibrary.LogUIDebug($"Selection changed: {grid.SelectedItems.Count} items selected, button enabled: {vm.IsButtonEnabled}");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogLibrary.LogUIError($"Error in SelectionChanged event: {ex.Message}");
                    }
                };
            }
        }
    }
}
