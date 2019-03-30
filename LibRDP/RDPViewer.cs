using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using LibRDP.WinInterop;

namespace LibRDP
{
    public partial class RDPViewer : UserControl, IRemote
    {
        public string ConnectionString { get; set; }
        public string Password { get; set; }
        public string GroupName { get; set; }        

        public RDPViewer()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
        }

        public event DisconnectEventHandler OnDisconnectedEvent;
        public void SetArguments(dynamic args)
        {
            Type type = args.GetType();

            var pinfo = type.GetProperty("CString");
            var s = pinfo.GetValue(args);            
            this.ConnectionString = s.ToString();

            pinfo = type.GetProperty("GName");
            s = pinfo.GetValue(args);
            this.GroupName = s.ToString();

            pinfo = type.GetProperty("Password");
            s = pinfo.GetValue(args);
            this.Password = s.ToString();
        }

        public void Connect()
        {
            this.TextLabel.Visible = true;
            this.axRDPViewer.OnError += AxRDPViewer_OnError;
            this.axRDPViewer.OnConnectionFailed += AxRDPViewer_OnConnectionFailed;            
            this.axRDPViewer.OnConnectionTerminated += AxRDPViewer_OnConnectionTerminated;

            this.axRDPViewer.OnConnectionAuthenticated += AxRDPViewer_OnConnectionAuthenticated;

            this.axRDPViewer.SmartSizing = true;
            this.axRDPViewer.Connect(this.ConnectionString, this.GroupName, this.Password);
        }

        private void AxRDPViewer_OnConnectionAuthenticated(object sender, EventArgs e)
        {
            this.TextLabel.Visible = false;
        }

        private void AxRDPViewer_OnConnectionTerminated(object sender, AxRDPCOMAPILib._IRDPSessionEvents_OnConnectionTerminatedEvent e)
        {
            this.OnDisconnectedEvent?.Invoke(this, new DisconnectEventArgs(e.discReason, "服务器断开了连接。\n" + e.ToString()));
        }

        public void SetTag(dynamic size)
        {
            this.Size = size;
            this.axRDPViewer.Size = size;
            this.Update();
        }

        public void Disconnect()
        {
            try { this.axRDPViewer.Disconnect();}
            catch { }

            this.axRDPViewer.Dispose();
            this.Dispose();
        }

        public void Active() => this.axRDPViewer.Handle.ActiveWindow();

        private void AxRDPViewer_OnError(object sender, AxRDPCOMAPILib._IRDPSessionEvents_OnErrorEvent e)
        {
            this.OnDisconnectedEvent?.Invoke(this, new DisconnectEventArgs(8888, e.errorInfo.ToString()));
        }

        private void AxRDPViewer_OnConnectionFailed(object sender, EventArgs e)
        {
            this.OnDisconnectedEvent?.Invoke(this, new DisconnectEventArgs(8888, "连接服务器失败：" + e.ToString()));
        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            // Fix for the missing focus issue on the rdp client component
            if (m.Msg == 0x0020) // WM_MOUSEACTIVATE
            {
                //this.RDPInfoMouseMoveEvent?.Invoke(this.RInfo);
                if (!this.axRDPViewer.ContainsFocus)
                    this.axRDPViewer.Focus();
            }

            base.WndProc(ref m);
        }

        public bool IsFullScreen => throw new NotImplementedException();

        public void EnterFullScreen()
        {
            throw new NotImplementedException();
        }

        public void ExitFullScreen()
        {
            throw new NotImplementedException();
        }
    }
}
