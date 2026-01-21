using System.Threading;
using System.Windows;

namespace CMATCTUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            const string appName = "CM ATCT";
            _mutex = new Mutex(true, appName, out bool isNew);

            if (!isNew)
            {
                Shutdown();
                return;
            }

            base.OnStartup(e);
        }
    }
}
