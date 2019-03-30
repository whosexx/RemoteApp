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
using System.Windows;
using AxMSTSCLib;
using MSTSCLib;
using LibRDP.WinInterop;

namespace LibRDP
{
    public partial class RDPControl : UserControl, IRemote
    {
        protected static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public RDPInfo RInfo { get; set; }
        public event DisconnectEventHandler OnDisconnectedEvent;

        private MsRdpClient8NotSafeForScripting Client { get; set; }

        private Version Version { get; set; }
        public RDPControl(RemoteInfo rinfo)
        {
            RDPInfo rdp = rinfo as RDPInfo;
            this.RInfo = rdp ?? throw new ArgumentNullException("参数不能为空！！！");
            InitializeComponent();
            
            this.Client = (MsRdpClient8NotSafeForScripting)this.rdpc.GetOcx();
            this.Version = new Version(this.Client.Version);

            CheckForIllegalCrossThreadCalls = false;
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

            //this.Version = new Version(this.Client.Version);
            //rdpc.RemoteProgram.RemoteProgramMode = true;
            //'Set keyboard hook mode to "Always hook" - only works on XP.
            //keyboardhook: i
            //此设置确定在何处应用 Windows 组合键。此设置对应于远程桌面连接“选项”的“本地资源”选项卡上“键盘”框中的选项。
            //值 设置
            //0   在本地计算机上应用。
            //1   在远程计算机上应用。
            //2   只在全屏模式下应用。
            Client.SecuredSettings2.KeyboardHookMode = 1;
            //此设置确定在何处播放声音。此设置对应于远程桌面连接“选项”的“本地资源”选项卡上“远程计算机声音”框中的选项。
            //值   设置
            //0   在客户端计算机上播放声音。
            //1   在主计算机上播放声音。
            //2   不播放声音。
            Client.SecuredSettings2.AudioRedirectionMode = 0;

            //Client.ColorDepth = 16;

            string pass = Utils.DecryptChaCha20(this.RInfo.Password, RemoteInfo.Nonce, RemoteInfo.SHA256);
            if (!string.IsNullOrWhiteSpace(this.RInfo.Password) && !string.IsNullOrWhiteSpace(pass))
                Client.AdvancedSettings2.ClearTextPassword = pass;            

            //Client.AdvancedSettings8.allowBackgroundInput = 0;
            Client.AdvancedSettings2.RDPPort = this.RInfo.Port;
            //映射磁盘
            Client.AdvancedSettings2.RedirectDrives = false;
            Client.AdvancedSettings2.RedirectPrinters = false;
            Client.AdvancedSettings2.RedirectSmartCards = false;
            Client.AdvancedSettings2.RedirectPorts = false;

            //指定是否允许位图缓冲
            //0 - 不允许
            //1 - 允许
            Client.AdvancedSettings2.PerformanceFlags = (int)(PerformanceFlags.TS_PERF_ENABLE_DESKTOP_COMPOSITION 
                | PerformanceFlags.TS_PERF_ENABLE_ENHANCED_GRAPHICS 
                | PerformanceFlags.TS_PERF_ENABLE_FONT_SMOOTHING);
            Client.AdvancedSettings2.BitmapPeristence = 1;
            //Client.AdvancedSettings2.SasSequence = 1;

            //Client.AdvancedSettings2.orderDrawThreshold = 1;
            Client.AdvancedSettings2.EnableMouse = 1;
            Client.AdvancedSettings2.SmartSizing = true;

            //Client.AdvancedSettings2.singleConnectionTimeout = 30;
            //Client.AdvancedSettings2.shutdownTimeout = 30;
            Client.AdvancedSettings2.overallConnectionTimeout = 20;
            Client.AdvancedSettings2.MinutesToIdleTimeout = 0;
            Client.AdvancedSettings2.keepAliveInterval = 60000;//in milliseconds (10,000 = 10 seconds)
            Client.AdvancedSettings2.GrabFocusOnConnect = true;            
            //Client.AdvancedSettings8.DisplayConnectionBar = true;
            Client.AdvancedSettings2.EnableWindowsKey = 1;
            //Client.AdvancedSettings2.Compress = 1;
            Client.AdvancedSettings2.EncryptionEnabled = 1;

            //Client.AdvancedSettings8.ConnectToAdministerServer = false;
            //Client.AdvancedSettings8.LoadBalanceInfo = "false";

            if (this.Version >= Versions.RDC61)
            {
                Client.AdvancedSettings7.EnableCredSspSupport = this.RInfo.EnableCredSspSupport;
                Client.AdvancedSettings8.AudioQualityMode = 0;
            }

            try
            {
                //Client.AdvancedSettings8.allowBackgroundInput = 0;                
                Client.AdvancedSettings3.MaxReconnectAttempts = 5;
                Client.AdvancedSettings3.EnableAutoReconnect = true;

                //取消全屏最小化按钮                
                Client.AdvancedSettings4.ConnectionBarShowMinimizeButton = false;
                Client.AdvancedSettings5.AuthenticationLevel = (uint)this.RInfo.AuthenticationLevel;

                //剪贴板                
                Client.AdvancedSettings6.RedirectClipboard = true;
                Client.AdvancedSettings6.PublicMode = false;

                Client.AdvancedSettings7.RelativeMouseMode = true;
                Client.AdvancedSettings7.EnableCredSspSupport = this.RInfo.EnableCredSspSupport;

                //带宽选择
                Client.AdvancedSettings8.NetworkConnectionType = (uint)this.RInfo.ConnectionType;
                Client.AdvancedSettings8.NegotiateSecurityLayer = true;
                Client.AdvancedSettings8.VideoPlaybackMode = 1;
            }
            catch { }

            Client.Domain = Environment.UserDomainName;
            Client.Server = this.RInfo.Ip; //远程桌面的IP地址或者域名
            Client.UserName = this.RInfo.User; //用户
            Client.FullScreen = this.RInfo.FullScreen;

            var sc = Screen.GetBounds(Cursor.Position);
            //var sc = Screen.FromControl(this).Bounds;
            if (this.RInfo.DesktopHeight <= 0)
                Client.DesktopHeight = (sc.Height > 0 ? sc.Height : (int)SystemParameters.PrimaryScreenHeight);
            else
                Client.DesktopHeight = this.RInfo.DesktopHeight; ;

            if (this.RInfo.DesktopWidth <= 0)
                Client.DesktopWidth = (sc.Width > 0 ? sc.Width : (int)SystemParameters.PrimaryScreenWidth);
            else
                Client.DesktopWidth = this.RInfo.DesktopWidth;

            Client.ColorDepth = (int)this.RInfo.ColorDepth;
            
            Client.OnConnected += Rdpc_OnConnected;
            Client.OnConnecting += Rdpc_OnConnecting;

            Client.OnLoginComplete += Rdpc_OnLoginComplete;
            Client.OnFatalError += Rdpc_OnFatalError;

            Client.OnDisconnected += Rdpc_OnDisconnected;
            Client.OnLeaveFullScreenMode += Rdpc_OnLeaveFullScreenMode;

            Client.OnIdleTimeoutNotification += Client_OnIdleTimeoutNotification;
            Client.OnLogonError += Rdpc_OnLogonError;
            //Client.OnWarning += Rdpc_OnWarning;

            Client.Connect();
        }

