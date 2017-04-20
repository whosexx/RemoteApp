using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibRDP;
using System.Windows.Media;
using System.Windows.Forms.Integration;

namespace LibRDP
{
    public class SSHHost : RDPHost
    {
        public override void GenerateHost(object obj)
        {
            this.RZIndex = System.Threading.Interlocked.Decrement(ref _Index);

            var client = new LibRDP.SSHControl(this.RInfo);
            client.OnDisconnectedEvent += ClientHost_OnDisconnected;

            var wf = new WindowsFormsHost();
            wf.Background = Brushes.Black;
            wf.Child = client;

            this.RObject = client;
            this.Host = wf;
            this.Host.IsEnabled = false;
        }

        public SSHHost(LibRDP.RemoteInfo rinfo) : base(rinfo) { }
    }

    public class SshInfo : LibRDP.RemoteInfo
    {
        private TimeSpan time = new TimeSpan(0, 10, 0);
        public TimeSpan KeepAliveInterval
        {
            get => this.time;
            set
            {
                if (this.time == value)
                    return;

                this.time = value;
                this.RaisedPropertyChanged(nameof(this.KeepAliveInterval));
            }
        }

        private string _encode = Encoding.UTF8.EncodingName;
        public string Encode
        {
            get => string.IsNullOrWhiteSpace(this._encode) ? Encoding.UTF8.EncodingName : this._encode;
            set
            {
                if (this._encode == value)
                    return;

                if (string.IsNullOrWhiteSpace(value))
                    this._encode = Encoding.UTF8.EncodingName;
                else
                    this._encode = value;
                this.RaisedPropertyChanged(nameof(this.Encode));
            }
        }

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

        public SshInfo() : base(Protocol.SSH)
        {
            this.Port = 22;

            this.EHost = new SSHHost(this);
        }
    }
}
