using CMATCTUI.Commands;
using CMATCTUI.Generic;
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
using System.Xml.Linq;

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
        private List<string> _incidentsItems;
        public List<string> IncidentsItems
        {
            get => _incidentsItems;
            set
            {
                _incidentsItems = value;
                OnPropertyChanged(nameof(IncidentsItems));
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
        private string _selectedIncidentType;
        public string SelectedIncidentType
        {
            get => _selectedIncidentType;
            set
            {
                if (_selectedIncidentType != value)
                {
                    _selectedIncidentType = value;
                    OnPropertyChanged(nameof(SelectedIncidentType));
                }
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
                
            }
            this.IncidentDescription = incedentMsg.ToString();

            LoadIncedents();
        }
        public void LoadIncedents()
        {
            XDocument doc = GenericXmlLoader.LoadingXML("IncidentsSettings.xml", "XMLConfiguration");
            IncidentsItems = new List<string>();
            XElement  root= doc.Element("IncedentsSetting");
            if (root != null)
            {
                foreach (XElement item in root.Elements("IncedentType"))
                {
                    IncidentsItems.Add(item.Value);
                }    
            }
        }
     

        public void SendEmail()
        {
            XDocument doc = GenericXmlLoader.LoadingXML("SMTPSettings.xml", "XMLConfiguration");
            SmtpSettings smtpSettings = null;
            if (doc != null)
            {
                var root = doc.Element("SmtpSettings");

                smtpSettings =new SmtpSettings
                {
                    Server = root.Element("Server")?.Value,
                    Port = int.Parse(root.Element("Port")?.Value ?? "0"),
                    Encryption = root.Element("Encryption")?.Value,
                    Username = root.Element("Username")?.Value,
                    Password = root.Element("Password")?.Value
                };
            }
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
