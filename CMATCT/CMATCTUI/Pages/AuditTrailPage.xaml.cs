using System.Windows.Controls;
using CMATCTUI.ChildWindows;

namespace CMATCTUI.Pages
{
    /// <summary>
    /// Interaction logic for ErrorsPage.xaml
    /// </summary>
    public partial class AuditTrailPage : Page
    {
        public AuditTrailPage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            EmailFormat emailFormatWindow = new EmailFormat();
            emailFormatWindow.Show();
        }
    }
}
