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

namespace LibRDP
{
    public partial class SSHControl : UserControl, IRemote
    {
        public SshInfo SInfo { get; set; }
        public Renci.SshNet.SshClient Client { get; set; }

        public SSHControl(RemoteInfo sinfo)
        {
            var ssh = sinfo as SshInfo;
            this.SInfo = ssh ?? throw new ArgumentNullException("参数错误。。");

            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();

            this.ConsoleUI.OnConsoleInput += this.ConsoleUI_OnConsoleInput;
            this.ConsoleUI.IsInputEnabled = true;
            this.ConsoleUI.InternalRichTextBox.WordWrap = this.SInfo.AutoWrap;
            try { this.Encode = Encoding.GetEncoding(this.SInfo.Encode); }
            catch { this.Encode = Encoding.UTF8; }

            this.SInfo.PropertyChangedRegister(nameof(this.SInfo.AutoWrap), PropertyChanged_Callback);
            this.SInfo.PropertyChangedRegister(nameof(this.SInfo.Encode), PropertyChanged_Callback);

            this.Client = new Renci.SshNet.SshClient(sinfo.Ip, sinfo.Port, sinfo.User, Utils.DecryptChaCha20(sinfo.Password, RemoteInfo.Nonce, RemoteInfo.SHA256));
            this.Client.ErrorOccurred += this.Client_ErrorOccurred;
            this.Client.ConnectionInfo.Timeout = new TimeSpan(0, 0, 2);
            this.Client.ConnectionInfo.RetryAttempts = 2;
            this.Client.ConnectionInfo.MaxSessions = 20;            
        }

        private void Client_ErrorOccurred(object sender, Renci.SshNet.Common.ExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString());
        }

        private Encoding Encode;
        private void PropertyChanged_Callback(object sender, PropertyChangedEventArgs args)
        {
            if(args.PropertyName == nameof(this.SInfo.AutoWrap))
                this.ConsoleUI.InternalRichTextBox.WordWrap = this.SInfo.AutoWrap;
            else if(args.PropertyName == nameof(this.SInfo.Encode))
            {
                try { this.Encode = Encoding.GetEncoding(this.SInfo.Encode); }
                catch { this.Encode = Encoding.UTF8; }
            }
        }

        private void ConsoleUI_OnConsoleInput(object sender, ConsoleLib.ConsoleEventArgs args)
        {
            if (this.ShellSteam == null)
                return;

            if (args.Content.Trim() == "clear")
                this.ConsoleUI.ClearOutput(false);

            ShellSteam.Write(args.Content);
            //if (args.Content.Trim() == "exit")
            //    Task.Run(() => { this.Disconnect(); });
        }        

        public bool IsFullScreen => throw new NotImplementedException();

        public event DisconnectEventHandler OnDisconnectedEvent;

        private Renci.SshNet.ShellStream ShellSteam;        
        public void Connect()
        {
            this.SInfo.ConnectedStatus = ConnectedStatus.正在连接;
            this.Client.KeepAliveInterval = this.SInfo.KeepAliveInterval;
            if (!this.Client.IsConnected)
            {
                try { this.Client.Connect(); }
                catch(Exception e)
                {
                    Logger.Error(e.ToString());
                    this.Disconnect();
                    this.OnDisconnectedEvent?.Invoke(this.SInfo, new DisconnectEventArgs(e.HResult, e.ToString()));
                    return;
                }

                IDictionary<Renci.SshNet.Common.TerminalModes, uint> termkvp = new Dictionary<Renci.SshNet.Common.TerminalModes, uint>();
                termkvp.Add(Renci.SshNet.Common.TerminalModes.ECHO, 53);
                termkvp.Add(Renci.SshNet.Common.TerminalModes.ECHOCTL, 60);

                ShellSteam = Client.CreateShellStream("xterm", 95, 30, (uint)this.Width - 20, (uint)this.Height, 1024 * 4, termkvp);
                ShellSteam.DataReceived += this.ShellSteam_DataReceived;
                this.SInfo.ConnectedStatus = ConnectedStatus.正常;
            }

            Task.Run(() => {
                while (true)
                {
                    if (!this.Client.IsConnected)
                        this.Disconnect();

                    System.Threading.Thread.Sleep(100);
                }
            });
        }

        private void ShellSteam_DataReceived(object sender, Renci.SshNet.Common.ShellDataEventArgs e)
        {
            if (this.ConsoleUI.IsDisposed)
                return;

            var output = Encode.GetString(e.Data);
            this.ConsoleUI.WriteOutput(output);
        }

        public void Disconnect()
        {
            Logger.WriteLine("退出程序。");
            if (this.SInfo.ConnectedStatus != ConnectedStatus.正常
                && this.SInfo.ConnectedStatus != ConnectedStatus.正在连接)
                return;

            this.SInfo.ConnectedStatus = ConnectedStatus.断开连接;
            if (this.ShellSteam != null)
            {
                this.ShellSteam.Dispose();
                this.ShellSteam = null;
            }

            if (this.Client != null)
            {
                this.Client.Disconnect();
                this.Client.Dispose();
                this.Client = null;
            }

            this.ConsoleUI.Dispose();
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
