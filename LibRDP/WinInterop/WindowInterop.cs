using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibRDP.WinInterop
{
    public static class WindowInterop
    {
        [DllImport("user32.dll ")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint wndproc);

        [DllImport("user32.dll ")]
        public static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern bool MoveWindow(IntPtr hwnd, int x, int y, int cx, int cy, bool repaint);

        [DllImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
        public static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);

        [DllImport("user32.dll")]
        public static extern int SetParent(IntPtr hWndChild, IntPtr hWndNewParent);





        public const int GWL_STYLE = (-16);
        public const int SW_HIDE = 0; //{隐藏, 并且任务栏也没有最小化图标}
        public const int SW_SHOWNORMAL = 1; //{用最近的大小和位置显示, 激活}
        public const int SW_NORMAL = 1; //{同 SW_SHOWNORMAL}
        public const int SW_SHOWMINIMIZED = 2; //{最小化, 激活}
        public const int SW_SHOWMAXIMIZED = 3; //{最大化, 激活}
        public const int SW_MAXIMIZE = 3; //{同 SW_SHOWMAXIMIZED}
        public const int SW_SHOWNOACTIVATE = 4; //{用最近的大小和位置显示, 不激活}
        public const int SW_SHOW = 5; //{同 SW_SHOWNORMAL}
        public const int SW_MINIMIZE = 6; //{最小化, 不激活}
        public const int SW_SHOWMINNOACTIVE = 7; //{同 SW_MINIMIZE}
        public const int SW_SHOWNA = 8; //{同 SW_SHOWNOACTIVATE}
        public const int SW_RESTORE = 9; //{同 SW_SHOWNORMAL}
        public const int SW_SHOWDEFAULT = 10; //{同 SW_SHOWNORMAL}
        public const int SW_MAX = 10; //{同 SW_SHOWNORMAL}

        public static void SetControlEnabled(IntPtr c, bool enabled)
        {
            if (enabled)
            { SetWindowLong(c, GWL_STYLE, (uint)(~WindowStyles.WS_DISABLED) & GetWindowLong(c, GWL_STYLE)); }
            else
            { SetWindowLong(c, GWL_STYLE, (uint)WindowStyles.WS_DISABLED | GetWindowLong(c, GWL_STYLE)); }
        }

        public static Process Start(this string cmd, string arg)
        {
            ProcessStartInfo info = new ProcessStartInfo(cmd, arg)
            {
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,

            };

            var Client = Process.Start(info);
            Client.EnableRaisingEvents = true;
            return Client;
        }

        public static bool FuseForm(this Process p, IntPtr parents, int ms = 250)
        {
            bool cmd = false;
            try { p.WaitForInputIdle(); }
            catch
            {
                System.Threading.Thread.Sleep(ms);
                cmd = true;
            }

            var handle = p.MainWindowHandle;
            WindowInterop.SetParent(handle, parents);
            WindowInterop.ShowWindow(handle, WindowInterop.SW_MAXIMIZE);

            var src = WindowInterop.GetWindowLong(handle, WindowInterop.GWL_STYLE);
            src &= (uint)(~(WindowStyles.WS_CAPTION | WindowStyles.WS_BORDER | WindowStyles.WS_THICKFRAME));
            WindowInterop.SetWindowLong(handle, WindowInterop.GWL_STYLE, src);

            return cmd;
        }
    }
}
