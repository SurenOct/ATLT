using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMATLTServiceTrayAppLaunch
{
    public static class TrayAppLaunch
    {
        static void Main(string[] args)
        {
            Process.Start("C:\\Program Files (x86)\\Thermo Fisher Scientific, Inc\\CMATLTSetup\\Bin\\CMATLTServiceTrayApp.exe");
        }
        public static void LaunchUIApp()
        {
            Process.Start("C:\\Program Files (x86)\\Thermo Fisher Scientific, Inc\\CMATLTSetup\\Bin\\CMATLTUI.exe");
        }

    }


}
