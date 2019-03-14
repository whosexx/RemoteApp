using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using IWshRuntimeLibrary;

namespace RemoteApp
{
    public static class Utils
    {
        public static void CrtShortCut(string workpath, string FilePath, string sname, string descraption)
        {
            WshShell shell = new WshShell();
            try
            {
                //创建桌面快捷方式
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\" + sname + ".lnk");
                shortcut.TargetPath = FilePath;
                shortcut.WorkingDirectory = workpath;
                shortcut.WindowStyle = 1;
                shortcut.Description = descraption;
                shortcut.Save();
            }
            catch { }

            try
            {
                //创建开始菜单快捷方式
                IWshShortcut sc = (IWshShortcut)shell.CreateShortcut(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) + "\\" + sname + ".lnk");
                sc.TargetPath = FilePath;
                sc.WorkingDirectory = workpath;
                sc.WindowStyle = 1;
                sc.Description = descraption;
                sc.Save();
            }
            catch { }
        }



        /***************获取鼠标键盘未操作时间***************************/
        [StructLayout(LayoutKind.Sequential)]
        public struct LASTINPUTINFO
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public uint dwTime;
        }
        [DllImport("user32.dll")]
        public static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
        public static long GetSystemIdleTick()
        {
            LASTINPUTINFO vLastInputInfo = new LASTINPUTINFO();
            vLastInputInfo.cbSize = Marshal.SizeOf(vLastInputInfo);
            if (!GetLastInputInfo(ref vLastInputInfo))
                return 0;

            return Environment.TickCount - (long)vLastInputInfo.dwTime;
        }

        public static void CMD(string args, string exe = "cmd.exe", bool wait = false, bool admin = false)
        {
            string work = new System.IO.FileInfo(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName).DirectoryName;
            //System.Windows.MessageBox.Show(work);
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(exe, args);
            info.WorkingDirectory = work;
            info.CreateNoWindow = false;
            info.UseShellExecute = false;
            if(admin)
                info.Verb = "RunAs";

            var p = System.Diagnostics.Process.Start(info);
            if (wait)
                p.WaitForExit();
        }

        public static T GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            T child = default(T);//初始化child
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                    child = GetVisualChild<T>(v);

                if (child != null)
                    break;
            }

            return child;
        }

        public static T GetVisualParent<T>(DependencyObject child) where T : Visual
        {
            T parent = default(T);//初始化child
            Visual v = (Visual)VisualTreeHelper.GetParent(child);
            if (v == null)
                return parent;

            parent = v as T;
            if (parent == null)
                parent = GetVisualParent<T>(v);

            return parent;
        }

        //public static byte[] GetRandomArray(int size)
        //{
        //    if (size <= 0)
        //        return null;

        //    return Sodium.SodiumCore.GetRandomBytes(size);
        //}

        public static string CalcSHA256(string value)
        {
            if (value == null)
                return null;

            byte[] retval = Encoding.UTF8.GetBytes(value);
            //using (MD5 md5 = new MD5CryptoServiceProvider())
            //using (SHA256 sha256 = new SHA256CryptoServiceProvider())
            //{
            //    retval = sha256.ComputeHash(retval);

            //    if (retval == null)
            //        return null;

            //    return Convert.ToBase64String(CalcSHA256(retval));
            //}
            return Convert.ToBase64String(CalcSHA256(retval));
        }

        public static byte[] CalcSHA256(byte[] value)
        {
            if (value == null)
                return null;

            //using (MD5 md5 = new MD5CryptoServiceProvider())
            using (SHA256 sha256 = new SHA256CryptoServiceProvider())
            {
                value = sha256.ComputeHash(value);

                if (value == null)
                    return null;

                return value;
            }
        }

        //internal static string EncryptChaCha20(string msg, string nonce, string key)
        //{
        //    if (string.IsNullOrWhiteSpace(key) || nonce.Length != 8)
        //        return string.Empty;

        //    var n = Encoding.UTF8.GetBytes(nonce);
        //    var s = Convert.FromBase64String(key);
        //    var cliper = Encoding.UTF8.GetBytes(msg);
        //    var bs = Sodium.StreamEncryption.EncryptChaCha20(cliper, n, s);
        //    return Convert.ToBase64String(bs);
        //}

        //internal static string DecryptChaCha20(string msg, string nonce, string key)
        //{
        //    if (string.IsNullOrWhiteSpace(key) || nonce.Length != 8)
        //        return string.Empty;

        //    var b = Convert.FromBase64String(msg);
        //    var n = Encoding.UTF8.GetBytes(nonce);
        //    var s = Convert.FromBase64String(key);

        //    var bs = Sodium.StreamEncryption.DecryptChaCha20(b, n, s);
        //    return Encoding.UTF8.GetString(bs);
        //}

        //internal static string AES_Encrypt(string toEncrypt, string salt = "")
        //{
        //    if (string.IsNullOrWhiteSpace(salt))
        //        salt = "88888888";

        //    byte[] keyArray = Encoding.UTF8.GetBytes(salt);
        //    if (keyArray.Length > 24)
        //        throw new ArgumentException("长度不符合要求");

        //    byte[] key = new byte[32];
        //    byte[] rand = new byte[key.Length - keyArray.Length];
        //    new Random().NextBytes(rand);
        //    Buffer.BlockCopy(keyArray, 0, key, 0, keyArray.Length);
        //    Buffer.BlockCopy(rand, 0, key, keyArray.Length, rand.Length);            

        //    byte[] toEncryptArray = Encoding.UTF8.GetBytes(toEncrypt);
        //    using (RijndaelManaged rDel = new RijndaelManaged())
        //    {
        //        rDel.Key = key;
        //        rDel.Mode = CipherMode.ECB;
        //        rDel.Padding = PaddingMode.PKCS7;

        //        using (ICryptoTransform cTransform = rDel.CreateEncryptor())
        //        {
        //            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

        //            int total = 0;
        //            Array.Reverse(rand);
        //            rand = Encrypt(rand);
        //            Array.Reverse(rand);

        //            byte l = (byte)rand.Length;
        //            var encpt = resultArray.ToArray();
        //            byte[] seedstr = new byte[rand.Length + 1 + encpt.Length];

        //            Buffer.BlockCopy(encpt, 0, seedstr, 0, encpt.Length);
        //            total += encpt.Length;

        //            Buffer.BlockCopy(rand, 0, seedstr, total, rand.Length);
        //            total += rand.Length;

        //            seedstr[total] = l;

        //            return Convert.ToBase64String(seedstr, 0, seedstr.Length);
        //        }
        //    }
        //}

        //internal static string AES_Decrypt(string pass, string salt = "")
        //{
        //    if (string.IsNullOrWhiteSpace(pass))
        //        return string.Empty;

        //    if (string.IsNullOrWhiteSpace(salt))
        //        salt = "88888888";

        //    byte[] keyArray = Encoding.UTF8.GetBytes(salt);
        //    if (keyArray.Length > 24)
        //        throw new ArgumentException("长度不符合要求");

        //    byte[] input = Convert.FromBase64String(pass);
        //    int total = input.Length;
        //    byte len = input[total - 1];
        //    if (len >= (total - 1))
        //        return null;

        //    byte[] key = new byte[len];
        //    Buffer.BlockCopy(input, total - len - 1, key, 0, len);
            
        //    Array.Reverse(key);
        //    key = Decrypt(key);
        //    Array.Reverse(key);
        //    if ((key.Length + keyArray.Length) != 32)
        //        return null;

        //    byte[] seed = new byte[32];
        //    Buffer.BlockCopy(keyArray, 0, seed, 0, keyArray.Length);
        //    Buffer.BlockCopy(key, 0, seed, keyArray.Length, key.Length);

        //    using (RijndaelManaged rDel = new RijndaelManaged())
        //    {
        //        rDel.Key = seed;
        //        rDel.Mode = CipherMode.ECB;
        //        rDel.Padding = PaddingMode.PKCS7;

        //        using (ICryptoTransform cTransform = rDel.CreateDecryptor())
        //        {
        //            byte[] resultArray = cTransform.TransformFinalBlock(input, 0, total - len - 1);
        //            return UTF8Encoding.UTF8.GetString(resultArray);
        //        }
        //    }
        //}

        ///// <summary>
        ///// 加密
        ///// </summary>
        ///// <param name="pass"></param>
        ///// <returns></returns>
        //internal static string Encrypt(string pass)
        //{
        //    byte[] rgbkey = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
        //    byte[] rgbIV = { 0xE2, 0x35, 0xC4, 0x6F, 0x30, 0xAB, 0xB6, 0xA2 };           

        //    byte[] inputByteArray = Encoding.UTF8.GetBytes(pass);
        //    inputByteArray = Encrypt(inputByteArray);
        //    return Convert.ToBase64String(inputByteArray);
        //}

        //internal static byte[] Encrypt(byte[] pass)
        //{
        //    byte[] rgbkey = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
        //    byte[] rgbIV = { 0xE2, 0x35, 0xC4, 0x6F, 0x30, 0xAB, 0xB6, 0xA2 };

        //    using (DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider())
        //    {
        //        using (MemoryStream mStream = new MemoryStream())
        //        {
        //            using (CryptoStream cStream = new CryptoStream(mStream, dCSP.CreateEncryptor(rgbkey, rgbIV), CryptoStreamMode.Write))
        //            {
        //                cStream.Write(pass, 0, pass.Length);
        //                cStream.FlushFinalBlock();

        //                return mStream.ToArray();
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// 解密
        ///// </summary>
        ///// <param name="pass"></param>
        ///// <returns></returns>
        //internal static string Decrypt(string pass)
        //{
        //    if (string.IsNullOrWhiteSpace(pass))
        //        return null;

        //    byte[] rgbkey = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
        //    byte[] rgbIV = { 0xE2, 0x35, 0xC4, 0x6F, 0x30, 0xAB, 0xB6, 0xA2 };

        //    try
        //    {
        //        byte[] input = Convert.FromBase64String(pass);
        //        input = Decrypt(input);
        //        return Encoding.UTF8.GetString(input.ToArray());
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

        //internal static byte[] Decrypt(byte[] pass)
        //{
        //    if (pass == null || pass.Length <= 0)
        //        return null;

        //    byte[] rgbkey = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
        //    byte[] rgbIV = { 0xE2, 0x35, 0xC4, 0x6F, 0x30, 0xAB, 0xB6, 0xA2 };

        //    try
        //    {
        //        using (DESCryptoServiceProvider DCSP = new DESCryptoServiceProvider())
        //        {
        //            using (MemoryStream mStream = new MemoryStream())
        //            {
        //                using (CryptoStream cStream = new CryptoStream(mStream, DCSP.CreateDecryptor(rgbkey, rgbIV), CryptoStreamMode.Write))
        //                {
        //                    cStream.Write(pass, 0, pass.Length);
        //                    cStream.FlushFinalBlock();
        //                    return mStream.ToArray();
        //                }
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}        
    }

    /// <summary>
    ///键盘钩子
    /// </summary>
    //public class KeyboardHook
    //{
    //    public event System.Windows.Forms.KeyEventHandler KeyDownEvent;
    //    public event KeyPressEventHandler KeyPressEvent;
    //    public event System.Windows.Forms.KeyEventHandler KeyUpEvent;

    //    public delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam);


    //    static int hKeyboardHook = 0; //声明键盘钩子处理的初始值。

    //    //值在Microsoft SDK的Winuser.h里查询
    //    public const int WH_KEYBOARD_LL = 13;	//线程键盘钩子监听鼠标消息设为2，全局键盘监听鼠标消息设为13。	
    //    public const int WH_CALLWNDPROC = 4;
    //    HookProc KeyboardHookProcedure; //声明KeyboardHookProcedure作为HookProc类型。



    //    //键盘结构 
    //    [StructLayout(LayoutKind.Sequential)]
    //    public class KeyboardHookStruct
    //    {
    //        public int vkCode;	//定一个虚拟键码。该代码必须有一个价值的范围1至254 。 
    //        public int scanCode; // 指定的硬件扫描码的关键。 
    //        public int flags;  // 键标志
    //        public int time; // 指定的时间戳记的这个讯息。
    //        public int dwExtraInfo; // 指定额外信息相关的信息。
    //    }


    //    //使用此功能，安装了一个钩子。
    //    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    //    public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);


    //    //调用此函数卸载钩子。
    //    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    //    public static extern bool UnhookWindowsHookEx(int idHook);


    //    //使用此功能，通过信息钩子继续下一个钩子
    //    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    //    public static extern int CallNextHookEx(int idHook, int nCode, Int32 wParam, IntPtr lParam);

    //    // 取得当前线程编号（线程钩子需要用到） 
    //    [DllImport("kernel32.dll")]
    //    static extern int GetCurrentThreadId();


    //    public void Start()
    //    {
    //        // 安装键盘钩子
    //        if (hKeyboardHook == 0)
    //        {
    //            KeyboardHookProcedure = new HookProc(KeyboardHookProc);
    //            hKeyboardHook = SetWindowsHookEx(2, KeyboardHookProcedure, IntPtr.Zero, GetCurrentThreadId());
    //            //************************************ fasdf
    //            //键盘线程钩子 
    //            //SetWindowsHookEx( 2,KeyboardHookProcedure, IntPtr.Zero, GetCurrentThreadId());//指定要监听的线程idGetCurrentThreadId(),
    //            //键盘全局钩子,需要引用空间(using System.Reflection;) 
    //            //SetWindowsHookEx( 13,MouseHookProcedure,Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]),0); 
    //            // 
    //            //关于SetWindowsHookEx (int idHook, HookProc lpfn, IntPtr hInstance, int threadId)函数将钩子加入到钩子链表中，说明一下四个参数： 
    //            //idHook 钩子类型，即确定钩子监听何种消息，上面的代码中设为2，即监听键盘消息并且是线程钩子，如果是全局钩子监听键盘消息应设为13， 
    //            //线程钩子监听鼠标消息设为7，全局钩子监听鼠标消息设为14。lpfn 钩子子程的地址指针。如果dwThreadId参数为0 或是一个由别的进程创建的 
    //            //线程的标识，lpfn必须指向DLL中的钩子子程。 除此以外，lpfn可以指向当前进程的一段钩子子程代码。钩子函数的入口地址，当钩子钩到任何 
    //            //消息后便调用这个函数。hInstance应用程序实例的句柄。标识包含lpfn所指的子程的DLL。如果threadId 标识当前进程创建的一个线程，而且子 
    //            //程代码位于当前进程，hInstance必须为NULL。可以很简单的设定其为本应用程序的实例句柄。threaded 与安装的钩子子程相关联的线程的标识符。 
    //            //如果为0，钩子子程与所有的线程关联，即为全局钩子。 
    //            //************************************ 


    //            //如果SetWindowsHookEx失败。
    //            if (hKeyboardHook == 0)
    //            {
    //                Stop();
    //                throw new Exception("安装键盘钩子失败");
    //            }
    //        }
    //    }

    //    public void Stop()
    //    {
    //        bool retKeyboard = true;


    //        if (hKeyboardHook != 0)
    //        {
    //            retKeyboard = UnhookWindowsHookEx(hKeyboardHook);
    //            hKeyboardHook = 0;
    //        }

    //        if (!(retKeyboard)) throw new Exception("卸载钩子失败！");
    //    }



    //    //ToAscii职能的转换指定的虚拟键码和键盘状态的相应字符或字符。
    //    [DllImport("user32")]
    //    public static extern int ToAscii(int uVirtKey, //[in] 指定虚拟关键代码进行翻译。 
    //                                     int uScanCode, // [in] 指定的硬件扫描码的关键须翻译成英文。高阶位的这个值设定的关键，如果是（不压） 。
    //                                     byte[] lpbKeyState, // [in] 指针，以256字节数组，包含当前键盘的状态。每个元素（字节）的数组包含状态的一个关键。如果高阶位的字节是一套，关键是下跌（按下）。在低比特，如果设置表明，关键是对切换。在此功能，只有肘位的CAPS LOCK键是相关的。在切换状态的NUM个锁和滚动锁定键被忽略。
    //                                     byte[] lpwTransKey, // [out] 指针的缓冲区收到翻译字符或字符。 
    //                                     int fuState); // [in] Specifies whether a menu is active. This parameter must be 1 if a menu is active, or 0 otherwise. 

    //    //获取按键的状态
    //    [DllImport("user32")]
    //    public static extern int GetKeyboardState(byte[] pbKeyState);

    //    private const int WM_KEYDOWN = 0x100;//KEYDOWN 
    //    private const int WM_KEYUP = 0x101;//KEYUP
    //    private const int WM_SYSKEYDOWN = 0x104;//SYSKEYDOWN
    //    private const int WM_SYSKEYUP = 0x105;//SYSKEYUP

    //    private int KeyboardHookProc(int nCode, Int32 wParam, IntPtr lParam)
    //    {
    //        //Console.WriteLine(nCode);
    //        // 侦听键盘事件
    //        if ((nCode >= 0) && (KeyDownEvent != null || KeyUpEvent != null || KeyPressEvent != null))
    //        {
    //            //KeyboardHookStruct MyKeyboardHookStruct = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));               
    //            //键盘按下
    //            this.KeyPressEvent?.Invoke(this, new KeyPressEventArgs((char)wParam));                
    //        }

    //        //如果返回1，则结束消息，这个消息到此为止，不再传递。
    //        //如果返回0或调用CallNextHookEx函数则消息出了这个钩子继续往下传递，也就是传给消息真正的接受者 
    //        return CallNextHookEx(hKeyboardHook, nCode, wParam, lParam);
    //    }

    //    ~KeyboardHook()
    //    {
    //        Stop();
    //    }
    //}

}