        private void Client_OnIdleTimeoutNotification()
        {
            
        }

        private void Rdpc_OnLogonError(int lError)
        {
            System.Windows.Forms.MessageBox.Show("Rdpc_OnLogonError: " + lError.ToString());
        }

        private void Rdpc_OnFatalError(int errorCode)
        {
            System.Windows.Forms.MessageBox.Show("Rdpc_OnFatalError: " + errorCode.ToString());
        }

        private void Rdpc_OnWarning(object sender, IMsTscAxEvents_OnWarningEvent e)
        {
            System.Windows.Forms.MessageBox.Show("Warning Code:" + this.Client.GetErrorDescription((uint)e.warningCode, 0));
        }

        private void Rdpc_OnLoginComplete()
        {
            //this.Client.CreateVirtualChannels(this.RInfo.Ip);
        }

        private void Rdpc_OnLeaveFullScreenMode() => this.RInfo.FullScreen = false;

        public bool IsFullScreen => this.Client.FullScreen;

        public void EnterFullScreen()
        {
            this.Client.FullScreen = true;
            this.RInfo.FullScreen = true;
            this.Client.FullScreenTitle = "正在全屏(ˉ﹃ˉ)……";
        }

        public void ExitFullScreen()
        {
            this.Client.FullScreen = false;
            this.RInfo.FullScreen = false;
        }

