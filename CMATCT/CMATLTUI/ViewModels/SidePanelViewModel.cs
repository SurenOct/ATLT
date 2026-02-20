using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CMATCTUI.Commands;
using CMATCTUI.Pages;
using CMATLTLoger;

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
            LogLibrary.LogUIInfo("SidePanelViewModel Initialized");
            DeleteFilesInFolder();
            //_serialService = SerialPortInstance.Instance;
            //_serialService.OpenPort();
            NavigateToAuditTrailPageCommand = new RelayCommand(NavigateToAuditTrailPage);
            NavigateToSettingsPageCommand = new RelayCommand(NavigateToSettingsPage);
            NavigateToDashBoardPageCommand = new RelayCommand(NavigateToDashBoardPage);
            NavigateToAboutPageCommand = new RelayCommand(NavigateToAboutPage);

            //LogLibrary.LogUIInfo("Application started. All Page Commands initialized.");

            _dashBoardPage = new DashBoardPage();
            CurrentPage = _dashBoardPage;
            // CurrentPage = new FatAnalyzer { DataContext = new FatAnalyzerViewModel() };
            //LogLibrary.LogUIInfo("Default page set to Dashboard.");
        }




        /// <summary>
        /// Deletes all files and subdirectories in the specified folder.
        /// </summary>
        /// <param name="folderPath"></param>
        private void DeleteFilesInFolder()
        {
            try
            {

                string folderPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "CMATLTData", "Data");

                LogLibrary.LogUIInfo($"Attempting to delete files in folder: {folderPath}");

                if (!Directory.Exists(folderPath))
                {
                    LogLibrary.LogError($"Folder not found: {folderPath}. No files to delete.");
                    MessageBox.Show($"Folder not found: {folderPath}. No files to delete.");

                    //   LogLibrary.LogUIWarning($"Folder not found: {folderPath}. No files to delete.");
                    return;
                }

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
                            LogLibrary.LogUIError($"Failed to delete {fileInfo.Name}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log exception if needed
                Console.WriteLine($"Error while deleting files: {ex.Message}");
                // LogLibrary.LogUIError($"Error while deleting files: {ex.Message}");
            }
        }

        private void NavigateToAuditTrailPage()
        {
            LogLibrary.LogUIInfo("Navigating to Audit Trail Page.");
            try
            {
                string destinationFolder = Path.Combine(
                   Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                   "CMATLTData", "Data");
                if (!Directory.Exists(destinationFolder))
                {
                    MessageBox.Show("No Local Database available");
                    return;
                }
                LogLibrary.LogUIInfo("Local Database found. Navigating to Audit Trail Page.");

                if (auditTrailPage == null)
                    auditTrailPage = new AuditTrailPage();
                CurrentPage = auditTrailPage;
                LogLibrary.LogUIInfo("Audit Trail Page set as current page.");
            }
            catch (Exception ex)
            {
                LogLibrary.LogUIError($"Error navigating to Audit Trail Page: {ex.Message}");
                MessageBox.Show("An error occurred while trying to access the Audit Trail. Please check the logs for more details.");
            }
        }

        private void NavigateToSettingsPage()
        {
            LogLibrary.LogUIInfo("Navigating to Settings Page.");
            try
            {
                if (_settingsPage == null)
                    _settingsPage = new SettingsPage();
                CurrentPage = _settingsPage;
                LogLibrary.LogUIInfo("Settings Page set as current page.");

                // CurrentPage = new Reports();
            }
            catch (Exception ex)
            {
                LogLibrary.LogUIError($"Error navigating to Settings Page: {ex.Message}");
                MessageBox.Show("An error occurred while trying to access the Settings. Please check the logs for more details.");
            }
        }

        private void NavigateToDashBoardPage()
        {
            LogLibrary.LogUIInfo("Navigating to Dashboard Page.");
            try
            {
                if (_dashBoardPage == null)
                    _dashBoardPage = new DashBoardPage();
                CurrentPage = _dashBoardPage;
            }
            catch (Exception ex)
            {
                LogLibrary.LogUIError($"Error navigating to Dashboard Page: {ex.Message}");
                MessageBox.Show("An error occurred while trying to access the Dashboard. Please check the logs for more details.");
            }
        }

        private void NavigateToAboutPage()
        {
            LogLibrary.LogUIInfo("Navigating to About Page.");
            try
            {
                if (_aboutPage == null)
                    _aboutPage = new AboutPage();
                CurrentPage = _aboutPage;
            }
            catch (Exception ex)
            {
                LogLibrary.LogUIError($"Error navigating to About Page: {ex.Message}");
                MessageBox.Show("An error occurred while trying to access the About Page. Please check the logs for more details.");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
