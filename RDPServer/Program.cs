using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using RDPCOMAPILib;

namespace RDPServer
{
    class Program
    {
        private static RDPServerHost RHost
        {
            get { return RDPServerHost.GetInstance; }
        }

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, int mode);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out int mode);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetStdHandle(int handle);

        const int STD_INPUT_HANDLE = -10;
        const int ENABLE_QUICK_EDIT_MODE = 0x40 | 0x80;        
        static void Main(string[] args)
        {
            int mode;
            IntPtr handle = GetStdHandle(STD_INPUT_HANDLE);
            GetConsoleMode(handle, out mode);
            mode &= ~ENABLE_QUICK_EDIT_MODE;
            SetConsoleMode(handle, mode);

            RHost.Open();
            EnterMessageInputMode();
        }

        static void EnterMessageInputMode()
        {
            string line;
            while (true)
            {
                Console.Write("cmd>");
                line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                switch (line.ToLower())
                {
                    case "clr":
                    case "clear":
                    {
                        Console.Clear();
                        break;
                    }
                    case "resume":
                    {
                        RHost.Resume();
                        break;
                    }
                    case "pause":
                    {
                        RHost.Pause();
                        break;
                    }
                    case "ls":
                    {
                        RHost.ShowAttendees();
                        break;
                    }
                    case "exit":
                    {
                        RHost.Close();
                        return;
                    }
                }
            }
        }
    }
}
