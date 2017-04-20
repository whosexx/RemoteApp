using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.ServiceModel;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace RemoteConfig
{
    public partial class MainForm : Form, RDPSrv.IRDPServiceCallback
    {
        private RDPSrv.RDPServiceClient _client;
        private RDPSrv.RDPServiceClient Client {
            get {
                if (_client == null
                    || _client.State == CommunicationState.Faulted
                    || _client.State == CommunicationState.Closed
                    || _client.State == CommunicationState.Closing)
                {
                    try { _client?.Close();} catch { }
                    _client = new RDPSrv.RDPServiceClient(NetContext);
                    _client.Open();
                }

                return _client;
            }
        }
        private InstanceContext NetContext = null;
        private Rectangle? SelectedRect = null;
        public MainForm()
        {
            InitializeComponent();
            this.SizeGripStyle = SizeGripStyle.Hide;            
            //this.FormBorderStyle = FormBorderStyle.FixedSingle;
        }

        public void NotifyMsg(string msg)
        {
            Console.WriteLine(msg);
        }

        private bool IsServerOpened = true;
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            System.Threading.Tasks.Task.Run(() => {
                try
                {
                    NetContext = new InstanceContext(this);
                    if (Client.State != CommunicationState.Opened)
                        Client.Open();

                    this.IsServerOpened = true;
                }
                catch(Exception ex)
                {
                    try {  _client?.Close();} catch { }
                    this.IsServerOpened = false;
                    MessageBox.Show(this,"连接服务端失败，程序依然可以配置监视区域，但是配置完了以后需要重启服务端才能生效。\r\n" + ex.Message, "错误", MessageBoxButtons.OK ,MessageBoxIcon.Error);
                }
            });
        }

        public void SaveConfig()
        {
            if (this.SelectedRect != null)
            {
                try
                {
                    string config = "RDPServer.exe.config";
                    XmlDocument doc = new XmlDocument();
                    doc.Load(config);

                    var node = doc.SelectSingleNode("configuration/applicationSettings/RDPServer.Properties.Settings");
                    foreach (XmlNode n in node.ChildNodes)
                    {
                        if (n.Attributes["name"].InnerText == "MonitorArea")
                        {
                            Rectangle rect = (Rectangle)this.SelectedRect;
                            string value = rect.X + "," + rect.Y + "," + rect.Width + "," + rect.Height;
                            n.InnerXml = "<value>" + value + "</value>";
                            break;
                        }
                    }
                    doc.Save(config);
                }
                catch { }
            }
        }

        protected override void OnClosed(EventArgs e)
        {            
            try
            {
                if (_client != null)
                    _client.Close();

                if (NetContext != null)
                    NetContext.Close();                   
            }
            catch { }

            base.OnClosed(e);
        }

        private void StartSelect_Click(object sender, EventArgs e)
        {
            var dlg = new CaptureScreen() { Owner = this };
            var result = dlg.ShowDialog();
            if (result == DialogResult.No)
                return;

            Rectangle rect = new Rectangle(0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            if (result == DialogResult.Yes)
            {
                rect = dlg.SelectedRectangle;
                this.SelectedRect = rect;
                if (rect.Width <= 20
                    || rect.Height <= 20)
                {
                    this.SelectedRect = null;
                    MessageBox.Show(this, "选择的区域过小，没有意义。。。", "错误", MessageBoxButtons.OK ,MessageBoxIcon.Error);
                    return;
                }

                try {
                    if (this.IsServerOpened
                        && this.SelectedRect != null)
                        this.Client.SetDesktopSharedRect(rect);
                } catch(Exception ex) { Logger.WriteLine("向服务端发送配置失败，此次配置只有在服务器重启的时候才生效。\r\n" + ex.Message); }

                this.SaveConfig();
                MessageBox.Show("选择监视区域成功[" + rect + "]");
            }
        }
    }
}
