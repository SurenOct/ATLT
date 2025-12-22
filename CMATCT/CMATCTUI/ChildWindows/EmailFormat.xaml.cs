using System.Windows;
using Microsoft.Win32;

namespace CMATCTUI.ChildWindows
{
    /// <summary>
    /// Interaction logic for EmailFormat.xaml
    /// </summary>
    public partial class EmailFormat : Window
    {
        public EmailFormat()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FileDialog fileDialog = new OpenFileDialog
            {
                Title = "Select Email Format File",
                Filter = "HTML Files (*.html;*.htm)|*.html;*.htm|All Files (*.*)|*.*"
            };
            fileDialog.ShowDialog();
        }
    }
}