        public void Active() => this.rdpc.Handle.ActiveWindow();

        private void Rdpc_OnConnecting() => this.RInfo.ConnectedStatus = ConnectedStatus.正在连接;

        private void Rdpc_OnConnected()
        {
            this.TextLabel.Visible = false;
            this.RInfo.ConnectedStatus = ConnectedStatus.正常;
        }

        private void Rdpc_OnDisconnected(int discReason)
        {
            if (discReason == 264)
                this.RInfo.ConnectedStatus = ConnectedStatus.连接错误;
            else
                this.RInfo.ConnectedStatus = ConnectedStatus.断开连接;

            if(discReason == 2825)
            {
                this.RInfo.EnableCredSspSupport = true;
                this.Connect();
                return;
            }

            this.OnDisconnectedEvent?.Invoke(this.RInfo, new DisconnectEventArgs(discReason, this.Client.GetErrorDescription((uint)discReason, 0)));
        }

        public void Disconnect()
        {
            try
            {
                this.RInfo.ConnectedStatus = ConnectedStatus.断开连接;
                if (this.Client.Connected > 0)
                {                    
                    this.Client.RequestClose();
                    this.Client.Disconnect();
                    this.rdpc.Dispose();                    
                }
            }
            catch (Exception e){ Logger.Error(e.ToString()); }

            try { this.Dispose(); } catch { }
        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            // Fix for the missing focus issue on the rdp client component
            if (m.Msg == 0x0020
                && System.Environment.OSVersion.Version.Major <= 6) // WM_MOUSEACTIVATE
            {
                //this.RDPInfoMouseMoveEvent?.Invoke(this.RInfo);
                if (!this.rdpc.ContainsFocus)
                    this.rdpc.Focus();
            }

            base.WndProc(ref m);
        }
    }

    public enum AuthenticationType
    {
        //No authentication is used.    
        None = 0,

        //Certificate authentication is used.
        Certificate = 1,

        //Kerberos authentication is used.
        Kerberos = 2,

        //Both certificate and Kerberos authentication are used.
        Both = 3,
    }

    public enum PerformanceFlags : uint
    {
        //No features are disabled.
        TS_PERF_DISABLE_NOTHING = 0x00000000,


        //Wallpaper on the desktop is not displayed.
        TS_PERF_DISABLE_WALLPAPER = 0x00000001,


        //Full-window drag is disabled; only the window outline is displayed when the window is moved.
        TS_PERF_DISABLE_FULLWINDOWDRAG = 0x00000002,


        //Menu animations are disabled.
        TS_PERF_DISABLE_MENUANIMATIONS = 0x00000004,


        //Themes are disabled.
        TS_PERF_DISABLE_THEMING = 0x00000008,


        //Enable enhanced graphics.
        TS_PERF_ENABLE_ENHANCED_GRAPHICS = 0x00000010,


        //No shadow is displayed for the cursor.
        TS_PERF_DISABLE_CURSOR_SHADOW = 0x00000020,


        //Cursor blinking is disabled.
        TS_PERF_DISABLE_CURSORSETTINGS = 0x00000040,


        //Enable font smoothing.
        TS_PERF_ENABLE_FONT_SMOOTHING = 0x00000080,


        //Enable desktop composition.
        TS_PERF_ENABLE_DESKTOP_COMPOSITION = 0x00000100,


        //Set internally for clients not aware of this setting.
        TS_PERF_DEFAULT_NONPERFCLIENT_SETTING = 0x40000000,


        //Reserved and used internally by the client.
        TS_PERF_RESERVED1 = 0x80000000,
    }
}
