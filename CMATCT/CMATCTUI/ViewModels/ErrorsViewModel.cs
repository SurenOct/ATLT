using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using CMATCTUI.Models;

namespace CMATCTUI.ViewModels
{
    public class ErrorsViewModel
    {
        public ObservableCollection<ErrorItem> Errors { get; set; }
        public ICollectionView ErrorsView { get; set; }


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

        public ErrorsViewModel()
        {

            // Sample data
            Errors = new ObservableCollection<ErrorItem>
            {
                new ErrorItem
                {
                    Date = DateTime.Now.AddDays(-1),
                    Time = DateTime.Now.Date.ToString("10:05:12"),
                    RetentionTime=Convert.ToDateTime("2025-12-04 10:05:12"),
                    SequenceName = "Seq_ICS5000_RoutineRun_001",
                    Instrument = "ICS-5000",
                    Summary = "Chromeleon reported communication timeout during sequence execution.",
                    Severity = "Critical",
                    Status = "New"
                },

                // 2. Chromeleon error – sequence name
                new ErrorItem
                {
                    Date = DateTime.Now.AddDays(-2),
                    Time = DateTime.Now.Date.ToString("10:05:12"),
                    RetentionTime=Convert.ToDateTime("2025-12-04 10:05:12"),
                    SequenceName = "Seq_QC_Check_StdMix_2025_12_04",
                    Instrument = "ICS-6000",
                    Summary = "Chromeleon detected missing vial position in sequence table.",
                    Severity = "Warning",
                    Status = "Ticket Created"
                },

                // 3. Chromeleon error – sequence name
                new ErrorItem
                {
                    Date = DateTime.Now.AddDays(-3),
                    Time = DateTime.Now.Date.ToString("10:03:12"),
                    RetentionTime=Convert.ToDateTime("2025-12-04 10:05:12"),
                    SequenceName = "Seq_StabilityStudy_BatchA_01",
                    Instrument = "ICS-5000+",
                    Summary = "Chromeleon reported peak integration failure for multiple channels.",
                    Severity = "Warning",
                    Status = "Ticket Created"
                },

                // 4. Windows error event (not tied to a specific sequence)
                new ErrorItem
                {
                     Date = DateTime.Now.AddDays(-4),
                    Time = DateTime.Now.Date.ToString("10:05:12"),
                    RetentionTime=Convert.ToDateTime("2025-12-04 10:05:12"),
                    SequenceName = "(N/A - Windows Event)",
                    Instrument = "ICS-6000",
                    Summary = "Windows Application Error: ChromeleonServer.exe stopped responding (Event ID: 1002).",
                    Severity = "Critical",
                    Status = "New"
                }
        };

            ErrorsView = CollectionViewSource.GetDefaultView(Errors);

            ErrorsView.Filter = FilterErrors;
            FromDate = null;
            ToDate = null;
        }
        private bool FilterErrors(object obj)
        {
            var item = obj as ErrorItem;
            if (item == null)
                return false;

            DateTime date = item.Date.Date;  // normalize time

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
}
