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
    public partial class EditSSHClient : Window
    {
        public RemoteClient Client { get; set; } = null;
        public event Action<RemoteClient, EditMode> EditCompletedEvent;

        public EditSSHClient(RemoteClient client = null)
        {
            this.Client = client;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Code.ItemsSource = Encoding.GetEncodings().Select(m => m.DisplayName);
            var rdp = new RemoteClient(Protocol.SSH);
            if (this.Client != null)
            {
                this.SSH.DataContext = this.Client;
                this.Password.Password = LibRDP.Utils.DecryptChaCha20(this.Client.RInfo.Password, Properties.Settings.Default.Nonce, Properties.Settings.Default.SHA256);
                this.Btn.Content = "修改";
                this.Window.Header = "修改" + Protocol.SSH + "桌面参数";
            }
            else
            {               
                this.SSH.DataContext = rdp;
                this.Btn.Content = "添加";

                this.Window.Header = "新建" + Protocol.SSH + "桌面";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            EditMode mode = EditMode.修改;
            if (this.Client == null)
                mode = EditMode.新建;

            var c = this.SSH.DataContext as RemoteClient;
            if (c == null)
                return;

            c.RInfo.Password = LibRDP.Utils.EncryptChaCha20(this.Password.Password, Properties.Settings.Default.Nonce, Properties.Settings.Default.SHA256);            
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
