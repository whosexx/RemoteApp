using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using Newtonsoft.Json.Converters;
using System.Reflection;

namespace LibRDP
{
    public enum Protocol : byte
    {
        Empty,
        RDP,
        VNC,
        SSH,
        TELNET
    }

    public static class ProtocolMap
    {
        private static Dictionary<Protocol, (Type Self, Type List)> Types;
        static ProtocolMap()
        {
            Types = new Dictionary<Protocol, (Type, Type)>();
            Register(Protocol.RDP, (typeof(RDPInfo), typeof(List<RDPInfo>)));
            Register(Protocol.VNC, (typeof(VNCInfo), typeof(List<VNCInfo>)));
            Register(Protocol.SSH, (typeof(SshInfo), typeof(List<SshInfo>)));
            Register(Protocol.TELNET, (typeof(TelnetInfo), typeof(List<TelnetInfo>)));
        }

        public static void Register(this Protocol p, (Type, Type) t)
        {
            if (Types.ContainsKey(p))
                return;

            Types[p] = t;
        }

        public static (Type Self, Type List) GetMapType(this Protocol p)
        {
            if (!Types.ContainsKey(p))
                return (null, null);

            return Types[p];
        }

        public static RemoteInfo CreateInstance(this Protocol p)
        {
            if (!Types.ContainsKey(p))
                return null;

            var value = Types[p];

            return Activator.CreateInstance(value.Self) as RemoteInfo;
        }
    }

    public class RemoteInfo : INotifyPropertyChanged
    {
        [JsonIgnore]
        public ElementHost EHost { get; protected set; }

        private string _uuid;
        /// <summary>
        /// 别名
        /// </summary>
        public string UUID
        {
            get { return this._uuid; }
            set
            {
                if (string.IsNullOrWhiteSpace(_uuid))
                    _uuid = Guid.NewGuid().ToString();

                if (this._uuid == value)
                    return;

                this._uuid = value;
                this.RaisedPropertyChanged(nameof(UUID));
            }
        }

        private Protocol _protocol;
        /// <summary>
        /// 协议
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public Protocol Protocol
        {
            get { return this._protocol; }
            protected set
            {
                if (this._protocol == value)
                    return;

                this._protocol = value;
                this.RaisedPropertyChanged(nameof(Protocol));
            }
        }

        private string _alias;
        /// <summary>
        /// 别名
        /// </summary>
        public string Alias
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this._alias))
                    return this.Ip;

                return this._alias;
            }
            set
            {
                if (this._alias == value)
                    return;

                this._alias = value;
                this.RaisedPropertyChanged(nameof(Alias));
            }
        }

        private string _ip;
        public string Ip
        {
            get { return this._ip; }
            set
            {
                if (this._ip == value)
                    return;

                this._ip = value;
                this.RaisedPropertyChanged(nameof(Ip));
            }
        }

        private int _port;
        public int Port
        {
            get { return this._port; }
            set
            {
                if (this._port == value)
                    return;

                this._port = value;
                this.RaisedPropertyChanged(nameof(Port));
            }
        }

        private string _user = string.Empty;
        public string User
        {
            get { return this._user; }
            set
            {
                if (this._user == value)
                    return;

                this._user = value;
                this.RaisedPropertyChanged(nameof(User));
            }
        }

        private string _pass = string.Empty;
        public string Password
        {
            get { return this._pass; }
            set
            {
                if (this._pass == value)
                    return;

                this._pass = value;
                this.RaisedPropertyChanged(nameof(Password));
            }
        }

        private ConnectedStatus _connected;
        [JsonIgnore]
        public virtual ConnectedStatus ConnectedStatus
        {
            get { return this._connected; }
            set
            {
                if (_connected == value)
                    return;

                this._connected = value;
                this.RaisedPropertyChanged(nameof(ConnectedStatus));
            }
        }

        private bool _isviewonly;
        public bool IsViewOnly
        {
            get { return this._isviewonly; }
            set
            {
                this._isviewonly = value;
                if (this.EHost != null)
                    this.EHost.IsReadOnly = value;
                this.RaisedPropertyChanged(nameof(IsViewOnly));
            }
        }

        private bool _canFull = true;
        public virtual bool CanFullScreen
        {
            get { return this._canFull; }
            set {
                this._canFull = value;
                this.RaisedPropertyChanged(nameof(CanFullScreen));
            }
        }
        
        private string _memo = string.Empty;
        public string Memo
        {
            get { return this._memo; }
            set
            {
                if (this._memo == value)
                    return;

                this._memo = value;
                this.RaisedPropertyChanged(nameof(Memo));
            }
        }

        public RemoteInfo()
        {
            this.Ip = "0.0.0.0";
            this.UUID = Guid.NewGuid().ToString();
        }

        public RemoteInfo(Protocol protocol)
        {
            this.Protocol = protocol;
            this.Ip = "0.0.0.0";
            this.UUID = Guid.NewGuid().ToString();
        }

        public static string SHA256 = string.Empty;
        public static string Nonce = string.Empty;

        public void PropertyChangedRegister(string parent, PropertyChangedEventHandler cb)
        {
            if (string.IsNullOrWhiteSpace(parent))
                return;

            if (!this.Properties.ContainsKey(parent))
                this.Properties[parent] = new List<PropertyChangedEventHandler>();

            var ps = this.Properties[parent];
            if (ps.Contains(cb))
                return;

            ps.Add(cb);
        }

        private Dictionary<string, List<PropertyChangedEventHandler>> Properties = new Dictionary<string, List<PropertyChangedEventHandler>>();
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisedPropertyChanged(string name)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            if (this.Properties.ContainsKey(name))
            {
                foreach (var v in this.Properties[name])
                    v.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
    }

    //public class RemoteInfoPropertyChangedEventArgs : PropertyChangedEventArgs
    //{
    //    public object Value { get; set; }
    //    public T GetValue<T>() => (T)Value;

    //    public RemoteInfoPropertyChangedEventArgs(string propertyName, object value) : base(propertyName) { }
    //}

    //public delegate void RemoteInfoPropertyChangedEventHandler(object sender, RemoteInfoPropertyChangedEventArgs e);
}
