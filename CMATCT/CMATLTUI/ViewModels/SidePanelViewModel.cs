using CMATCTUI.Commands;
using CMATCTUI.Pages;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

//Test Comment
namespace CMATCTUI.ViewModels
{
    public class SidePanelViewModel : INotifyPropertyChanged
    {
        private Page _currentPage;
        public Page CurrentPage
        {
            get => _currentPage;
            set { _currentPage = value; OnPropertyChanged(); }
        }


        public ICommand NavigateToAuditTrailPageCommand { get; }
        public ICommand NavigateToSettingsPageCommand { get; }
        public ICommand NavigateToDashBoardPageCommand { get; }
        public ICommand NavigateToAboutPageCommand { get; }

        // private SerialPortInstance _serialService;

        private AuditTrailPage auditTrailPage;
        private SettingsPage _settingsPage;
        private DashBoardPage _dashBoardPage;
        private AboutPage _aboutPage;
        public SidePanelViewModel()
        {
            DeleteFilesInFolder();
            //_serialService = SerialPortInstance.Instance;
            //_serialService.OpenPort();
            NavigateToAuditTrailPageCommand = new RelayCommand(NavigateToAuditTrailPage);
            NavigateToSettingsPageCommand = new RelayCommand(NavigateToSettingsPage);
            NavigateToDashBoardPageCommand = new RelayCommand(NavigateToDashBoardPage);
            NavigateToAboutPageCommand = new RelayCommand(NavigateToAboutPage);

            _dashBoardPage = new DashBoardPage();
            CurrentPage = _dashBoardPage;
            // CurrentPage = new FatAnalyzer { DataContext = new FatAnalyzerViewModel() };
          
        }
        /// <summary>
        /// Deletes all files and subdirectories in the specified folder.
        /// </summary>
        /// <param name="folderPath"></param>
        private void DeleteFilesInFolder()
        {
                string folderPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "CMATLTData", "Data");


            if (!Directory.Exists(folderPath))
                return;

            var files = Directory.GetFiles(folderPath);

            foreach (var file in files)
            {
                FileInfo fileInfo = new FileInfo(file);

                // Check if file is older than 30 days
                if (fileInfo.CreationTime < DateTime.Now.AddDays(-30))
                {
                    try
                    {
                        MessageBox.Show($"Deleting file: {fileInfo.Name}");
                        fileInfo.Delete();
                    }
                    catch (Exception ex)
                    {
                        // Log exception if needed
                        Console.WriteLine($"Failed to delete {fileInfo.Name}: {ex.Message}");
                    }
                }
            }
        }

        private void NavigateToAuditTrailPage()
        {
            string destinationFolder = Path.Combine(
               Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
               "CMATLTData", "Data");
            if (!Directory.Exists(destinationFolder))
            {
                MessageBox.Show("No Local Database available");
                return;
            }


            if (auditTrailPage == null)
                auditTrailPage = new AuditTrailPage();
            CurrentPage = auditTrailPage;
        }

        private void NavigateToSettingsPage()
        {
            if (_settingsPage == null)
                _settingsPage = new SettingsPage();
            CurrentPage = _settingsPage;


            // CurrentPage = new Reports();

        }

        private void NavigateToDashBoardPage()
        {
            if (_dashBoardPage == null)
                _dashBoardPage = new DashBoardPage();
            CurrentPage = _dashBoardPage;
        }

        private void NavigateToAboutPage()
        {
            if (_aboutPage == null)
                _aboutPage = new AboutPage();
            CurrentPage = _aboutPage;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
