using LibRDP;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace RemoteApp
{
    public class RemoteClient : INotifyPropertyChanged
    {
        public static readonly Size DefaultSize = new Size(404, 340);

        private BitmapSource rdpicon;
        [JsonIgnore]
        public virtual BitmapSource RDPIcon
        {
            get { return this.rdpicon; }
            set
            {
                this.rdpicon = value;
                this.RaisedPropertyChanged(nameof(RDPIcon));
            }
        }

        private bool _center;
        [JsonIgnore]
        public virtual bool IsCenter
        {
            get { return this._center; }
            set
            {
                this._center = value;
                this.RaisedPropertyChanged(nameof(IsCenter));
            }
        }

        private double _width;
        [JsonIgnore]
        public virtual double Width
        {
            get { return this._width; }
            set
            {
                this._width = value;
                this.RaisedPropertyChanged(nameof(Width));
            }
        }

        private double _height;
        [JsonIgnore]
        public virtual double Height
        {
            get { return this._height; }
            set
            {
                this._height = value;
                this.RaisedPropertyChanged(nameof(Height));
            }
        }

        private bool _modify;
        [JsonIgnore]
        public bool Modify
        {
            get { return this._modify; }
            set
            {
                this._modify = value;
                this.RaisedPropertyChanged(nameof(Modify));
            }
        }

        private static BitmapSource ON { get; set; }
        private static BitmapSource LINKING { get; set; }
        private static BitmapSource OFF { get; set; }
        private static BitmapSource ERROR { get; set; }
        public BitmapSource RDPBitmap
        {
            get
            {
                if (this.RInfo == null)
                    return null;
                
                switch (this.RInfo.ConnectedStatus)
                {
                    case LibRDP.ConnectedStatus.连接错误:
                    return ERROR;
                    case LibRDP.ConnectedStatus.正常:
                    return ON;
                    case LibRDP.ConnectedStatus.正在连接:
                    return LINKING;
                    case LibRDP.ConnectedStatus.断开连接:
                    return OFF;
                    default:
                    return OFF;
                }
            }
        }

        private RemoteInfo _rinfo;
        public RemoteInfo RInfo
        {
            get => _rinfo;
            set
            {
                this._rinfo = value;
                if(value != null)
                    value.PropertyChangedRegister(nameof(value.ConnectedStatus), RDPBitmap_Callback);
                this.RaisedPropertyChanged(nameof(this.RInfo));
            }
        }

        private void RDPBitmap_Callback(object sender, PropertyChangedEventArgs e)
        {
            this.RaisedPropertyChanged(nameof(this.RDPBitmap));
        }

        protected static BitmapSource COMPUTER { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisedPropertyChanged(string name) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public RemoteClient()
        {
            this.RDPIcon = COMPUTER;
            this.Width = DefaultSize.Width;
            this.Height = DefaultSize.Height;
        }

        public RemoteClient(Protocol protocol)
        {
            var r = protocol.CreateInstance();
            if(r == null)            
                throw new ArgumentException("参数不支持！！！");

            this.RInfo = r;
            this.RDPIcon = COMPUTER;

            this.Width = DefaultSize.Width;
            this.Height = DefaultSize.Height;
        }

        public RemoteClient(RemoteInfo rinfo)
        {
            this.RInfo = rinfo ?? throw new ArgumentNullException("不能为空！！！");
            this.RDPIcon = COMPUTER;

            this.Width = DefaultSize.Width;
            this.Height = DefaultSize.Height;
        }

        public bool CanFullScreen => this.RInfo.CanFullScreen;

        public bool IsFullScreen
        {
            get
            {
                var obj = this.RInfo?.EHost?.RObject;
                if (obj == null)
                    return false;

                return this.RInfo.EHost.RObject.IsFullScreen;
            }
        }

        public void EnterFullScreen()
        {
            this.RInfo.EHost?.RObject?.EnterFullScreen();
        }

        public void ExitFullScreen()
        {
            this.RInfo.EHost?.RObject?.ExitFullScreen();
        }

        public void Close()
        {
            this.RInfo.EHost.Close();
        }

        static RemoteClient()
        {
            COMPUTER = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.Computer.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            ON = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.Computer_Accept.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            LINKING = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.Computer_Connecting.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            OFF = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.Computer_Remove.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            ERROR = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.Computer_Error.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }      
    }
}
