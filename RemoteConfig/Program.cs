using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace RemoteConfig
{
    static class Program
    {
        //private static Mutex _singltonMutex;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
