using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace LibRDP
{
    public interface IHost
    {
        bool IsReadOnly { get; set; }
        FrameworkElement Host { get; }

        IRemote RObject { get; }

        void GenerateHost(object obj);

        void Open();
        void Close();

        void DisposeHost();
    }

    public abstract class ElementHost : IHost, INotifyPropertyChanged
    {
        protected static int _Index = 0;
        private int index;
        public virtual int RZIndex
        {
            get { return this.index; }
            set
            {
                this.index = value;
                this.RaisedPropertyChanged(nameof(this.RZIndex));
            }
        }

        private bool _isreadonly = true;
        public virtual bool IsReadOnly
        {
            get => this._isreadonly;
            set
            {
                if (this.Host != null)
                    this.Host.IsEnabled = !value;

                this._isreadonly = value;
                this.RaisedPropertyChanged(nameof(this.IsReadOnly));
            }
        }

        private FrameworkElement _host;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisedPropertyChanged(string name) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public virtual FrameworkElement Host
        {
            get => this._host;
            protected set
            {
                if (this._host == value)
                    return;

                this._host = value;
                this.RaisedPropertyChanged(nameof(this.Host));
            }
        }

        private IRemote _robject;
        public virtual IRemote RObject
        {
            get => _robject;
            protected set
            {
                if (this._robject == value)
                    return;

                this._robject = value;
                this.RaisedPropertyChanged(nameof(this.RObject));
            }
        }

        public abstract void GenerateHost(object obj);        
        public abstract void DisposeHost();

        public virtual void Open() => this.RObject?.Connect();
        public virtual void Close() => this.RObject?.Disconnect();

        protected RemoteInfo RInfo;
        public ElementHost(RemoteInfo rinfo) => this.RInfo = rinfo;
    }

    //身份验证选项
    public enum AuthenticationLevel
    {
        连接且不显示警告 = 0,
        不连接 = 1,
        显示警告 = 2,
    }

    public enum ColorDepth
    {
        ColorDepth8 = 8,
        ColorDepth15 = 15,
        ColorDepth16 = 16,
        ColorDepth24 = 24,
        ColorDepth32 = 32
    }

    public enum NetworkConnectionType
    {
        //Modem (56 Kbps)
        MODEM = 0x1,

        //Low-speed broadband (256 Kbps to 2 Mbps)
        低速宽带 = 0x2,

        //Satellite (2 Mbps to 16 Mbps, with high latency)
        卫星 = 0x3,

        //High-speed broadband (2 Mbps to 10 Mbps)
        高速宽带 = 0x4,

        //Wide area network (WAN) (10 Mbps or higher, with high latency)
        WAN = 0x5,

        //Local area network (LAN) (10 Mbps or higher)
        局域网 = 0x6,

        自动检测 = 0x7

    }

    public enum ConnectedStatus
    {
        断开连接 = 0,
        正常 = 1,
        正在连接 = 2,


        连接错误,
    }

    //public abstract class RemoteOption : INotifyPropertyChanged
    //{
    //    public static readonly System.Drawing.Size DefaultSize = new System.Drawing.Size(404, 340);

    //    private BitmapSource rdpicon;
    //    [JsonIgnore]
    //    public virtual BitmapSource RDPIcon
    //    {
    //        get { return this.rdpicon; }
    //        set
    //        {
    //            this.rdpicon = value;
    //            this.RaisedPropertyChanged(nameof(RDPIcon));
    //        }
    //    }

    //    private ConnectedStatus _connected;
    //    [JsonIgnore]
    //    public virtual ConnectedStatus ConnectedStatus
    //    {
    //        get { return this._connected; }
    //        set
    //        {
    //            if (_connected == value)
    //                return;

    //            this._connected = value;
    //            this.RaisedPropertyChanged(nameof(ConnectedStatus));
    //        }
    //    }

    //    private bool _center;
    //    [JsonIgnore]
    //    public virtual bool IsCenter
    //    {
    //        get { return this._center; }
    //        set
    //        {
    //            this._center = value;
    //            this.RaisedPropertyChanged(nameof(IsCenter));
    //        }
    //    }

    //    private double _width;
    //    [JsonIgnore]
    //    public virtual double Width
    //    {
    //        get {
    //            return this._width;
    //        }
    //        set {
    //            this._width = value;
    //            this.RaisedPropertyChanged(nameof(Width));
    //        }
    //    }

    //    private double _height;
    //    [JsonIgnore]
    //    public virtual double Height
    //    {
    //        get {
    //            return this._height;
    //        }
    //        set {
    //            this._height = value;
    //            this.RaisedPropertyChanged(nameof(Height));
    //        }
    //    }

    //    private bool _isscale;
    //    [JsonIgnore]
    //    public bool IsScale
    //    {
    //        get { return this._isscale; }
    //        set
    //        {
    //            if (this._isscale == value)
    //                return;

    //            this._isscale = value;
    //            this.RaisedPropertyChanged(nameof(IsScale));
    //        }
    //    }

    //    private bool _isviewonly;
    //    [JsonIgnore]
    //    public bool IsViewOnly
    //    {
    //        get { return this._isviewonly; }
    //        set
    //        {
    //            if (this._isviewonly == value)
    //                return;

    //            this._isviewonly = value;
    //            this.RaisedPropertyChanged(nameof(IsViewOnly));
    //        }
    //    }

    //    private bool _CredSspSupport;
    //    public bool EnableCredSspSupport
    //    {
    //        get { return this._CredSspSupport; }
    //        set
    //        {
    //            if (this._CredSspSupport == value)
    //                return;
    //            this._CredSspSupport = value;
    //            this.RaisedPropertyChanged(nameof(EnableCredSspSupport));
    //        }
    //    }

    //    private NetworkConnectionType _cennectiontype;
    //    public NetworkConnectionType ConnectionType
    //    {
    //        get { return this._cennectiontype; }
    //        set
    //        {
    //            if (_cennectiontype == value)
    //                return;

    //            this._cennectiontype = value;
    //            this.RaisedPropertyChanged(nameof(ConnectionType));
    //        }
    //    }

    //    private AuthenticationLevel _authlevel;
    //    public AuthenticationLevel AuthenticationLevel
    //    {
    //        get { return this._authlevel; }
    //        set
    //        {
    //            if (_authlevel == value)
    //                return;

    //            this._authlevel = value;
    //            this.RaisedPropertyChanged(nameof(AuthenticationLevel));
    //        }
    //    }

    //    private ColorDepth _color;
    //    public ColorDepth ColorDepth
    //    {
    //        get { return this._color; }
    //        set
    //        {
    //            if (this._color == value)
    //                return;
    //            this._color = value;
    //            this.RaisedPropertyChanged(nameof(ColorDepth));
    //        }
    //    }

    //    private bool _fullscreen;
    //    public bool FullScreen
    //    {
    //        get { return this._fullscreen; }
    //        set
    //        {
    //            if (this._fullscreen == value)
    //                return;
    //            this._fullscreen = value;
    //            this.RaisedPropertyChanged(nameof(FullScreen));
    //        }
    //    }

    //    private int _dheight;
    //    /// <summary>
    //    /// 在Connect以前生效
    //    /// </summary>
    //    public int DesktopHeight
    //    {
    //        get { return this._dheight; }
    //        set
    //        {
    //            if (this._dheight == value)
    //                return;

    //            this._dheight = value;
    //            this.RaisedPropertyChanged(nameof(DesktopHeight));
    //        }
    //    }

    //    private int _dwidth;
    //    /// <summary>
    //    /// 在Connect以前生效
    //    /// </summary>
    //    public int DesktopWidth
    //    {
    //        get { return this._dwidth; }
    //        set
    //        {
    //            if (this._dwidth == value)
    //                return;

    //            this._dwidth = value;
    //            this.RaisedPropertyChanged(nameof(DesktopWidth));
    //        }
    //    }

    //    public string IP { get; set; }

    //    public event PropertyChangedEventHandler PropertyChanged;
    //    protected void RaisedPropertyChanged(string name) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    //    public RemoteOption(RemoteInfo rinfo)
    //    {
    //        this.IP = rinfo.Ip;
    //        this.Width = DefaultSize.Width;
    //        this.Height = DefaultSize.Height;

    //        this.ColorDepth = ColorDepth.ColorDepth32;
    //        this.ConnectionType = NetworkConnectionType.局域网;
    //        this.EnableCredSspSupport = false;
    //        this.AuthenticationLevel = AuthenticationLevel.连接且不显示警告;
    //    }        
    //}
}
