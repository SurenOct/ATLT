using CMATCTUI.Commands;
using CMATCTUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CMATCTUI.ViewModels
{
    public class EmailViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<AuditLogItem> _items;
        public ObservableCollection<AuditLogItem> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }
        //public EmailViewModel()
        //{

        //}
        //public EmailViewModel()
        //{

        //}

        public ICommand SendEmailCommand { get; }
        public EmailViewModel(ObservableCollection<AuditLogItem> itemsFromParent)
        {
            SendEmailCommand = new RelayCommand(SendEmail);
            // Make a copy if you don't want parent and child to modify the same collection
            Items = new ObservableCollection<AuditLogItem>(itemsFromParent);
        }

        public void SendEmail()
        {
            // Implement email sending logic here using the Items collection
            // For example, you could format the items into an email body and send it via SMTP
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
