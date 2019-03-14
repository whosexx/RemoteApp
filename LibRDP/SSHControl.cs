using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Runtime.InteropServices;
using LibRDP.WinInterop;

namespace LibRDP
{
    public partial class SSHControl : UserControl, IRemote
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        public const string Putty = "putty.exe";
        public SshInfo SInfo { get; set; }
        //public Renci.SshNet.SshClient Client { get; set; }

        public SSHControl(RemoteInfo sinfo)
        {
            var ssh = sinfo as SshInfo;
            this.SInfo = ssh ?? throw new ArgumentNullException("参数错误。。");

            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();

            //this.ConsoleUI.OnConsoleInput += this.ConsoleUI_OnConsoleInput;
            //this.ConsoleUI.IsInputEnabled = true;
            //this.ConsoleUI.InternalRichTextBox.WordWrap = this.SInfo.AutoWrap;
            try { this.Encode = Encoding.GetEncoding(this.SInfo.Encode); }
            catch(Exception ef)
            {
                Logger.Error(ef.ToString());
                this.Encode = Encoding.UTF8;
            }

            this.SInfo.PropertyChangedRegister(nameof(this.SInfo.AutoWrap), PropertyChanged_Callback);
            this.SInfo.PropertyChangedRegister(nameof(this.SInfo.Encode), PropertyChanged_Callback);

            
            //this.Client = new Renci.SshNet.SshClient(sinfo.Ip, sinfo.Port, sinfo.User, pass);
            //this.Client.ErrorOccurred += this.Client_ErrorOccurred;
            //this.Client.ConnectionInfo.Timeout = new TimeSpan(0, 0, 2);
            //this.Client.ConnectionInfo.RetryAttempts = 2;
            //this.Client.ConnectionInfo.MaxSessions = 20;            
        }

        private Encoding Encode;
        private void PropertyChanged_Callback(object sender, PropertyChangedEventArgs args)
        {
            //if(args.PropertyName == nameof(this.SInfo.AutoWrap))
            //    this.ConsoleUI.InternalRichTextBox.WordWrap = this.SInfo.AutoWrap;
            //else if(args.PropertyName == nameof(this.SInfo.Encode))
            //{
            //    try { this.Encode = Encoding.GetEncoding(this.SInfo.Encode); }
            //    catch { this.Encode = Encoding.UTF8; }
            //}
        }

        public bool IsFullScreen => throw new NotImplementedException();

        public event DisconnectEventHandler OnDisconnectedEvent;

        private Process Client { get; set; }
        public void Connect()
        {
            this.SInfo.ConnectedStatus = ConnectedStatus.正在连接;

            string pass = this.SInfo.Password;
            if (!string.IsNullOrWhiteSpace(pass))
                pass = Utils.DecryptChaCha20(this.SInfo.Password, RemoteInfo.Nonce, RemoteInfo.SHA256);

            StringBuilder sb = new StringBuilder();
            sb.Append($"-ssh -P {this.SInfo.Port} ");

            if (!string.IsNullOrWhiteSpace(this.SInfo.User))
                sb.Append($"-l {this.SInfo.User} ");

            if (!string.IsNullOrWhiteSpace(pass))
                sb.Append($"-pw {pass} ");

            sb.Append(this.SInfo.Ip);
            ProcessStartInfo info = new ProcessStartInfo(Putty, sb.ToString());
            info.UseShellExecute = false;
            info.WindowStyle = ProcessWindowStyle.Hidden;

            this.Client = Process.Start(info);
            this.Client.WaitForInputIdle();

            SetParent(this.Client.MainWindowHandle, this.Handle);
            var src = WindowInterop.GetWindowLong(this.Client.MainWindowHandle, WindowInterop.GWL_STYLE);

            WindowInterop.SetWindowLong(this.Client.MainWindowHandle, WindowInterop.GWL_STYLE, (uint)(~WindowStyles.WS_CAPTION) & src);
            
            //WindowInterop.ShowWindow(this.Client.MainWindowHandle, 3);
            
            //Utils.MoveWindow(this.Client.MainWindowHandle, 0, 0, this.Width, this.Height, true);
            //var c = Control.FromHandle(this.Client.MainWindowHandle);
            //MessageBox.Show(c?.ToString());
            //this.Controls.Add(c);

            this.SInfo.ConnectedStatus = ConnectedStatus.正常;
        }

        protected override void OnResize(EventArgs e)
        {
            if (this.Client != null)
                WindowInterop.MoveWindow(this.Client.MainWindowHandle, 0, 0, this.Width, this.Height, true);
            base.OnResize(e);
        }

        public void Disconnect()
        {
            Logger.WriteLine("退出程序。");
            if (this.SInfo.ConnectedStatus != ConnectedStatus.正常
                && this.SInfo.ConnectedStatus != ConnectedStatus.正在连接)
                return;

            this.SInfo.ConnectedStatus = ConnectedStatus.断开连接;
            this.Client.Kill();
            this.Dispose();
            //this.OnDisconnectedEvent(this.SInfo, new DisconnectEventArgs(8888, "断开连接。。。"));
        }

        public void EnterFullScreen()
        {
            throw new NotImplementedException();
        }

        public void ExitFullScreen()
        {
            throw new NotImplementedException();
        }

        public void SetTag(object tag)
        {
            throw new NotImplementedException();
        }
    }
}
