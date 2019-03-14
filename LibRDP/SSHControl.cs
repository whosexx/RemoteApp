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
        protected static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public const string Putty = "putty.exe";
        public SshInfo SInfo { get; set; }

        public SSHControl(RemoteInfo sinfo)
        {
            var ssh = sinfo as SshInfo;
            this.SInfo = ssh ?? throw new ArgumentNullException("参数错误。。");

            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();         
        }

        public bool IsFullScreen => false;

        public event DisconnectEventHandler OnDisconnectedEvent;

        private Process Client { get; set; }
        public void Connect()
        {
            this.SInfo.ConnectedStatus = ConnectedStatus.正在连接;

            string pass = this.SInfo.Password;
            if (!string.IsNullOrWhiteSpace(pass))
                pass = Utils.DecryptChaCha20(this.SInfo.Password, RemoteInfo.Nonce, RemoteInfo.SHA256);

            StringBuilder sb = new StringBuilder();
            sb.Append($"-ssh -C -P {this.SInfo.Port} ");

            if (!string.IsNullOrWhiteSpace(this.SInfo.User))
                sb.Append($"-l {this.SInfo.User} ");

            if (!string.IsNullOrWhiteSpace(pass))
                sb.Append($"-pw {pass} ");

            sb.Append(this.SInfo.Ip);
            ProcessStartInfo info = new ProcessStartInfo(Putty, sb.ToString())
            {
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            this.Client = Process.Start(info);
            this.Client.EnableRaisingEvents = true;
            this.Client.Exited += this.Client_Exited;
            Logger.Info("WaitForInputIdle: " + this.Client.WaitForInputIdle());

            var handle = this.Client.MainWindowHandle;
            WindowInterop.SetParent(handle, this.Handle);
            WindowInterop.ShowWindow(handle, WindowInterop.SW_MAXIMIZE);
            
            var src = WindowInterop.GetWindowLong(handle, WindowInterop.GWL_STYLE);
            //src &= (uint)(~WindowStyles.WS_CAPTION);
            src &= (uint)(~(WindowStyles.WS_CAPTION | WindowStyles.WS_BORDER | WindowStyles.WS_THICKFRAME));
            WindowInterop.SetWindowLong(handle, WindowInterop.GWL_STYLE, src);     

            this.SInfo.ConnectedStatus = ConnectedStatus.正常;
        }

        private void Client_Exited(object sender, EventArgs e)
        {
            if (!this.manual)
                this.OnDisconnectedEvent?.Invoke(this.SInfo, new DisconnectEventArgs(4879, "SSH已经断开连接。。。"));
        }

        protected override void OnResize(EventArgs e)
        {
            if (this.Client != null)
                WindowInterop.MoveWindow(this.Client.MainWindowHandle, 0, 0, this.Width, this.Height, true);

            base.OnResize(e);
        }

        private bool manual = false;
        public void Disconnect()
        {
            Logger.Info("ssh 断开连接。。。");            
            if (this.SInfo.ConnectedStatus != ConnectedStatus.正常
                && this.SInfo.ConnectedStatus != ConnectedStatus.正在连接)
                return;

            this.SInfo.ConnectedStatus = ConnectedStatus.断开连接;
            if (!this.Client.HasExited)
            {
                this.manual = true;
                this.Client.Kill();
            }

            this.Dispose();
            //this.OnDisconnectedEvent(this.SInfo, new DisconnectEventArgs(8888, "断开连接。。。"));
        }

        public void EnterFullScreen()
        {
            
        }

        public void ExitFullScreen()
        {
            
        }

        public void SetTag(object tag)
        {
            
        }
    }
}
