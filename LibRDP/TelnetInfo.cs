using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Integration;
using System.Windows.Media;

namespace LibRDP
{
    public class TelnetHost : RDPHost
    {
        public override void GenerateHost(object obj)
        {
            this.RZIndex = System.Threading.Interlocked.Decrement(ref _Index);

            var client = new LibRDP.TelnetControl(this.RInfo);
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

        public TelnetHost(LibRDP.RemoteInfo rinfo) : base(rinfo) { }
    }

    public class TelnetInfo : LibRDP.RemoteInfo
    {
        //private string _encode = Encoding.UTF8.EncodingName;
        //public string Encode
        //{
        //    get => string.IsNullOrWhiteSpace(this._encode) ? Encoding.UTF8.EncodingName : this._encode;
        //    set
        //    {
        //        if (this._encode == value)
        //            return;

        //        if (string.IsNullOrWhiteSpace(value))
        //            this._encode = Encoding.UTF8.EncodingName;
        //        else
        //            this._encode = value;
        //        this.RaisedPropertyChanged(nameof(this.Encode));
        //    }
        //}

        private bool _autowrap = true;
        public bool AutoWrap
        {
            get => this._autowrap;
            set
            {
                if (this._autowrap == value)
                    return;

                this._autowrap = value;
                this.RaisedPropertyChanged(nameof(this.AutoWrap));
            }
        }

        public TelnetInfo() : base(Protocol.TELNET)
        {
            this.Port = 23;

            this.EHost = new TelnetHost(this);
        }
    }
}
