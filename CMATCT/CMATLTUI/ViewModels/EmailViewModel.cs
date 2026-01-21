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
using System.Threading;
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
        public ICommand GeneratedCmbxFileCommand { get; }
        public EmailViewModel(ObservableCollection<AuditLogItem> itemsFromParent)
        {
            GeneratedCmbxFileCommand = new RelayCommand(GenerateCmbxFile);


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

        private void GenerateCmbxFile()
        {

            //using (new CmSdkScope())
            //{
            //    CmSdk.Logon.DoLogon();
            //    if (CmSdk.Logon.IsLoggedOn)
            //    {
            //        try
            //        {
            //            var exportEngine = CmSdk.GetExportEngine();
            //            var tokenSource = new CancellationTokenSource();
            //            var targetFileName = @"C:\Demo\test.cmbx";
            //            var progress = new Progress<double>();
            //            var itemsToExport = new List<Uri> { new Uri("chrom://tf-251216232713/ChromeleonLocal/Instrument Data/MyInuvion/SeqInuvion.seq") };
            //            // Create and start a new export task
            //            var importTask = exportEngine.ExportToCmbxAsync(
            //                itemsToExport, // The items which will be exported
            //                targetFileName, // The target filename
            //                "Just a short description",
            //                CmbxExportOptions.IncludeAllData, // Exporting options
            //                CmbxContainerFormat.V2, // Container format (currently there are two different formats)
            //                progress, // Progress information
            //                tokenSource.Token // Cancellation token (in order to cancel the export operation)
            //                );

            //            // Wait for task completion and continue in the current synchronization 
            //            // context (useful for UI threads)
            //            importTask.ContinueWith(t =>
            //            {
            //                if (t.IsFaulted)
            //                {
            //                    // Faulted with exception
            //                    Exception ex = t.Exception;
            //                    while (ex is AggregateException && ex.InnerException != null)
            //                        ex = ex.InnerException;
            //                    MessageBox.Show("Error: " + ex.Message);
            //                }
            //                else if (t.IsCanceled)
            //                {
            //                    MessageBox.Show("Cancelled.");
            //                }
            //                else
            //                {
            //                    MessageBox.Show("Completed.");
            //                }
            //            }, TaskScheduler.FromCurrentSynchronizationContext());
            //        }
            //        catch (Exception ex)
            //        {
            //            MessageBox.Show("Error: " + ex.Message);
            //        }
            //    }
            //}
                   
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
        public bool isMailSent{get;set;}

        public void SendEmail()
        {
            isMailSent = true;

            if (IsMailSent())
            {
                isMailSent = true;
                MessageBox.Show("Email sent successfully.");

            }
            else
            {
                MessageBox.Show("Failed to send email.");
                isMailSent = false;
            }

        }
        public bool IsMailSent()
        {
            bool IsConnect = false;
            XDocument doc = GenericXmlLoader.LoadingXML("SMTPSettings.xml", "XMLConfiguration");
            SmtpSettings smtpSettings = null;
            if (doc != null)
            {
                var root = doc.Element("SmtpSettings");

                smtpSettings = new SmtpSettings
                {
                    Server = root.Element("Server")?.Value,
                    Port = int.Parse(root.Element("Port")?.Value ?? "0"),
                    Encryption = root.Element("Encryption")?.Value,
                    Username = root.Element("Username")?.Value,
                    Password = root.Element("Password")?.Value
                };
            }
            if (smtpSettings == null)
            {
                MessageBox.Show("Mail is not Configured Properly. Please contact Administrator.");
             }
             SmtpClient mailClient = new SmtpClient();
            if (ConnectingSMTPServer(mailClient, smtpSettings))
            {
                var message = new MimeMessage();


               
                message.From.Add(new MailboxAddress("Test", smtpSettings.Username));
                message.To.Add(MailboxAddress.Parse(smtpSettings.Username));
                message.Subject = "Incedent Type:" + _selectedIncidentType;
                message.Body = new TextPart("plain")
                {
                    Text = "Hello,\n\n "+ Items.First().Message + ".\n\nRegards,\nMahesh"
                };
                mailClient.Send(message);
                mailClient.Disconnect(true);
                IsConnect = true;
            }
            else
            {
                MessageBox.Show("Unable to connect to SMTP Server. Please check the configuration.");
            }
            return IsConnect;
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
                //client.Authenticate(smtpSettings.Username, smtpSettings.Password);


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
