using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Integration;
using System.Windows.Media;

namespace LibRDP
{
    public class VNCHost : RDPHost
    {
        public override void GenerateHost(object obj)
        {
            this.RZIndex = System.Threading.Interlocked.Decrement(ref _Index);

            var client = new LibRDP.VNCControl(this.RInfo);
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

        public VNCHost(LibRDP.RemoteInfo rinfo) : base(rinfo) { }
    }

    public class VNCInfo : RemoteInfo
    {
        private bool _isscale;
        public bool IsScale
        {
            get { return this._isscale; }
            set
            {
                if (this._isscale == value)
                    return;

                this._isscale = value;
                this.RaisedPropertyChanged(nameof(IsScale));
            }
        }

        public VNCInfo() : base(Protocol.VNC)
        {
            this.Port = 5900;

            this.IsViewOnly = false;
            this.IsScale = true;

            this.EHost = new VNCHost(this);
        }
    }
}
