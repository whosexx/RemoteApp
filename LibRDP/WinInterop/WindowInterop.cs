using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibRDP.WinInterop
{
    public class WindowInterop
    {
        [DllImport("user32.dll ")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint wndproc);

        [DllImport("user32.dll ")]
        public static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern bool MoveWindow(IntPtr hwnd, int x, int y, int cx, int cy, bool repaint);


        [DllImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
        public static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);







        public const int GWL_STYLE = (-16);
        public static void SetControlEnabled(IntPtr c, bool enabled)
        {
            if (enabled)
            { SetWindowLong(c, GWL_STYLE, (uint)(~WindowStyles.WS_DISABLED) & GetWindowLong(c, GWL_STYLE)); }
            else
            { SetWindowLong(c, GWL_STYLE, (uint)WindowStyles.WS_DISABLED | GetWindowLong(c, GWL_STYLE)); }
        }
    }
}
