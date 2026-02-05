using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace CMATLTServiceTrayApp
{
    //public class TrayMessageListener : NativeWindow
    //{
    //    private readonly NotifyIcon _icon;
    //    private readonly int _taskbarRestartMsg;

    //    public TrayMessageListener(NotifyIcon icon)
    //    {
    //        _icon = icon;

    //        CreateHandle(new CreateParams());
    //        _taskbarRestartMsg = RegisterWindowMessage("TaskbarCreated");
    //    }

    //    protected override void WndProc(ref Message m)
    //    {
    //        if (m.Msg == _taskbarRestartMsg)
    //        {
    //            _icon.Visible = false;
    //            _icon.Visible = true;
    //        }

    //        base.WndProc(ref m);
    //    }

    //    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    //    private static extern int RegisterWindowMessage(string lpString);
    //}
}
