using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRDP
{
    public static class Versions
    {
        public static readonly Version RDC60 = new Version(6, 0, 6000);
        public static readonly Version RDC61 = new Version(6, 0, 6001);
        public static readonly Version RDC70 = new Version(6, 1, 7600);
        public static readonly Version RDC80 = new Version(6, 2, 9200);
        public static readonly Version RDC81 = new Version(6, 3, 9600);
    }

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
