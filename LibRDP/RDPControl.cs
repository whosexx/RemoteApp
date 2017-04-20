using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace LibRDP
{
    public partial class RDPControl : UserControl, IRemote
    {
        public RDPInfo RInfo { get; set; }
        public event DisconnectEventHandler OnDisconnectedEvent;

        public RDPControl(RemoteInfo rinfo)
        {
            RDPInfo rdp = rinfo as RDPInfo;
            this.RInfo = rdp ?? throw new ArgumentNullException("参数不能为空！！！");
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
        }

        public void SetTag(object tag)
        {

        }

        public void Connect()
        {
            if (this.RInfo == null)
                throw new NullReferenceException("尚未初始化！");

            this.TextLabel.Visible = true;
            this.TextLabel.Text = "正在连接中，请稍候……";

            //rdpc.RemoteProgram.RemoteProgramMode = true;

            rdpc.AdvancedSettings2.allowBackgroundInput = 0;
            //'Set keyboard hook mode to "Always hook" - only works on XP.
            //keyboardhook: i
            //此设置确定在何处应用 Windows 组合键。此设置对应于远程桌面连接“选项”的“本地资源”选项卡上“键盘”框中的选项。
            //值 设置
            //0   在本地计算机上应用。
            //1   在远程计算机上应用。
            //2   只在全屏模式下应用。
            rdpc.SecuredSettings2.KeyboardHookMode = 1;
            //此设置确定在何处播放声音。此设置对应于远程桌面连接“选项”的“本地资源”选项卡上“远程计算机声音”框中的选项。
            //值   设置
            //0   在客户端计算机上播放声音。
            //1   在主计算机上播放声音。
            //2   不播放声音。
            rdpc.SecuredSettings2.AudioRedirectionMode = 0;

            string pass = Utils.DecryptChaCha20(this.RInfo.Password, RemoteInfo.Nonce, RemoteInfo.SHA256);
            if (!string.IsNullOrWhiteSpace(this.RInfo.Password)
                && !string.IsNullOrWhiteSpace(pass))
                rdpc.AdvancedSettings2.ClearTextPassword = pass;

            rdpc.AdvancedSettings2.RDPPort = this.RInfo.Port;
            //映射磁盘
            rdpc.AdvancedSettings2.RedirectDrives = true;
            rdpc.AdvancedSettings2.RedirectPrinters = false;
            rdpc.AdvancedSettings2.RedirectSmartCards = false;
            //指定是否允许位图缓冲
            //0 - 不允许
            //1 - 允许
            rdpc.AdvancedSettings2.BitmapPersistence = 1;
                  
            //rdpc.AdvancedSettings2.orderDrawThreshold = 1;
            rdpc.AdvancedSettings2.EnableMouse = 1;
            rdpc.AdvancedSettings2.SmartSizing = true;

            rdpc.AdvancedSettings2.singleConnectionTimeout = 30;
            rdpc.AdvancedSettings2.shutdownTimeout = 1;
            rdpc.AdvancedSettings2.overallConnectionTimeout = 30;
            rdpc.AdvancedSettings2.MinutesToIdleTimeout = 1;
            rdpc.AdvancedSettings2.keepAliveInterval = 1;
            rdpc.AdvancedSettings2.GrabFocusOnConnect = true;
            rdpc.AdvancedSettings2.DisplayConnectionBar = true;
            rdpc.AdvancedSettings2.EnableWindowsKey = 1;
            rdpc.AdvancedSettings2.Compress = 1;

            try
            {
                rdpc.AdvancedSettings3.MaxReconnectAttempts = 2;

                //取消全屏最小化按钮
                rdpc.AdvancedSettings4.ConnectionBarShowMinimizeButton = false;
                rdpc.AdvancedSettings5.AuthenticationLevel = (uint)this.RInfo.AuthenticationLevel;
                //剪贴板                
                rdpc.AdvancedSettings6.RedirectClipboard = true;
                rdpc.AdvancedSettings6.PublicMode = false;

                rdpc.AdvancedSettings7.RelativeMouseMode = true;
                rdpc.AdvancedSettings7.EnableCredSspSupport = this.RInfo.EnableCredSspSupport;
                //带宽选择
                rdpc.AdvancedSettings8.NetworkConnectionType = (uint)this.RInfo.ConnectionType;
                rdpc.AdvancedSettings8.NegotiateSecurityLayer = false;
                rdpc.AdvancedSettings8.VideoPlaybackMode = 1;
                
            }
            catch { }

            rdpc.Server = this.RInfo.Ip; //远程桌面的IP地址或者域名
            rdpc.UserName = this.RInfo.User; //用户
            rdpc.FullScreen = this.RInfo.FullScreen;

            if (this.RInfo.FullScreen || this.RInfo.DesktopHeight <= 0)
                rdpc.DesktopHeight = SystemInformation.VirtualScreen.Height;
            else
                rdpc.DesktopHeight = this.RInfo.DesktopHeight; ;

            if (this.RInfo.FullScreen || this.RInfo.DesktopWidth <= 0)
                rdpc.DesktopWidth = SystemInformation.VirtualScreen.Width;
            else
                rdpc.DesktopWidth = this.RInfo.DesktopWidth;

            rdpc.ColorDepth = (int)this.RInfo.ColorDepth;
            rdpc.Dock = DockStyle.Fill;

            rdpc.OnDisconnected += Rdpc_OnDisconnected;
            rdpc.OnConnected += Rdpc_OnConnected;
            rdpc.OnConnecting += Rdpc_OnConnecting;

            rdpc.OnLeaveFullScreenMode += Rdpc_OnLeaveFullScreenMode;
            //rdpc.OnLogonError += Rdpc_OnLogonError;
            rdpc.OnLoginComplete += Rdpc_OnLoginComplete;

            rdpc.OnWarning += Rdpc_OnWarning;
            rdpc.Connect();            
        }

        private void Rdpc_OnWarning(object sender, AxMSTSCLib.IMsTscAxEvents_OnWarningEvent e)
        {
            MessageBox.Show("Warning Code:" + this.rdpc.GetErrorDescription((uint)e.warningCode, 0));
        }

        private void Rdpc_OnLoginComplete(object sender, EventArgs e)
        {
            this.rdpc.CreateVirtualChannels(this.RInfo.Ip);
        }

        private void Rdpc_OnLeaveFullScreenMode(object sender, EventArgs e)
        {
            this.RInfo.FullScreen = false;
        }

        public bool IsFullScreen => this.rdpc.FullScreen;
        public void EnterFullScreen()
        {
            this.rdpc.FullScreen = true;
            this.RInfo.FullScreen = true;
            this.rdpc.FullScreenTitle = "正在全屏(ˉ﹃ˉ)……";
        }

        public void ExitFullScreen()
        {
            this.rdpc.FullScreen = false;
            this.RInfo.FullScreen = false;
        }

        private void Rdpc_OnConnecting(object sender, EventArgs e)
        {            
            this.RInfo.ConnectedStatus = ConnectedStatus.正在连接;
        }

        private void Rdpc_OnConnected(object sender, EventArgs e)
        {
            this.TextLabel.Visible = false;
            this.RInfo.ConnectedStatus = ConnectedStatus.正常;
        }

        private void Rdpc_OnDisconnected(object sender, AxMSTSCLib.IMsTscAxEvents_OnDisconnectedEvent e)
        {            
            if (e.discReason == 264)
                this.RInfo.ConnectedStatus = ConnectedStatus.连接错误;
            else
                this.RInfo.ConnectedStatus = ConnectedStatus.断开连接;

            if(e.discReason == 2825)
            {
                this.RInfo.EnableCredSspSupport = true;
                this.Connect();
                return;
            }

            var msg = this.rdpc.GetErrorDescription((uint)e.discReason, 0);
            this.OnDisconnectedEvent?.Invoke(this.RInfo, new DisconnectEventArgs(e.discReason, msg));
        }

        public void Disconnect()
        {
            try
            {
                this.RInfo.ConnectedStatus = ConnectedStatus.断开连接;
                if (this.rdpc.Connected > 0)
                {                    
                    this.rdpc.RequestClose();
                    this.rdpc.Disconnect();
                    this.rdpc.Dispose();                    
                }
            }
            catch (Exception e){ Logger.Error(e.ToString()); }

            try { this.Dispose(); } catch { }
        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            // Fix for the missing focus issue on the rdp client component
            if (m.Msg == 0x0020) // WM_MOUSEACTIVATE
            {
                //this.RDPInfoMouseMoveEvent?.Invoke(this.RInfo);
                if (!this.rdpc.ContainsFocus)
                    this.rdpc.Focus();
            }

            base.WndProc(ref m);
        }        
    }
}
