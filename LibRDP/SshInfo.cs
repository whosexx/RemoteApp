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

            var wf = new WindowsFormsHost
            {
                Background = Brushes.Black,
                Child = client
            };

            this.RObject = client;
            this.Host = wf;
            this.Host.IsEnabled = false;
        }

        public SSHHost(RemoteInfo rinfo) : base(rinfo) { }
    }

    public class SshInfo : RemoteInfo
    {
        public override bool CanFullScreen => false;

        private bool _usessh = true;
        public bool UseOpenSSH
        {
            get => this._usessh;
            set
            {
                if (this._usessh == value)
                    return;

                this._usessh = value;
                this.RaisedPropertyChanged(nameof(this.UseOpenSSH));
            }
        }

        public SshInfo() : base(Protocol.SSH)
        {
            this.Port = 22;

            this.EHost = new SSHHost(this);
        }
    }
}
