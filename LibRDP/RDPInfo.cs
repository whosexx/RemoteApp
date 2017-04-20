using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Integration;
using System.Windows.Media;

namespace LibRDP
{
    public class RDPHost : ElementHost
    {
        public override void GenerateHost(object obj = null)
        {
            this.RZIndex = System.Threading.Interlocked.Decrement(ref _Index);

            var client = new RDPControl(this.RInfo);
            client.OnDisconnectedEvent += ClientHost_OnDisconnected;

            var wf = new WindowsFormsHost()
            {
                Background = Brushes.Black,
                Child = client
            };

            this.RObject = client;
            this.Host = wf;
            this.Host.IsEnabled = false;
        }

        public static event LibRDP.DisconnectEventHandler OnDisconnectedEvent;
        protected void ClientHost_OnDisconnected(object sender, LibRDP.DisconnectEventArgs e)
        {
            Logger.WriteLine(e.ErrCode+ ", " + e.Reason);
            OnDisconnectedEvent?.Invoke(sender, e);
        }

        public void EnterFullScreen()
        {
            if (this.RObject == null)
                return;

            this.RObject.EnterFullScreen();
        }

        public void ExitFullScreen()
        {
            if (this.RObject == null)
                return;

            this.RObject.ExitFullScreen();
        }

        public override void DisposeHost()
        {
            //throw new NotImplementedException();
        }

        public RDPHost(RemoteInfo rinfo) : base(rinfo) { }
    }

    public class RDPInfo : RemoteInfo
    {
        private bool _CredSspSupport;
        public bool EnableCredSspSupport
        {
            get { return this._CredSspSupport; }
            set
            {
                if (this._CredSspSupport == value)
                    return;
                this._CredSspSupport = value;
                this.RaisedPropertyChanged(nameof(EnableCredSspSupport));
            }
        }

        private NetworkConnectionType _cennectiontype;
        public NetworkConnectionType ConnectionType
        {
            get { return this._cennectiontype; }
            set
            {
                if (_cennectiontype == value)
                    return;

                this._cennectiontype = value;
                this.RaisedPropertyChanged(nameof(ConnectionType));
            }
        }

        private AuthenticationLevel _authlevel;
        public AuthenticationLevel AuthenticationLevel
        {
            get { return this._authlevel; }
            set
            {
                if (_authlevel == value)
                    return;

                this._authlevel = value;
                this.RaisedPropertyChanged(nameof(AuthenticationLevel));
            }
        }

        private ColorDepth _color;
        public ColorDepth ColorDepth
        {
            get { return this._color; }
            set
            {
                if (this._color == value)
                    return;
                this._color = value;
                this.RaisedPropertyChanged(nameof(ColorDepth));
            }
        }

        private bool _fullscreen;
        public bool FullScreen
        {
            get { return this._fullscreen; }
            set
            {
                if (this._fullscreen == value)
                    return;
                this._fullscreen = value;
                this.RaisedPropertyChanged(nameof(FullScreen));
            }
        }

        private int _dheight;
        /// <summary>
        /// 在Connect以前生效
        /// </summary>
        public int DesktopHeight
        {
            get { return this._dheight; }
            set
            {
                if (this._dheight == value)
                    return;

                this._dheight = value;
                this.RaisedPropertyChanged(nameof(DesktopHeight));
            }
        }

        private int _dwidth;
        /// <summary>
        /// 在Connect以前生效
        /// </summary>
        public int DesktopWidth
        {
            get { return this._dwidth; }
            set
            {
                if (this._dwidth == value)
                    return;

                this._dwidth = value;
                this.RaisedPropertyChanged(nameof(this.DesktopWidth));
            }
        }

        public RDPInfo() : base(Protocol.RDP)
        {            
            this.Port = 3389;

            this.ColorDepth = ColorDepth.ColorDepth32;
            this.ConnectionType = NetworkConnectionType.局域网;
            this.EnableCredSspSupport = false;
            this.AuthenticationLevel = AuthenticationLevel.连接且不显示警告;

            this.IsViewOnly = false;
            this.EHost = new RDPHost(this);
        }
    }
}
