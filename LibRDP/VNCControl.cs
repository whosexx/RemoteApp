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
    public partial class VNCControl : UserControl, IRemote
    {
        public VNCInfo VInfo { get; set; }
        //public event Action<int, string> OnDisconnected;

        public VNCControl(RemoteInfo vinfo)
        {
            var vnc = vinfo as VNCInfo;
            this.VInfo = vnc ?? throw new ArgumentNullException("参数不能为空！");

            this.VInfo.PropertyChangedRegister(nameof(this.VInfo.IsViewOnly), PropertyChanged_Callback);
            this.VInfo.PropertyChangedRegister(nameof(this.VInfo.IsScale), PropertyChanged_Callback);

            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
        }

        private void PropertyChanged_Callback(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.VInfo.IsViewOnly))
                this.RDesk.SetInputMode(this.VInfo.IsViewOnly);
            else if (e.PropertyName == nameof(this.VInfo.IsScale))
                this.RDesk.SetScalingMode(this.VInfo.IsScale);
        }

        public event DisconnectEventHandler OnDisconnectedEvent;

        public void Connect()
        {
            this.RDesk.VncPort = this.VInfo.Port;

            if (!string.IsNullOrWhiteSpace(this.VInfo.Password))
                this.RDesk.GetPassword = RDesk_GetPasswordEvent;

            this.VInfo.ConnectedStatus = ConnectedStatus.正在连接;
            this.RDesk.Connect(this.VInfo.Ip, this.VInfo.IsViewOnly, this.VInfo.IsScale);            
        }

        private string RDesk_GetPasswordEvent()
        {
            return Utils.DecryptChaCha20(this.VInfo.Password, RemoteInfo.Nonce, RemoteInfo.SHA256);
        }

        private int ErrCode = 8888;
        public void Disconnect()
        {
            if (this.VInfo.ConnectedStatus != ConnectedStatus.正常
                && this.VInfo.ConnectedStatus != ConnectedStatus.正在连接)
                return;

            //IsManual = true;
            this.ErrCode = 0x01;
            this.VInfo.ConnectedStatus = ConnectedStatus.断开连接;
            try {
                if (this.RDesk.IsConnected)
                    this.RDesk.Disconnect();
            }
            catch { }

            this.RDesk.Dispose();
            this.Dispose();
        }

        public bool IsFullScreen { get; protected set; }

        //TODO: 
        private Form FullWin { get; set; } = null;
        private VNCToolBar Bar { get; set; } = null;
        public void EnterFullScreen()
        {
            this.IsFullScreen = true;
            this.Controls.Remove(this.RDesk);

            this.Bar = new VNCToolBar(this.RDesk.Hostname);
            this.Bar.CloseEvent += this.Bar_CloseEvent;
            this.Bar.BackEvent += this.Bar_BackEvent;

            this.FullWin = new Form()
            {
                FormBorderStyle = FormBorderStyle.None,
                WindowState = FormWindowState.Maximized,
                ShowInTaskbar = false,
                TopMost = true
            };
            this.FullWin.Controls.Add(Bar);
            this.FullWin.Controls.Add(this.RDesk);

            this.Bar.Location = new Point((int)((System.Windows.SystemParameters.PrimaryScreenWidth - this.Bar.Width) / 2), 0);           
            this.FullWin.ShowDialog();
        }

        private void Bar_BackEvent()
        {
            this.ExitFullScreen();
        }

        private void Bar_CloseEvent()
        {
            this.ErrCode = 0x01;
            this.ExitFullScreen();
            this.Disconnect();
        }

        public void ExitFullScreen()
        {
            this.IsFullScreen = false;
            if(this.FullWin != null)
            {
                this.FullWin.Controls.Remove(this.RDesk);
                this.Controls.Add(this.RDesk);
                this.FullWin.Dispose();
                this.Bar = null;
                this.FullWin = null;
            }            
        }

        public void SetTag(object tag)
        {
            
        }

        private void RDesk_ConnectComplete(object sender, VncSharp.ConnectEventArgs e)
        {
            this.VInfo.ConnectedStatus = ConnectedStatus.正常;
        }

        private void RDesk_ConnectionLost(object sender, EventArgs e)
        {
            this.VInfo.ConnectedStatus = ConnectedStatus.断开连接;
            var dis = new DisconnectEventArgs(ErrCode, "客户端已经断开连接……");
            this.OnDisconnectedEvent?.Invoke(this.VInfo, dis);
        }
    }
}
