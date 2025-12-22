using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using CMATCTUI.Commands;
using CMATCTUI.Pages;

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

        private void NavigateToAuditTrailPage()
        {
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
