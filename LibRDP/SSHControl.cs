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
using LibRDP;

namespace LibRDP
{
    public partial class SSHControl : UserControl, IRemote
    {
        protected static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public string SSHClient = "putty.exe";
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
            if (this.SInfo.UseOpenSSH)
            {
                SSHClient = @"ssh.exe";
                sb.Append($" -p {this.SInfo.Port} ");
                if (!string.IsNullOrWhiteSpace(this.SInfo.User))
                    sb.Append($" -l {this.SInfo.User} ");

                sb.Append(this.SInfo.Ip);
            }
            else
            {
                sb.Append($"-ssh -C -P {this.SInfo.Port} ");

                if (!string.IsNullOrWhiteSpace(this.SInfo.User))
                    sb.Append($"-l {this.SInfo.User} ");

                if (!string.IsNullOrWhiteSpace(pass))
                    sb.Append($"-pw {pass} ");

                sb.Append(this.SInfo.Ip);
            }

            this.Client = SSHClient.Start(sb.ToString());
            this.Client.Exited += this.Client_Exited;

            if(this.Client.FuseForm(this.Handle, 500))
                pass.SimulateSendKeys();

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

        public void Active() => this.Client.MainWindowHandle.ActiveWindow();
    }
}
