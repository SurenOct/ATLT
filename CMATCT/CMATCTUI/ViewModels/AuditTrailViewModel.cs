using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using CMATCTUI.ChildWindows;
using CMATCTUI.Commands;
using CMATCTUI.Models;
using Newtonsoft.Json;

//Test Commit
namespace CMATCTUI.ViewModels
{
    public class AuditTrailViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<AuditLogItem> Errors { get; set; }
        public ObservableCollection<AuditLogItem> AuditCompletedErrors { get; set; }
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
        


        private DateTime? _fromDate;
        public DateTime? FromDate
        {
            get => _fromDate;
            set
            {
                _fromDate = value;
                OnPropertyChanged(nameof(FromDate));
                ErrorsView.Refresh();
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
                ErrorsView.Refresh();
            }
        }

        public ICommand CreateTicketCommand { get; }

        public AuditTrailViewModel()
        {
            SelectedAuditLogs = new ObservableCollection<AuditLogItem>();
            InstrumentsDetails = new ObservableCollection<string>();
            CreateTicketCommand = new RelayCommand(CreateTicket);
            string destinationFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "CMATCTData", "Data");
             
            string[] files = Directory.GetFiles(destinationFolder);

            if (files.Length <= 0)
            {
                IsFilterStackEnabled = false;
                MessageBox.Show("Data is Not Available in Database ");
            }
            else
            {
                IsFilterStackEnabled = true;
                List<string> fileList = files.ToList();
                foreach (var file in files)
                {
                    InstrumentsDetails.Add(file.ToString().Split('\\').Last().Split('_').First().ToString());
                }
                InstrumentsDetails = new ObservableCollection<string>(InstrumentsDetails.Distinct());
                // var jsonString = File.ReadAllText(@"C:\\CM_AT_Tool\\CMATCT\\CMATCTUI\\FileData\\backup_2025-12-24_114803.json");
                string path = files[0].ToString();
                var jsonString = File.ReadAllText(files[0].ToString());
                var items = File.ReadAllText(path);
                // Deserialize JSON string to object
                Errors = JsonConvert.DeserializeObject<ObservableCollection<AuditLogItem>>(jsonString);
                DateTimeOffset dto = DateTimeOffset.Parse(Errors[0].Time);
                 foreach(var db in Errors)
                {
                    db.Date = DateTimeOffset.Parse(db.Time).Date.ToString("yyyy-MM-dd");
                    db.Time = DateTimeOffset.Parse(db.Time).Date.ToString(@"hh\:mm\:ss");
                }
                Errors.ToList().ForEach(a => a.Status = "New");

                AuditCompletedErrors = JsonConvert.DeserializeObject<ObservableCollection<AuditLogItem>>(jsonString);
                
                ErrorsView = CollectionViewSource.GetDefaultView(Errors.Where(x => x.Status != "Completed").ToList());
                // AuditCompletedErrors = new ObservableCollection<AuditLogItem>(Errors.Where(x => x.Status == "Completed").ToList());
                var item = Errors.Take(5).ToList();
                AuditCompletedErrors = new ObservableCollection<AuditLogItem>(item);
                AuditCompletedErrors.ToList().ForEach(a => a.Status = "Completed");

                ErrorsView.Filter = FilterErrors;
                FromDate = null;
                ToDate = null;
            }
        }

        private void CreateTicket()
        {
            var childWindow = new EmailFormat();
            childWindow.ViewModel = new EmailViewModel(new ObservableCollection<AuditLogItem>(SelectedAuditLogs));
            childWindow.Show();
 

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

            if (ToDate != null && date > ToDate.Value.Date)
                return false;

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
                        vm.IsButtonEnabled = grid.SelectedItems.Count > 0;
                    }
                };
            }
        }
    }

}
