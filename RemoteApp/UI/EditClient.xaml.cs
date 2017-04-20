using LibRDP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RemoteApp.UI
{
    /// <summary>
    /// EditParam.xaml 的交互逻辑
    /// </summary>
    public partial class EditClient : Window
    {
        public enum EditMode
        {
            修改,
            新建
        }

        public RemoteClient Client { get; set; } = null;
        public event Action<RemoteClient, EditMode> EditCompletedEvent;

        public Protocol Protocol { get; private set; }
        public EditClient(Protocol procotol, RemoteClient client = null)
        {
            this.Protocol = procotol;
            this.Client = client;
            InitializeComponent();
        }

        public ScrollViewer Edit
        {
            get
            {
                switch(this.Protocol)
                {
                    case Protocol.VNC:
                    return this.VNC;
                    case Protocol.RDP:
                    default:
                    return this.RDP;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var rdp = new RemoteClient();            
            if (this.Protocol == Protocol.VNC)
            {
                rdp.RInfo = new VNCInfo();
                this.VNC.Visibility = Visibility.Visible;
                this.RDP.Visibility = Visibility.Collapsed;
            }
            else
            {
                rdp.RInfo = new TelnetInfo();
                this.VNC.Visibility = Visibility.Collapsed;
                this.RDP.Visibility = Visibility.Visible;

                this.Colors.ItemsSource = Enum.GetValues(typeof(ColorDepth));
                this.Networks.ItemsSource = Enum.GetValues(typeof(NetworkConnectionType));
                this.AuthLevel.ItemsSource = Enum.GetValues(typeof(AuthenticationLevel));
            }

            if (this.Client != null)
            {
                this.Edit.DataContext = this.Client;
                if (this.Protocol == Protocol.VNC)
                    this.Password1.Password = Utils.DecryptChaCha20(this.Client.RInfo.Password, Properties.Settings.Default.Nonce, Properties.Settings.Default.SHA256);
                else
                {
                    this.Password.Password = Utils.DecryptChaCha20(this.Client.RInfo.Password, Properties.Settings.Default.Nonce, Properties.Settings.Default.SHA256);
                }
                this.Btn.Content = "修改";
                this.Window.Header = "修改" + this.Protocol + "桌面参数";
            }
            else
            {               
                this.Edit.DataContext = rdp;
                this.Btn.Content = "添加";

                this.Window.Header = "新建" + this.Protocol + "桌面";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            EditMode mode = EditMode.修改;
            if (this.Client == null)
                mode = EditMode.新建;

            var c = this.Edit.DataContext as RemoteClient;
            if (c == null)
                return;

            if (this.Protocol == Protocol.VNC)
                c.RInfo.Password = Utils.EncryptChaCha20(this.Password1.Password, Properties.Settings.Default.Nonce, Properties.Settings.Default.SHA256);
            else
                c.RInfo.Password = Utils.EncryptChaCha20(this.Password.Password, Properties.Settings.Default.Nonce, Properties.Settings.Default.SHA256);

            this.EditCompletedEvent?.Invoke(c, mode);
            this.Close();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            // 获取鼠标相对标题栏位置  
            this.DragMove();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
