using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibRDP
{
    public partial class TelnetControl : UserControl, IRemote
    {
        public TelnetInfo SInfo { get; set; }
        //public PrimS.Telnet.Client Client { get; set; }

        public TelnetControl(RemoteInfo sinfo)
        {
            var ssh = sinfo as TelnetInfo;
            this.SInfo = ssh ?? throw new ArgumentNullException("参数错误。。");

            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();

            //this.ConsoleUI.OnConsoleInput += this.ConsoleUI_OnConsoleInput;
            //this.ConsoleUI.IsInputEnabled = true;

            //this.ConsoleUI.InternalRichTextBox.WordWrap = this.SInfo.AutoWrap;            

            this.SInfo.PropertyChangedRegister(nameof(this.SInfo.AutoWrap), PropertyChanged_Callback);

            System.Threading.CancellationToken token = new System.Threading.CancellationToken();
            //this.Client = new PrimS.Telnet.Client(this.SInfo.Ip, this.SInfo.Port, token);            
        }

        private void PropertyChanged_Callback(object sender, PropertyChangedEventArgs args)
        {
            //if (args.PropertyName == nameof(this.SInfo.AutoWrap))
            //    this.ConsoleUI.InternalRichTextBox.WordWrap = this.SInfo.AutoWrap;            
        }

        //private async void ConsoleUI_OnConsoleInput(object sender, ConsoleLib.ConsoleEventArgs args)
        //{
        //    if (this.ConsoleUI.IsDisposed)
        //        return;

        //    if (args.Content.Trim() == "clear")
        //        this.ConsoleUI.ClearOutput(false);

        //    await this.Client.Write(args.Content);
        //}        

        public bool IsFullScreen => throw new NotImplementedException();

        public event DisconnectEventHandler OnDisconnectedEvent;
        public void Connect()
        {
            this.SInfo.ConnectedStatus = ConnectedStatus.正在连接;
            try
            {
                if (string.IsNullOrWhiteSpace(this.SInfo.User))
                    goto THREAD;

                string pass = string.Empty;
                if (!string.IsNullOrWhiteSpace(this.SInfo.Password))
                    pass = Utils.DecryptChaCha20(this.SInfo.Password, RemoteInfo.Nonce, RemoteInfo.SHA256);

                //this.Client.TryLoginAsync(this.SInfo.User, pass, 250);

                THREAD:;
                //Task.Run(() => {
                //    while (this.Client.IsConnected)
                //    {
                //        var result = this.Client.ReadAsync(new TimeSpan(0, 0, 5));
                //        if (string.IsNullOrEmpty(result.Result))
                //            continue;

                //        if (this.ConsoleUI.IsDisposed)
                //            return;

                //        this.ConsoleUI.WriteOutput(result.Result);
                //    }
                //    this.OnDisconnectedEvent?.Invoke(this.SInfo, new DisconnectEventArgs(8888, "断开链接……"));
                //});
            }
            finally
            {
                //if (this.Client.IsConnected)
                //    this.SInfo.ConnectedStatus = ConnectedStatus.正常;
            }
        }

        public void Disconnect()
        {
            if (this.SInfo.ConnectedStatus != ConnectedStatus.正常
                && this.SInfo.ConnectedStatus != ConnectedStatus.正在连接)
                return;

            this.SInfo.ConnectedStatus = ConnectedStatus.断开连接;
            //if (this.Client != null && this.Client.IsConnected)
            //{
            //    this.Client.Dispose();
            //    this.Client = null;
            //}

            //this.ConsoleUI.Dispose();
            this.Dispose();
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
