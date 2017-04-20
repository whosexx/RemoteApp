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
    /// Crypto.xaml 的交互逻辑
    /// </summary>
    public partial class Crypto : Window
    {
        public event Action CryptoCheckEvent;
        //private class CryptoUser
        //{
        //    public string UserName { get; set; }
        //    public string Password { get; set; }

        //    public CryptoUser(string user, string pass)
        //    {
        //        this.UserName = user;
        //        this.Password = pass;
        //    }
        //}

        //private CryptoUser User = null;
        public Crypto(bool cancle = false)
        {
            InitializeComponent();
            if (cancle)
                this.Cancle.Visibility = Visibility.Visible;
            else
                this.Cancle.Visibility = Visibility.Hidden;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //var p = Utils.Decrypt(Properties.Settings.Default.MD5);
            //if (p == null)
            //    return;

            //var group = p.Split('|');
            //if (group.Length != 2)
            //    return;

            //User = new CryptoUser(group[0], Utils.Encrypt(group[1]));
            this.Password.Focus();
        }

        private bool IsCanClosed = false;
        private int MaxCount = 4;
        private int Count = 4;
        private int TimeCount = 0;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.Count < 0)
                return;

            if (string.IsNullOrWhiteSpace(this.Password.Password))
            {
                MessageBox.Show(this, "密码栏不能为空！！！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if(Utils.CalcSHA256(this.Password.Password) != Properties.Settings.Default.SHA256)
            {
                this.Password.Clear();
                this.Count--;
                if(this.Count < 0)
                {
                    Task.Run(() => {
                        try
                        {
                            this.TimeCount = (int)Math.Pow(2.0, (double)this.TimeCount);
                            int t = this.TimeCount * 60;
                            this.Dispatcher.Invoke(() => {
                                this.Btn.IsEnabled = false;
                            });

                            while (t-- > 0)
                            {
                                this.Dispatcher.Invoke(() => {
                                    this.Btn.Content = t + "S";
                                });
                                System.Threading.Thread.Sleep(1000);
                            }
                        }
                        finally
                        {
                            this.MaxCount = (this.MaxCount - 1) <= 0 ? 0 : (this.MaxCount - 1);
                            this.Count = this.MaxCount;
                            this.Dispatcher.Invoke(() => {
                                this.Btn.Content = "校验";
                                this.Btn.IsEnabled = true;
                            });
                        }
                    });

                    MessageBox.Show(this, "您的尝试次数过多，请稍等一段时间后再次尝试！！！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                MessageBox.Show(this, "您输入的密码校验有误，请确认后再次尝试，还有<" + (this.Count + 1) + ">次机会！！！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);                
                return;
            }

            this.IsCanClosed = true;
            this.CryptoCheckEvent?.Invoke();
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(!IsCanClosed)
            {
                e.Cancel = true;
                return;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (this.Owner != null)
            {
                this.Owner.Activate();
                this.Owner.Focus();                
            }            
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            // 获取鼠标相对标题栏位置  
            this.DragMove();
        }

        private void Cancle_Click(object sender, RoutedEventArgs e)
        {
            this.IsCanClosed = true;
            this.Close();
        }
    }
}
