using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibRDP
{
    public static class Utils
    {
        

        //public const int GWL_STYLE = -16;
        //public const int WS_DISABLED = 0x8000000;



        


        ////public const int SWP_NOOWNERZORDER = 0x200;
        ////public const int SWP_NOREDRAW = 0x8;
        ////public const int SWP_NOZORDER = 0x4;
        ////public const int SWP_SHOWWINDOW = 0x0040;
        ////public const int WS_EX_MDICHILD = 0x40;
        ////public const int SWP_NOACTIVATE = 0x10;
        ////public const int SWP_ASYNCWINDOWPOS = 0x4000;
        ////public const int SWP_NOMOVE = 0x2;
        ////public const int SWP_NOSIZE = 0x1;
        ////public const int WM_CLOSE = 0x10;

        
        ////public const int WS_BORDER = 0x00800000;
        //public const int WS_CAPTION = 0x00C00000;
        ////const int PROCESS_ALL_ACCESS = 0x1F0FFF;
        ////const int PROCESS_VM_READ = 0x0010;
        ////const int PROCESS_VM_WRITE = 0x0020;     

        //internal const int
        //    GWL_WNDPROC = (-4),
        //    GWL_HINSTANCE = (-6),
        //    GWL_HWNDPARENT = (-8),
        //    GWL_STYLE = (-16),
        //    GWL_EXSTYLE = (-20),
        //    GWL_USERDATA = (-21),
        //    GWL_ID = (-12);
        //internal const int
        //      WS_CHILD = 0x40000000,
        //      WS_VISIBLE = 0x10000000,
        //      LBS_NOTIFY = 0x00000001,
        //      HOST_ID = 0x00000002,
        //      LISTBOX_ID = 0x00000001,
        //      WS_VSCROLL = 0x00200000,
        //      WS_BORDER = 0x00800000;


            public static readonly char[] Specials = new char[] { '+', '^', '%', '~', '(', ')' };
        public static void SimulateSendKeys(this string keys)
        {
            //模拟发送+、^、%、~及圆括号 ( ) ，要将它放在大括号 ({}) 当中
            if (string.IsNullOrEmpty(keys))
                keys = string.Empty;
                
            foreach (var sss in Specials)
                keys = keys.Replace(sss.ToString(), "{" + sss + "}");

            SendKeys.SendWait(keys);
            SendKeys.SendWait("\n\n");
            SendKeys.Flush();
        }


        

        public static string CalcSHA256(this string value)
        {
            if (value == null)
                return null;

            byte[] retval = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(CalcSHA256(retval));
        }

        public static byte[] CalcSHA256(this byte[] value)
        {
            if (value == null)
                return null;
            
            using (SHA256 sha256 = new SHA256CryptoServiceProvider())
            {
                value = sha256.ComputeHash(value);

                if (value == null)
                    return null;

                return value;
            }
        }

        public static string EncryptChaCha20(this string msg, string nonce, string key)
        {
            if (string.IsNullOrWhiteSpace(key) || nonce.Length != 8)
                return string.Empty;

            var n = Encoding.UTF8.GetBytes(nonce);
            var s = Convert.FromBase64String(key);
            var cliper = Encoding.UTF8.GetBytes(msg);
            var bs = Sodium.StreamEncryption.EncryptChaCha20(cliper, n, s);
            return Convert.ToBase64String(bs);
        }

        public static string DecryptChaCha20(this string msg, string nonce, string key)
        {
            if (string.IsNullOrWhiteSpace(key) || nonce.Length != 8)
                return string.Empty;

            var b = Convert.FromBase64String(msg);
            var n = Encoding.UTF8.GetBytes(nonce);
            var s = Convert.FromBase64String(key);

            var bs = Sodium.StreamEncryption.DecryptChaCha20(b, n, s);
            return Encoding.UTF8.GetString(bs);
        }
    }
}
