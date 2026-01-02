using CMATCTUI.Commands;
using CMATCTUI.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        private string _incidentDescription;
        public string IncidentDescription
        {
            get => _incidentDescription;
            set
            {
                _incidentDescription = value;
                OnPropertyChanged(nameof(IncidentDescription));
            }
        }


        public ICommand SendEmailCommand { get; }
        public EmailViewModel(ObservableCollection<AuditLogItem> itemsFromParent)
        {
            SendEmailCommand = new RelayCommand(SendEmail);
            // Make a copy if you don't want parent and child to modify the same collection
            Items = new ObservableCollection<AuditLogItem>(itemsFromParent);
            StringBuilder incedentMsg = new StringBuilder();
            foreach (var item in Items)
            {
                incedentMsg.AppendLine(item.Message);
                //        incedentMsg.AppendLine("\n");
            }
            this.IncidentDescription = incedentMsg.ToString();
        }

        public void SendEmail()
        {
            var smtpSettings = SmtpConfigLoader.Load("SMTPSettings.xml");
            if(smtpSettings==null)
            {
                MessageBox.Show("Mail is not Configured Properly. Please contact Administrator.");
                return;
            }
            SmtpClient mailClient = new SmtpClient();
            if (ConnectingSMTPServer(mailClient, smtpSettings))
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Mahesh Kulkarni",smtpSettings.Username));
                message.To.Add(MailboxAddress.Parse("recipient@company.com"));
                message.Subject = "Test Email using XML Configuration";
                message.Body = new TextPart("plain")
                {
                    Text = "Hello,\n\nThis email was sent using SMTP settings loaded from XML.\n\nRegards,\nMahesh"
                };
                mailClient.Send(message);
                mailClient.Disconnect(true);
            }
            else
            {
                MessageBox.Show("Unable to connect to SMTP Server. Please check the configuration.");
            }
        }

        public bool ConnectingSMTPServer(SmtpClient client, SmtpSettings smtpSettings)
        {
            bool IsConnect = false;
            try
            {
              
                SecureSocketOptions socketOptions;

                string encryption = smtpSettings.Encryption?.ToUpper() ?? "STARTTLS"; // default if null

                switch (encryption)
                {
                    case "STARTTLS":
                        socketOptions = SecureSocketOptions.StartTls;
                        break;
                    case "SSL":
                        socketOptions = SecureSocketOptions.SslOnConnect;
                        break;
                    case "NONE":
                        socketOptions = SecureSocketOptions.None;
                        break;
                    default:
                        socketOptions = SecureSocketOptions.StartTls;
                        break;
                }
                client.Connect(smtpSettings.Server, smtpSettings.Port, socketOptions);
                client.Authenticate(smtpSettings.Username, smtpSettings.Password);


                IsConnect = true;
            }
            catch (Exception ex)
            {
                IsConnect = false;
            }
            return IsConnect;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
