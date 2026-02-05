using CMATCTUI.ChildWindows;
using CMATCTUI.Commands;
using CMATCTUI.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shapes;
using static System.Net.WebRequestMethods;

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
            if(this.FromDate!=null)
            {
                filesWithDate = Directory.GetFiles(destinationFolder)
                 .Where(f => System.IO.File.GetCreationTime(f).Date == FromDate)
                  .ToList();
                var instrumentsDetails = this.filesWithDate.Where(a => a.ToString().Split('\\').Last().Split('_').First().ToString() == SelectedInstrument).ToList();
                if (instrumentsDetails.Count() > 0)
                {
                    DynamicJsonFileLoading(instrumentsDetails.First(), SelectedInstrument);
                    ReadCompletedAuditTrailLogs();
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
                    ReadCompletedAuditTrailLogs();
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
        public List<string> filesWithDate;
        public string destinationFolder;
        public AuditTrailViewModel()
        {

            SelectedAuditLogs = new ObservableCollection<AuditLogItem>();
            //InstrumentsDetails = new ObservableCollection<string>();
            CreateTicketCommand = new RelayCommand(CreateTicket);
            destinationFolder = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "CMATLTData", "Data");

            filesWithDate = Directory.GetFiles(destinationFolder)
                 .Where(f => System.IO.File.GetCreationTime(f).Date == DateTime.Today)
                  .ToList();
            this.FromDate= DateTime.Now.Date;
            MinDate = DateTime.Today.AddDays(-30).Date;
           
            // files = Directory.GetFiles(destinationFolder);

            if (filesWithDate.Count() <= 0)
            {
                IsFilterStackEnabled = false;
                MessageBox.Show("Data is Not Available in Database ");
            }
            else
            {
                IsFilterStackEnabled = true;
                List<string> fileList = filesWithDate.ToList();
                InstrumentsDetails = new ObservableCollection<string>();
               // InstrumentsDetails.Add("--ALL--");
                foreach (var file in filesWithDate)
                {
                    InstrumentsDetails.Add(file.ToString().Split('\\').Last().Split('_').First().ToString());
                }
                InstrumentsDetails = new ObservableCollection<string>(InstrumentsDetails.Distinct());
                SelectedInstrument = InstrumentsDetails[0];
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
            }

        }

        private void OnInstrumentChanged(string instrumentName)
        {
            var instrumentsDetails = this.filesWithDate.Where(a => a.ToString().Split('\\').Last().Split('_').First().ToString() == instrumentName).ToList();
            if (instrumentsDetails.Count() > 0)
            {
                DynamicJsonFileLoading(instrumentsDetails.First(), instrumentName);

                ReadCompletedAuditTrailLogs();
            }
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
            
        }


        public void DynamicJsonFileLoading(string instrumentsFilePath, string selectedInstrument)
        {
            SelectedAuditLogs = new ObservableCollection<AuditLogItem>();
            AuditErrors = new ObservableCollection<AuditLogItem>();
            LoadJsonData(instrumentsFilePath);

            ErrorsView = CollectionViewSource.GetDefaultView(AuditErrors);
            //var item = Errors.Take(5).ToList();
            //AuditCompletedErrors = new ObservableCollection<AuditLogItem>(item);
            //AuditCompletedErrors.ToList().ForEach(a => a.Status = "Completed");

            //ErrorsView.Filter = FilterErrors;
            //FromDate = null;
            //ToDate = null;
        }


        private void LoadJsonData(string filePath)
        {
            var jsonString = System.IO.File.ReadAllText(filePath);
            AuditErrors = JsonConvert.DeserializeObject<ObservableCollection<AuditLogItem>>(jsonString);
            var items = AuditErrors.Where(a => a.Category == "Warning" || a.Category == "Error" || a.Category == "abort");
            AuditErrors = new ObservableCollection<AuditLogItem>(items);
            foreach (var db in AuditErrors)
            {
                db.Date = DateTimeOffset.Parse(db.Time).Date.ToString("yyyy-MM-dd");
                db.Time = DateTimeOffset.Parse(db.Time).Date.ToString(@"hh\:mm\:ss");
                db.InstrumentName = filePath.ToString().Split('\\').Last().Split('_').First().ToString();
            }

        }

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

        private void CreateTicket()
        {
           
            var childWindow = new EmailFormat();
           
            childWindow.ViewModel = new EmailViewModel(new ObservableCollection<AuditLogItem>(SelectedAuditLogs));
            childWindow.Show();
            childWindow.Closed += (s, e) =>
            {
                if (childWindow.ViewModel.isMailSent)
                    CreatedCompletedAuditLogs(SelectedAuditLogs);


                // Refresh the ErrorsView to reflect any changes made in the child window
               // ErrorsView.Refresh();


            };


        }
        /// <summary>
        /// Created Completed Audit Logs
        /// </summary>
        /// <param name="selectedAuditLogs"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void CreatedCompletedAuditLogs(ObservableCollection<AuditLogItem> selectedAuditLogs)
        {
            foreach (var log in selectedAuditLogs)
            {
                var item = AuditErrors.FirstOrDefault(a => a.Message == log.Message && a.Time == log.Time && a.Date == log.Date);
                if (item != null)
                {
                    item.Status = "Completed";
                }
            }
            WriteToCompletedAuditTraillogs(selectedAuditLogs);
        }
        /// <summary>
        /// write To Completed Audit Traillogs
        /// </summary>
        /// <param name="selectedAuditLogs"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void WriteToCompletedAuditTraillogs(ObservableCollection<AuditLogItem> selectedAuditLogs)
        {
            CompletedAuditTrailLogs(selectedAuditLogs);

            ReadCompletedAuditTrailLogs();
            IsButtonEnabled = false;


        }
        /// <summary>
        /// Reading Completed Audit Trail Logs
        /// </summary>
        public void ReadCompletedAuditTrailLogs()
        {
            AuditCompletedErrors = new ObservableCollection<AuditLogItem>();

            string destinationFolder = System.IO.Path.Combine(
             Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
             "CMATLTData", "CompletedAuditLogs");


            if (Directory.Exists(destinationFolder))
            {
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
                    foreach (var file in filesWithDate)
                    {
                        //selected instrument file only we need to read
                        if (file.FileName.Split('_')[0].ToString() == SelectedInstrument)
                        {
                            List<AuditLogItem> data = new List<AuditLogItem>();
                            string json = System.IO.File.ReadAllText(file.FullPath, Encoding.UTF8);

                            List<List<AuditLogItem>> nestedList = JsonConvert.DeserializeObject<List<List<AuditLogItem>>>(json);

                            // Flatten to a single list if needed
                            List<AuditLogItem> flatList = nestedList.SelectMany(x => x).ToList();
                            foreach (var log in flatList)
                            {
                                AuditErrors.Where(a=>a.ID==log.ID).FirstOrDefault().Status="Completed";
                                AuditCompletedErrors.Add(log);
                            }
                            foreach (var db in AuditCompletedErrors)
                            {
                                db.Date = DateTimeOffset.Parse(db.Time).Date.ToString("yyyy-MM-dd");
                                db.Time = DateTimeOffset.Parse(db.Time).Date.ToString(@"hh\:mm\:ss");
                            }
                        }
                    }

                    

                }

            }
            else
            {
                //MessageBox.Show("Completed Audit Logs Folder does not exist.");
            }

        }
        /// <summary>
        /// Writing Completed Audit Trail Logs
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public void CompletedAuditTrailLogs(ObservableCollection<AuditLogItem> completedAuditLogs)
        {
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

        public static IList GetBindableSelectedItems(DependencyObject obj) => (IList)obj.GetValue(BindableSelectedItemsProperty);
        public static void SetBindableSelectedItems(DependencyObject obj, IList value) => obj.SetValue(BindableSelectedItemsProperty, value);

        private static void OnBindableSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid grid)
            {
                grid.SelectionChanged += (s, ev) =>
                {

                    var bindable = GetBindableSelectedItems(grid);
                    bindable?.Clear();
                    foreach (var item in grid.SelectedItems)
                        bindable?.Add(item);

                    if (grid.DataContext is AuditTrailViewModel vm)
                    {
                       var completedItems= vm.SelectedAuditLogs.Where(a=>a.Status!="Completed");
                        if (completedItems != null && completedItems.Count() > 0)
                        {
                            vm.IsButtonEnabled = grid.SelectedItems.Count > 0;
                        }
                        else
                        {
                            vm.IsButtonEnabled = false;
                        }
                    }
                };
            }
        }
    }

}
