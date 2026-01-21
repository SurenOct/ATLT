using System.Windows;
using CMATCTUI.ViewModels;
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
        public EmailViewModel ViewModel
        {
            get => DataContext as EmailViewModel;
            set => DataContext = value;
        }

        
    }
}
