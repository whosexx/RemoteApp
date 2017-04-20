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
    /// SetCrypto.xaml 的交互逻辑
    /// </summary>
    public partial class SetCrypto : Window
    {
        public event Action CryptoCompletedEvent;
        public SetCrypto()
        {
            InitializeComponent();
        }

        private bool ischecked = false;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Password.Focus();
            if (Utils.CalcSHA256("") == Properties.Settings.Default.SHA256)
            {
                ischecked = true;
                return;
            }
            
            Task.Run(() => {
                System.Threading.Thread.Sleep(100);
                this.Dispatcher.Invoke(() => 
                {
                    this.Hide();
                    Crypto crypto = new Crypto(true);
                    crypto.Owner = this;
                    crypto.ShowInTaskbar = false;
                    crypto.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    crypto.CryptoCheckEvent += Crypto_CryptoCheckEvent;
                    crypto.ShowDialog();

                    if (!this.ischecked)
                        this.Close();
                    else
                    {
                        this.Show();
                        this.Password.Focus();
                    }
                });
            });
        }
        
        private void Crypto_CryptoCheckEvent()
        {
            this.ischecked = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!this.ischecked)
                return;

            if (string.IsNullOrWhiteSpace(this.Password.Password))
            {
                Properties.Settings.Default.SHA256 = Utils.CalcSHA256("");
                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.SHA256 = Utils.CalcSHA256(this.Password.Password);
                Properties.Settings.Default.Save();
            }

            this.CryptoCompletedEvent?.Invoke();
            this.Close();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            // 获取鼠标相对标题栏位置  
            this.DragMove();
        }

        private void Cancle_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
