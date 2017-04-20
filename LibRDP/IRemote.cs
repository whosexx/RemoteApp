using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRDP
{
    public class DisconnectEventArgs : EventArgs
    {
        public string Reason { get; set; }
        public int ErrCode { get; set; }

        public DisconnectEventArgs(int code, string reason)
        {
            this.Reason = reason;
            this.ErrCode = code;
        }
    }

    public delegate void DisconnectEventHandler(object sender, DisconnectEventArgs e);
    public interface IRemote : IDisposable
    {
        bool IsFullScreen { get; }

        event DisconnectEventHandler OnDisconnectedEvent;
        void Connect();
        void Disconnect();        

        void EnterFullScreen();

        void ExitFullScreen();

        void SetTag(object tag);
    }
}
