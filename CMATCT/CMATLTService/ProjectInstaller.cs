using System.ComponentModel;
using System.Diagnostics;

namespace CMATLTService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
            var eventLogInstaller = new EventLogInstaller
            {
                Log = "Application",
                Source = "ATLTService"
            };

            Installers.Add(eventLogInstaller);
        }
    }
}
