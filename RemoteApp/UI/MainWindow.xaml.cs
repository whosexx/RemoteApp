using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LibRDP;
using System.Windows.Interop;

namespace RemoteApp.UI
{
    public enum Mode
    {
        标签 = 0,
        九宫格
    }

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string ConfigPath = "Config";
        public const string SConfig = "RInfos.json";

        public ObservableCollection<RemoteClient> RInfos { get; set; }
        public RemoteClient this[string uuid]
        {
            get {
                if (this.RInfos == null || this.RInfos.Count <= 0)
                    return null;

                foreach (var v in this.RInfos)
                {
                    if (v.RInfo.UUID == uuid)
                        return v;
                }

                return null;
            }
        }

        /// <summary>
        /// 当前程序版本
        /// </summary>
        public const string Version = @"0.0.0.7";
        public string Description { get; set; }
        public ObservableCollection<RemoteClient> PInfos { get; set; }
        private System.Windows.Forms.NotifyIcon ClientIcon;
        private System.Windows.Forms.ContextMenuStrip ClientMenu;
        public MainWindow()
        {
            InitializeComponent();

            #region 初始化托盘图标
            this.ClientMenu = new System.Windows.Forms.ContextMenuStrip();
            // Initialize menuItem
            System.Windows.Forms.ToolStripMenuItem ClientItem = null;
            ClientItem = new System.Windows.Forms.ToolStripMenuItem();
            ClientItem.Text = "锁定";
            ClientItem.Image = Properties.Resources.Lock;
            ClientItem.Click += ToolStripMenuItem_Click;

            this.ClientMenu.Items.Add(ClientItem);
            this.ClientMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());

            ClientItem = new System.Windows.Forms.ToolStripMenuItem();
            ClientItem.Text = "打开主面板";
            ClientItem.Click += new System.EventHandler(ToolStripMenuItem_Click);

            this.ClientMenu.Items.Add(ClientItem);
            this.ClientMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());

            ClientItem = new System.Windows.Forms.ToolStripMenuItem();
            ClientItem.Text = "退出";
            ClientItem.Image = Properties.Resources.Exit;
            ClientItem.Click += ToolStripMenuItem_Click;

            this.ClientMenu.Items.Add(ClientItem);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("远程桌面管理系统");
            sb.AppendLine("版本：" + Version);
            sb.AppendLine("进程：" + System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName);
            this.Description = sb.ToString();
            // Create the NotifyIcon.
            this.ClientIcon = new System.Windows.Forms.NotifyIcon();
            this.ClientIcon.DoubleClick += ClientIcon_DoubleClick;
            ClientIcon.Icon = Properties.Resources.viewer;
            ClientIcon.ContextMenuStrip = this.ClientMenu;
            ClientIcon.Text = this.Description;// "远程桌面监控系统";
            ClientIcon.Visible = true;
            oldstate = this.WindowState;
            #endregion
        }

        //从九宫格切回标签会导致内存泄漏
        public Selector Selector
        {
            get {
                Mode mode = (Mode)Enum.Parse(typeof(Mode), Properties.Settings.Default.Mode);
                switch (mode)
                {
                    case Mode.标签:
                    {
                        if (this.Tabs.ItemsSource == null)
                        {
                            int index = this.ClientView.SelectedIndex;
                            this.Tabs.ItemsSource = this.PInfos;
                            this.ClientView.ItemsSource = null;
                            this.ClientView.Visibility = System.Windows.Visibility.Hidden;

                            if (this.Tabs.Visibility != System.Windows.Visibility.Visible)
                            {
                                this.Tabs.Visibility = System.Windows.Visibility.Visible;
                                if (index < 0)
                                    index = this.PInfos.Count - 1;
                                this.Tabs.SelectedIndex = index;
                            }
                        }

                        return this.Tabs;
                    }
                    case Mode.九宫格:
                    {
                        if (this.ClientView.ItemsSource == null)
                        {
                            int index = this.Tabs.SelectedIndex;
                            this.ClientView.ItemsSource = this.PInfos;
                            this.Tabs.ItemsSource = null;
                            this.Tabs.Visibility = Visibility.Hidden;

                            if (this.ClientView.Visibility != Visibility.Visible)
                            {
                                this.ClientView.Visibility = Visibility.Visible;
                                this.ClientView.SelectedIndex = index;
                            }
                        }

                        return this.ClientView;
                    }
                }

                return this.Tabs;
            }
        }

        private void GridSplitter_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Properties.Settings.Default.IsHidden)
                return;

            GridSplitter split = sender as GridSplitter;
            if (split == null)
                return;

            split.Cursor = Cursors.Arrow;
            e.Handled = true;            
            if (this.ClientsPopup.IsOpen)
                return;

            if (!this.IsActive)
                return;

            this.ClientsPopup.IsOpen = true;
            this.Focus();
        }

        private void ClientIcon_DoubleClick(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized
                || this.Visibility != Visibility.Visible)
            {
                this.Show();
                this.WindowState = oldstate;
            }

            this.Topmost = true;
            this.Activate();
            this.Topmost = false;
            this.Semap.Release();
        }

        private void ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.ToolStripMenuItem item = sender as System.Windows.Forms.ToolStripMenuItem;
            if (item == null)
                return;

            switch (item.Text)
            {
                case "打开主面板":
                {
                    if (this.WindowState == WindowState.Minimized 
                        || this.Visibility != Visibility.Visible)
                    {
                        this.Show();
                        this.WindowState = oldstate;
                    }

                    this.Topmost = true;
                    this.Activate();
                    this.Topmost = false;
                    this.Semap.Release();
                    break;
                }
                case "退出":
                {
                    this.IsRealClose = true;
                    this.Close();
                    break;
                }
                case "锁定":
                {
                    this.LockApp();
                    break;
                }
            }
        }

        public void CheckUpdate(string args = null)
        {
            try
            {
                System.IO.FileInfo finfo = new System.IO.FileInfo("Update.exe");
                if (finfo.Exists)
                    Utils.CMD(args, finfo.Name);
            }catch(Exception e) { Logger.WriteLine(e.ToString()); }
        }

        #region 窗口初始化
        private WindowState oldstate;
        private void Crypto_CryptoCheckEvent()
        {
            this.IsAskLock = false;
            this.IsEnabled = true;
        }

        //请求锁定
        private bool IsAskLock = true;
        //private KeyboardHook KBHook;
        private System.Threading.Semaphore Semap = new System.Threading.Semaphore(0, 10);
        protected override void OnInitialized(EventArgs e)
        {
            if (Properties.Settings.Default.MaxWhenOpened)
                this.WindowState = WindowState.Maximized;

            base.OnInitialized(e);
        }

        public void ReadConfig()
        {
            List<RemoteInfo> remotes = new List<RemoteInfo>();
            List<string> uuids = null;

            if (this.RInfos == null)
                this.RInfos = new ObservableCollection<RemoteClient>();

            if (!System.IO.Directory.Exists(ConfigPath))
                System.IO.Directory.CreateDirectory(ConfigPath);

            var file = ConfigPath + "\\" + SConfig;
            if (System.IO.File.Exists(file))
            {
                string json = System.IO.File.ReadAllText(file);
                uuids = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(json);
            }

            var list = Enum.GetValues(typeof(Protocol)).Cast<Protocol>().ToList();
            list.Remove(Protocol.Empty);
            foreach(var p in list)
            {
                file = ConfigPath + "\\" + p.ToString() + ".json";
                if (System.IO.File.Exists(file))
                {
                    string json = System.IO.File.ReadAllText(file);
                    var rdp = Newtonsoft.Json.JsonConvert.DeserializeObject(json, p.GetMapType().List);
                    var ris = rdp as System.Collections.IList;
                    remotes.AddRange(ris.Cast<RemoteInfo>());
                }
            }                       
            
            if (uuids != null && uuids.Count > 0)
            {
                foreach(var u in uuids)
                {
                    RemoteInfo r = remotes.FirstOrDefault(m => m.UUID == u);
                    if (r == null)
                        continue;

                    this.RInfos.Add(new RemoteClient(r));
                }
            }
            else
            {
                foreach (var r in remotes)
                    this.RInfos.Add(new RemoteClient(r));
            }            
        }

        public void SaveConfig()
        {
            if (!System.IO.Directory.Exists(ConfigPath))
                System.IO.Directory.CreateDirectory(ConfigPath);

            var uuids = this.RInfos.Select(m => m.RInfo.UUID).ToList();
            var file = ConfigPath + "\\" + SConfig;
            System.IO.File.WriteAllText(file, Newtonsoft.Json.JsonConvert.SerializeObject(uuids, Newtonsoft.Json.Formatting.Indented));

            foreach(var p in this.RInfos.Select(m => m.RInfo).GroupBy(m => m.Protocol))
            {
                string path = ConfigPath + "\\" + p.Key + ".json";
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(p.ToList(), Newtonsoft.Json.Formatting.Indented);
                System.IO.File.WriteAllText(path, json);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var values = Enum.GetValues(typeof(Protocol)).Cast<Protocol>().ToList();
            values.Remove(Protocol.Empty);
            this.RemoteC.ItemsSource = values;

            ProtocolMap.Register(Protocol.Empty, (typeof(EmptyInfo), typeof(List<EmptyInfo>)));
            this.Title = this.Title + " - " + System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

            if (Properties.Settings.Default.IsHidden)
                this.LeftSplitter.Visibility = Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.SHA256))
            {
                Properties.Settings.Default.SHA256 = Utils.CalcSHA256("");
                Properties.Settings.Default.Save();
            }

            RemoteInfo.SHA256 = Properties.Settings.Default.SHA256;
            RemoteInfo.Nonce = Properties.Settings.Default.Nonce;

            RDPHost.OnDisconnectedEvent += RemoteClient_OnDisconnected;

            this.Tabs.ItemsSource = null;
            this.ClientView.ItemsSource = null;

            this.PInfos = new ObservableCollection<RemoteClient>();
            this.ReadConfig();
            if (this.RInfos != null && this.RInfos.Count > 0)
            {               
                foreach (var r in this.RInfos)
                {
                    if (string.IsNullOrWhiteSpace(r.RInfo.Password))
                        continue;

                    var p = Utils.DecryptChaCha20(r.RInfo.Password, Properties.Settings.Default.Nonce, Properties.Settings.Default.SHA256);
                    if (string.IsNullOrWhiteSpace(p) || p.FirstOrDefault(m => (uint)m < 33 || (uint)m > 126) > 0)
                        r.RInfo.Password = string.Empty;
                }
                this.SaveConfig();                
            }
            
            this.Thumbnail.ItemsSource = this.RInfos;
            this.RInfos.CollectionChanged += RInfos_CollectionChanged;
            this.Selector.Focus();

            Task.Run(() => {
                System.Threading.Thread.Sleep(100);
                if (Properties.Settings.Default.Opened != null 
                    && Properties.Settings.Default.Opened.Count > 0)
                {
                    foreach (var uuid in Properties.Settings.Default.Opened)
                    {
                        var client = this[uuid];
                        if (client == null)
                        {
                            Logger.Warn("没有找到UUID[" + uuid + "]的信息。。。");
                            continue;
                        }
                          

                        this.Dispatcher.Invoke(() => { this.OpenClient(client); });                        
                        System.Threading.Thread.Sleep(100);
                    }
                }

                if (Properties.Settings.Default.SelectedIndex > (this.PInfos.Count - 1))
                    Properties.Settings.Default.SelectedIndex = this.PInfos.Count - 1;
                this.Dispatcher.Invoke(() => {                   
                    this.Selector.SelectedIndex = Properties.Settings.Default.SelectedIndex;
                });
            });

            //检查更新
            Task.Run(() => {
                while (true)
                {
                    System.Threading.Thread.Sleep(1000 * 60);
                    if (Properties.Settings.Default.IsCheckUpdate)
                        this.CheckUpdate("invoke");                  
                }
            });                       

            ///校验操作
            Task.Run(() => {
                while (true)
                {
                    try
                    {
                        if (Utils.CalcSHA256("") != Properties.Settings.Default.SHA256)
                        {

                            if (IsAskLock || Utils.GetSystemIdleTick() >= 5 * 60 * 1000)
                            {
                                this.Dispatcher.Invoke(() => {
                                    if (this.WindowState == WindowState.Minimized
                                        || this.Visibility != Visibility.Visible)
                                    {
                                        IsAskLock = true;
                                        return;
                                    }

                                    var windows = App.Current.Windows.Cast<Window>().ToList();
                                    windows.RemoveAll(m => m.GetType() == this.GetType());
                                    if (windows.FirstOrDefault(m => m.GetType() == typeof(Crypto)) != null)
                                    {
                                        //this.LastMove = DateTime.Now;
                                        return;
                                    }

                                    foreach (Window win in windows)
                                        win.Close();

                                    foreach (var p in this.PInfos)
                                    {
                                        if (p.IsFullScreen)
                                            p.ExitFullScreen();
                                    }
                                    this.IsEnabled = false;

                                    Crypto crypto = new Crypto()
                                    {
                                        Owner = this,
                                        ShowInTaskbar = false,
                                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                                    };
                                    crypto.CryptoCheckEvent += Crypto_CryptoCheckEvent;
                                    crypto.ShowDialog();
                                });
                            }
                        }
                        else
                            IsAskLock = false;

                    }
                    catch(Exception ef) { Logger.Error(ef.ToString()); }
                    this.Semap.WaitOne(2000);
                }
            });
        }

        private void RInfos_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //TODO:
            this.SaveConfig();
        }

        public void LockApp()
        {
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.SHA256))
            {
                MessageBox.Show(this, "请先设置密码保护了以后在锁定！！！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            this.IsAskLock = true;
            this.Semap.Release();
        }

        public void EnterAppFullScreen()
        {           
            this.top = this.Top;
            this.left = this.Left;
            this.height = this.ActualHeight;
            this.width = this.ActualWidth;

            this.mode = this.ResizeMode;
            this.style = this.WindowStyle;
            this.state = this.WindowState;

            this.Menu.Visibility = Visibility.Collapsed;
            this.WindowState = WindowState.Normal;
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;

            var IntPtrhwnd = new WindowInteropHelper(this).Handle;
            var screen = System.Windows.Forms.Screen.FromHandle(IntPtrhwnd);

            this.Top = screen.Bounds.Top;
            this.Left = screen.Bounds.Left;
            this.Width = screen.Bounds.Width;
            this.Height = screen.Bounds.Height;

            this.IsFullScreen = true;
        }

        private bool IsFullScreen;
        private ResizeMode mode;
        private WindowStyle style;
        private WindowState state;
        private double top;
        private double left;
        private double width;
        private double height;
        public void ExitAppFullScreen()
        {
            this.Menu.Visibility = Visibility.Visible;

            this.WindowState = state;
            this.WindowStyle = style;
            this.ResizeMode = mode;

            this.Top = this.top;
            this.Left = this.left;
            this.Height = this.height;
            this.Width = this.width;
            this.IsFullScreen = false;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.ExitAppFullScreen();
            else if (e.Key == Key.F11)
            {
                if(this.IsFullScreen)
                    this.ExitAppFullScreen();
                else
                    this.EnterAppFullScreen();
            }else if(e.Key == Key.L)
            {
                if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl))
                    this.LockApp();
            }
                
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.IsLoaded
                && this.WindowState != WindowState.Minimized
                && this.PInfos != null
                && this.PInfos.Count > 0)
                this.ResizeWindow(this.PInfos.Count);
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            //此方法 太牛叉了！简直神来之笔，参考链接1
            //var mi = typeof(Popup).GetMethod("UpdatePosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            //var pop = Utils.GetVisualChild<Popup>(this.ClientView);
            //if (pop == null)
            //    return;

            //mi.Invoke(pop, null);
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (this.ClientsPopup.IsOpen)
                this.ClientsPopup.IsOpen = false;
        }

        public bool IsRealClose = false;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Properties.Settings.Default.MinWhenClosed && !IsRealClose)
            {
                this.Hide();
                e.Cancel = true;
            }                
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //KBHook.Stop();
            Properties.Settings.Default.Opened = new System.Collections.Specialized.StringCollection();
            foreach(var p in this.PInfos.ToList())
            {
                if (p.RInfo.Protocol == Protocol.Empty)
                {
                    this.PInfos.Remove(p);                                     
                    Logger.WriteLine("Remove Type:" + p.GetType() + "," + p.RInfo.Ip);
                }
            }

            Properties.Settings.Default.Opened.AddRange(this.PInfos.Select(m => m.RInfo.UUID).ToArray());
            Properties.Settings.Default.SelectedIndex = this.Selector.SelectedIndex < 0 ? 0 : this.Selector.SelectedIndex;
            Properties.Settings.Default.Save();

            foreach (var p in this.PInfos.ToList())
                p.Close();

            this.SaveConfig();
            Environment.Exit(0);
        }

        /// <summary>
        /// callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arg2"></param>
        private void RemoteClient_OnDisconnected(object sender, DisconnectEventArgs e)
        {
            RemoteInfo rinfo = sender as RemoteInfo;
            Logger.WriteLine(rinfo.Ip + ": " + e.ErrCode + "," + e.Reason);
            WindowState state = WindowState.Normal;
            this.Dispatcher.Invoke(() => {
                state = this.WindowState;
                this.CloseClient(this[rinfo.UUID]);
                if (string.IsNullOrWhiteSpace(e.Reason))
                    return;

                if (e.ErrCode == 0x01
                    || e.ErrCode == 0x02)
                    return;

                string emsg = "计算机： " + rinfo.Ip + "\n错误代码：0x" + e.ErrCode.ToString("x4") + "(" + e.ErrCode + ")\n" + e.Reason;
                if (state == WindowState.Minimized)
                    this.ClientIcon.ShowBalloonTip(1, "错误", emsg, System.Windows.Forms.ToolTipIcon.Error);
                else
                    MessageBox.Show(this, emsg, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            });            
        }

        public void SwitchMode()
        {
            if (this.Preview)
                return;

            Mode m = (Mode)Enum.Parse(typeof(Mode), Properties.Settings.Default.Mode);
            switch (m)
            {
                case Mode.九宫格:
                {
                    Properties.Settings.Default.Mode = Mode.标签.ToString();
                    break;
                }
                case Mode.标签:
                {
                    Properties.Settings.Default.Mode = Mode.九宫格.ToString();
                    break;
                }
            }

            this.Selector.Focus();
            this.ResizeWindow(this.PInfos.Count);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if(e.Key == Key.Tab 
                && (e.KeyboardDevice.IsKeyDown(Key.LeftShift) 
                || e.KeyboardDevice.IsKeyDown(Key.RightShift)))
            {               
                this.SwitchMode();
                e.Handled = true;
                return;
            }
            base.OnPreviewKeyDown(e);
        }
        #endregion        

        #region CMD
        private void CMD_Connect_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            RemoteClient client = e.Parameter as RemoteClient;
            if (client == null || this.Preview)
            {
                e.CanExecute = false;
                return;
            }

            switch (client.RInfo.ConnectedStatus)
            {
                case ConnectedStatus.正在连接:
                case ConnectedStatus.正常:
                e.CanExecute = false;
                break;
                case ConnectedStatus.连接错误:
                case ConnectedStatus.断开连接:
                e.CanExecute = true;
                break;
            }
        }

        private void CMD_Connect_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var client = e.Parameter as RemoteClient;
            if (client == null)
                client = this.Thumbnail.SelectedItem as RemoteClient;

            if (client == null)
                return;

            this.OpenClient(client);            
        }

        private void CMD_DisConnected_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            RemoteClient client = e.Parameter as RemoteClient;
            if (client == null)
                client = this.Thumbnail.SelectedItem as RemoteClient;

            if (client == null || this.Preview)
            {
                e.CanExecute = false;
                return;
            }

            switch (client.RInfo.ConnectedStatus)
            {
                case ConnectedStatus.正在连接:
                case ConnectedStatus.正常:
                e.CanExecute = true;
                break;
                case ConnectedStatus.断开连接:
                e.CanExecute = false;
                break;
            }
        }

        private void CMD_DisConnected_Executed(object sender, ExecutedRoutedEventArgs e)
        {            
            var client = e.Parameter as RemoteClient;
            if (client == null)
                client = this.Thumbnail.SelectedItem as RemoteClient;

            if (client == null)
                return;

            this.CloseClient(client);            
        }        

        private void CMD_Delete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RemoteClient client = e.Parameter as RemoteClient;
            if (client == null)
                client = this.Thumbnail.SelectedItem as RemoteClient;

            if (client == null || client.IsCenter)
                return;

            if (MessageBox.Show("确定移除这台电脑的配置？", "警告", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                return;

            this.CloseClient(client);
            this.RInfos.Remove(client);
        }

        private void CMD_Operable_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            RemoteClient client = e.Parameter as RemoteClient;
            if (client == null)
                client = this.Selector.SelectedItem as RemoteClient;

            if (client == null)
            {
                e.CanExecute = false;
                return;
            }

            switch (client.RInfo.ConnectedStatus)
            {
                case ConnectedStatus.正在连接:
                case ConnectedStatus.正常:
                e.CanExecute = true;
                break;
                case ConnectedStatus.断开连接:
                e.CanExecute = false;
                break;
            }
        }

        private void CMD_Shutdown_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //MonitorClient client = e.Parameter as MonitorClient;
            //if (client == null)
            //    return;

            //client.Resume();
        }

        private void CMD_Reboot_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        #region 修改别名
        /// <summary>
        /// 修改别名
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CMD_ModifyAlias_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RemoteClient client = e.Parameter as RemoteClient;
            if (client == null)
                client = this.Thumbnail.SelectedItem as RemoteClient;

            if (client == null)
                return;

            if (!client.Modify)
            {
                client.Modify = true;
                ListBoxItem lvi = this.Thumbnail.ItemContainerGenerator.ContainerFromItem(client) as ListBoxItem;//获取ListViewItem
                if (lvi == null)
                    return;

                var popup = Utils.GetVisualChild<Popup>(lvi);
                if (popup == null)
                    return;

                var b = popup.Child as Border;
                if (b == null)
                    return;

                var tb = b.Child as TextBox;
                if (tb == null)
                    return;

                tb.Focus();
                tb.SelectAll();
            }
            else
                client.Modify = false;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox tb = sender as TextBox;
                if (tb == null)
                    return;

                RemoteClient view = tb.DataContext as RemoteClient;
                if (view == null)
                    return;

                if (view.Modify)
                    view.Modify = false;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb == null)
                return;

            RemoteClient view = tb.DataContext as RemoteClient;
            if (view == null)
                return;

            if (view.Modify)
                view.Modify = false;
        }
        #endregion

        private void CMD_Edit_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            RemoteClient client = e.Parameter as RemoteClient;
            if (client == null)
            {
                e.CanExecute = false;
                return;
            }

            if (client.RInfo is EmptyInfo)
            {
                e.CanExecute = false;
                return;
            }

            e.CanExecute = true;
        }

        private void CMD_Edit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RemoteClient client = e.Parameter as RemoteClient;
            if (client == null)
                client = this.Thumbnail.SelectedItem as RemoteClient;

            if (client == null)
                return;

            if (!(client is RemoteClient))
                return;

            var edit = this.GetClientUI(client.RInfo.Protocol, client);
            edit.ShowDialog();
        }        

        private void CMD_New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RemoteClient client = new RemoteClient();
            EmptyInfo ef = new EmptyInfo(this.RInfos);
            client.RInfo = ef;
            this.OpenClient(client, this.Resources["box2"]);
        }

        private void CMD_Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RemoteClient client = new RemoteClient();
            client.RInfo = new RDPInfo();

            this.RInfos.Add(client);
        }

        private void CMD_Max_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            RemoteClient client = e.Parameter as RemoteClient;
            if (client == null)
                client = this.Thumbnail.SelectedItem as RemoteClient;

            if (client == null || this.Preview)
            {
                e.CanExecute = false;
                return;
            }

            if(client.RInfo.IsViewOnly)
            {
                e.CanExecute = false;
                return;
            }

            switch (client.RInfo.ConnectedStatus)
            {
                case ConnectedStatus.正在连接:
                case ConnectedStatus.正常:
                e.CanExecute = true;
                break;
                case ConnectedStatus.断开连接:
                e.CanExecute = false;
                break;
            }
        }

        private void CMD_Max_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RemoteClient client = e.Parameter as RemoteClient;
            if (client == null)
                client = this.Thumbnail.SelectedItem as RemoteClient;

            if (client == null)
                return;

            client.EnterFullScreen();
        }

        private void CMD_Close_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RemoteClient client = e.Parameter as RemoteClient;
            if (client == null)
                client = this.Selector.SelectedItem as RemoteClient;

            if (client == null)
                return;

            this.CloseClient(client);
        }

        private void CMD_CloseOther_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RemoteClient cinfo = e.Parameter as RemoteClient;
            if (cinfo == null)
                cinfo = this.Selector?.SelectedItem as RemoteClient;

            Task.Run(() => {
                foreach (RemoteClient item in this.PInfos.ToList())
                {
                    if (cinfo != null && cinfo == item)
                        continue;

                    if (item == null)
                        continue;

                    this.Dispatcher.Invoke(() => {
                        this.CloseClient(item);
                    });
                    System.Threading.Thread.Sleep(20);
                }
            });
        }

        private void CMD_CloseAll_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Task.Run(() => {
                foreach (var item in this.PInfos.ToList())
                {
                    if (item == null)
                        continue;

                    this.Dispatcher.Invoke(() => {
                        this.CloseClient(item);
                    });

                    System.Threading.Thread.Sleep(20);
                }
            });
        }

        private void CMD_Center_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var client = e.Parameter as RemoteClient;
            if (client == null)
                client = this.Selector.SelectedItem as RemoteClient;

            if (client == null)
                return;

            this.AlignCenter(client);
        }

        private bool Preview = false;
        public void AlignCenter(RemoteClient client = null)
        {
            if (client == null)
                return;

            if (!this.Preview && !client.IsCenter)
            {
                client.IsCenter = true;
                this.Preview = true;
                this.ClientView.ItemsSource = null;
                this.Single.DataContext = client;
                this.Single.Visibility = Visibility.Visible;
                this.ClientView.Visibility = Visibility.Hidden;
                client.RInfo.EHost.IsReadOnly = client.RInfo.IsViewOnly;
            }
            else
            {
                this.Preview = false;
                this.Single.DataContext = null;
                this.Single.Visibility = Visibility.Hidden;
                this.ClientView.Visibility = Visibility.Visible;

                this.ClientView.ItemsSource = this.PInfos;
                if (client != null)
                {
                    client.IsCenter = false;
                    this.ClientView.SelectedItem = client;                    
                }
            }
        }
        #endregion

        private void Thumbnail_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var view = this.Thumbnail.SelectedItem as RemoteClient;
            if (view == null)
            {
                e.Handled = true;
                return;
            }

            ListBoxItem lvi = this.Thumbnail.ItemContainerGenerator.ContainerFromItem(view) as ListBoxItem;//获取ListViewItem
            if (lvi == null)
            {
                e.Handled = true;
                return;
            }

            var p = Mouse.GetPosition(lvi);
            Rect bounds = VisualTreeHelper.GetDescendantBounds(lvi);
            if (!bounds.Contains(p))
            {
                e.Handled = true;
                return;
            }
        }

        //private void Thumbnail_SizeChanged(object sender, SizeChangedEventArgs e)
        //{
        //    if (this.IsLoaded 
        //        && this.WindowState != WindowState.Minimized
        //        && this.PInfos != null)
        //        this.ResizeWindow(this.PInfos.Count);
        //}

        #region MenuItem的Event和ClickEvent
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (item == null)
                return;
            
            switch((string)item.Tag)
            {
                case "新建远程连接":
                {
                    var sub = e.OriginalSource as MenuItem;
                    Protocol p = (Protocol)sub.DataContext;

                    var client = this.GetClientUI(p, null);
                    client.ShowDialog();
                    break;
                }
                case "打开新的标签":
                {
                    RemoteClient client = new RemoteClient();
                    EmptyInfo ef = new EmptyInfo(this.RInfos);
                    client.RInfo = ef;
                    this.OpenClient(client, this.Resources["box2"]);
                    break;
                }
                case "全屏":
                {
                    this.EnterAppFullScreen();
                    break;
                }
                case "创建桌面快捷方式":
                {
                    //using IWshRuntimeLibrary;                    
                    var file = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                    var dinfo = System.IO.Path.GetDirectoryName(file);
                    var dir = System.IO.Path.GetFileName(dinfo);

                    Utils.CrtShortCut(dinfo, file, System.IO.Path.GetFileNameWithoutExtension(file), this.Description);
                    break;
                }
                case "关于RemoteApp":
                {
                    AboutBox about = new AboutBox();
                    about.ShowDialog();
                    break;
                }
                case "保存":
                {
                    this.SaveConfig();
                    break;
                }
                case "切换模式":
                {
                    this.SwitchMode();
                    break;
                }
                case "锁定":
                {
                    this.LockApp();
                    break;
                }
                case "设置保护":
                {
                    SetCrypto crypto = new SetCrypto()
                    {
                        Owner = this,
                        ShowInTaskbar = false,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };

                    crypto.CryptoCompletedEvent += Crypto_CryptoCompletedEvent;
                    crypto.ShowDialog();
                    break;
                }
                case "检查更新":
                {
                    this.CheckUpdate();
                    break;
                }
                case "参数配置":
                {
                    RConfig config = new RConfig()
                    {
                        Owner = this,
                        ShowInTaskbar = false,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };

                    config.AdjustResizeEvent += Config_AdjustResizeEvent;
                    config.ShowDialog();                    
                    break;
                }                
                case "退出":
                {
                    this.IsRealClose = true;
                    this.Close();
                    break;
                }
            }            
        }

        //更新密码保护，所有电脑的信息重新加密
        private void Crypto_CryptoCompletedEvent()
        {            
            foreach (var r in this.RInfos)
            {
                if (string.IsNullOrWhiteSpace(r.RInfo.Password))
                    continue;

                var p = Utils.DecryptChaCha20(r.RInfo.Password, Properties.Settings.Default.Nonce, RemoteInfo.SHA256);
                if (p == null)
                    r.RInfo.Password = string.Empty;
                else
                    r.RInfo.Password = Utils.EncryptChaCha20(p, Properties.Settings.Default.Nonce, Properties.Settings.Default.SHA256);
            }
            //更新密码保护
            RemoteInfo.SHA256 = Properties.Settings.Default.SHA256;
            this.SaveConfig();
        }

        private void Client_EditCompletedEvent(RemoteClient remote, EditMode mode)
        {
            if(mode == EditMode.新建)
            {
                this.RInfos.Add(remote);
                this.OpenClient(remote);
            }
        }
        #endregion

        #region Adjust size
        private void Config_AdjustResizeEvent(dynamic obj)
        {
            if (Properties.Settings.Default.IsHidden)
                this.LeftSplitter.Visibility = Visibility.Collapsed;
            else
                this.LeftSplitter.Visibility = Visibility.Visible;

            foreach (var v in PInfos)
            {
                if (v != this.Selector.SelectedItem)
                    v.RInfo.EHost.IsReadOnly = Properties.Settings.Default.SingleEnabled;
            }
                
            this.AlignCenter(this.PInfos.FirstOrDefault(m => m.IsCenter));            
            this.Selector.Focus();
            if (obj.Max)
                this.WindowState = WindowState.Maximized;
            else
                this.ResizeWindow(this.PInfos.Count);
        }

        public void ResizeWindow(int count)
        {
            if (this.ClientView.ActualWidth <= 150)
                return;

            var size = RemoteClient.DefaultSize;
            if (Properties.Settings.Default.SmartSize)
                size = AdjustSize(this.WindowState, count, new Size(this.ClientView.ActualWidth - 25, this.ClientView.ActualHeight));

            foreach (var v in this.PInfos)
            {
                v.Width = size.Width;
                v.Height = size.Height;
            }
        }

        public Size AdjustSize(WindowState state, int count, Size s)
        {
            double factor = Properties.Settings.Default.Factor;
            if(factor <= 0 || Properties.Settings.Default.UseScreenFactor)
                factor = SystemParameters.PrimaryScreenWidth / SystemParameters.PrimaryScreenHeight;

            switch (state)
            {
                case WindowState.Minimized:
                case WindowState.Normal:
                {
                    if (count > Properties.Settings.Default.NormalCount)
                        count = Properties.Settings.Default.NormalCount;                    
                    break;                   
                }
                case WindowState.Maximized:
                {
                    if (count > Properties.Settings.Default.MaxCount)
                        count = Properties.Settings.Default.MaxCount;
                    break;
                }
            }

            s = new Size(s.Width / count, s.Height / count);
            //不用考虑高度
            return new Size(s.Width, s.Width / factor + 16);
        }
        #endregion

        #region Client Opearator
        private void ListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem item = sender as ListBoxItem;
            if (item == null)
                return;

            //item.PreviewMouseLeftButtonDown += Item_PreviewMouseLeftButtonDown;
            RemoteClient client = item.DataContext as RemoteClient;
            if (client == null)
                return;

            int pos = -1;
            //TODO:
            var parent = Utils.GetVisualParent<ListBox>(item);
            if (parent?.DataContext != null)
            {
                RemoteClient empty = parent.DataContext as RemoteClient;
                if (empty != null && empty.RInfo is EmptyInfo)
                {
                    pos = this.PInfos.IndexOf(empty);
                    this.PInfos.Remove(empty);
                    client.RInfo.EHost.RZIndex = empty.RInfo.EHost.RZIndex;
                }
            }

            this.OpenClient(client, pos);
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem item = sender as ListBoxItem;
            if (item == null)
                return;

            RemoteClient client = item.DataContext as RemoteClient;
            if (client == null)
                return;

            int pos = -1;
            //TODO:
            var parent = Utils.GetVisualParent<ListBox>(item);
            if (parent?.DataContext != null)
            {
                RemoteClient empty = parent.DataContext as RemoteClient;
                if (empty != null)
                {
                    pos = this.PInfos.IndexOf(empty);
                    this.PInfos.Remove(empty);
                    client.RInfo.EHost.RZIndex = empty.RInfo.EHost.RZIndex;
                }
            }

            this.OpenClient(client, pos);
        }

        public void OpenClient(RemoteClient client)
        {
            this.OpenClient(client, -1, null);
        }

        public void OpenClient(RemoteClient client, int pos)
        {
            this.OpenClient(client, pos, null);
        }

        public void OpenClient(RemoteClient client, object args)
        {
            this.OpenClient(client, -1, args);
        }

        public void OpenClient(RemoteClient client, int pos = -1, object args = null)
        {            
            if (client == null)
                return;

            if (this.Preview)
                return;

            try
            {
                var f = this.PInfos.FirstOrDefault(m => m == client);
                if (f == null)
                {
                    int zindex = client.RInfo.EHost.RZIndex;
                    client.RInfo.EHost.GenerateHost(args);
                    if (pos >= 0)
                    {
                        this.PInfos.Insert(pos, client);
                        client.RInfo.EHost.RZIndex = zindex;
                    }
                    else
                        this.PInfos.Add(client);

                    f = client;
                    this.ResizeWindow(this.PInfos.Count);
                    client.RInfo.EHost.Open();
                }

                this.Selector.SelectedItem = f;
                if (this.Selector is ListBox)
                    ((ListBox)this.Selector).ScrollIntoView(f);
            }catch(Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        public void CloseClient(RemoteClient client)
        {            
            if (client == null)
                return;

            if (client.IsCenter)
                this.AlignCenter(client);

            Task.Run(() => { client.Close(); });            
            this.PInfos.Remove(client);                                 
            this.ResizeWindow(this.PInfos.Count);
        }

        public void ReopenClient(RemoteClient client)
        {
            if (client == null)
                return;

            this.CloseClient(client);
            this.OpenClient(client);
        }
        #endregion        

        private void ClientView_MouseMove(object sender, MouseEventArgs e)
        {
            ListBox box = sender as ListBox;
            if (box == null)
            {
                e.Handled = false;
                return;
            }

            if (box.Visibility != Visibility.Visible)
            {
                e.Handled = false;
                return;
            }

            var c = box.SelectedItem as RemoteClient;
            if (c == null)
            {
                e.Handled = false;
                return;
            }

            var item = box.ItemContainerGenerator.ContainerFromItem(c) as ListBoxItem;
            if (item == null)
            {
                e.Handled = false;
                return;
            }
            
            var p = e.GetPosition(item);
            var bound = VisualTreeHelper.GetDescendantBounds(item);
            if (!bound.Contains(p) && !this.ClientView.IsFocused)
                box.Focus();
            else
                this.Focus();

            e.Handled = false;
        }

        private void ClientView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.ClientView.Visibility != Visibility.Visible)
            {
                e.Handled = false;
                return;
            }

            foreach (var c in this.PInfos)
            {
                var item = this.ClientView.ItemContainerGenerator.ContainerFromItem(c) as ListBoxItem;
                if (item == null)
                    continue;

                var bound = VisualTreeHelper.GetDescendantBounds(item);
                var p = e.GetPosition(item);
                if (bound.Contains(p))
                {
                    this.ClientView.SelectedItem = c;
                    c.RInfo.EHost.IsReadOnly = c.RInfo.IsViewOnly;
                    break;
                }
            }

            e.Handled = false;
        }

        #region 拖拽事件控制
        private AdornerLayer PanelLayer = null;
        private void ListBoxItem_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            if (this.PanelLayer != null)
                PanelLayer.Update();
        }

        private void ListBoxItem_MouseEnter(object sender, MouseEventArgs e)
        {
            //MessageBox.Show("Enter");
            this.Cursor = Cursors.Hand;
            //Mouse.SetCursor(Cursors.Hand);
            //Mouse.UpdateCursor();
        }

        private void ListBoxItem_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
        }

        private void ListBoxItem_MouseMove(object sender, MouseEventArgs e)
        {
            ListBoxItem item = sender as ListBoxItem;
            if (item == null)
                return;

            if (e.RightButton == MouseButtonState.Pressed 
                && Keyboard.IsKeyDown(Key.LeftCtrl))
            {                
                DragAdorner adorner = null;
                this.PanelLayer = AdornerLayer.GetAdornerLayer(MainPanel);
                if (this.PanelLayer != null)
                {
                    adorner = new DragAdorner(item);
                    this.PanelLayer.Add(adorner);
                }

                Mouse.SetCursor(Cursors.Arrow);
                DragDrop.DoDragDrop(item, item.DataContext, DragDropEffects.Move);
                if (this.PanelLayer != null && adorner != null)
                    this.PanelLayer.Remove(adorner);
            }
        }

        private void ListBoxItem_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {            
            Point pos = new Point(System.Windows.Forms.Control.MousePosition.X, System.Windows.Forms.Control.MousePosition.Y);
            pos = this.Thumbnail.PointFromScreen(pos);
            if (pos.X >= 0 && pos.Y >= 0 &&pos.X <= this.Thumbnail.ActualWidth && pos.Y <= this.Thumbnail.ActualHeight)
                Mouse.SetCursor(Cursors.Arrow);
            else
                Mouse.SetCursor(Cursors.No);

            e.UseDefaultCursors = false;
            e.Handled = true;
        }

        private void Thumbnail_Drop(object sender, DragEventArgs e)
        {
            var client = e.Data.GetData(typeof(RemoteClient)) as RemoteClient;
            if (client == null)
                return;

            RemoteClient selected = null;
            foreach (var c in this.RInfos)
            {
                var item = this.Thumbnail.ItemContainerGenerator.ContainerFromItem(c) as ListBoxItem;
                if (item == null)
                    continue;

                var bound = VisualTreeHelper.GetDescendantBounds(item);
                var p = e.GetPosition(item);
                if (bound.Contains(p))
                {
                    selected = c;
                    break;
                }
            }

            if (selected != null)
            {
                var index = this.RInfos.IndexOf(selected);
                if(index >= 0)
                {
                    this.RInfos.Remove(client);
                    this.RInfos.Insert(index, client);
                    this.Thumbnail.SelectedItem = client;
                }
            }
        }

        private void Thumbnail_DragLeave(object sender, DragEventArgs e)
        {
            e.Handled = true;
            e.Effects = DragDropEffects.None;
        }
        #endregion

        private void Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                var client = e.AddedItems[0] as RemoteClient;
                client.RInfo.EHost.IsReadOnly = client.RInfo.IsViewOnly;
            }

            if (e.RemovedItems != null && e.RemovedItems.Count > 0)
            {
                var client = e.RemovedItems[0] as RemoteClient;
                if (client != null && Properties.Settings.Default.SingleEnabled)
                    client.RInfo.EHost.IsReadOnly = true;
            }
        }

        public Window GetClientUI(Protocol protocol, RemoteClient client = null)
        {
            Window win = null;
            switch (protocol)
            {
                case Protocol.RDP:
                {
                    var add = new EditRDPClient(client);
                    if (client == null)
                        add.EditCompletedEvent += Client_EditCompletedEvent;

                    win = add;
                    break;
                }
                case Protocol.SSH:
                {
                    var add = new EditSSHClient(client);
                    if (client == null)
                        add.EditCompletedEvent += Client_EditCompletedEvent;

                    win = add;
                    break;
                }
                case Protocol.TELNET:
                {
                    var add = new EditTelnetClient(client);
                    if (client == null)
                        add.EditCompletedEvent += Client_EditCompletedEvent;

                    win = add;
                    break;
                }
                case Protocol.VNC:
                {
                    var add = new EditVNCClient(client);
                    if (client == null)
                        add.EditCompletedEvent += Client_EditCompletedEvent;

                    win = add;
                    break;
                }
                default:
                return null;
            }

            win.ShowInTaskbar = false;
            win.Owner = this;
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            return win;
        }
    }

    //拖拽模版
    public class DragAdorner : Adorner
    {
        private UIElement Template { get; set; }
        private VisualBrush TVB { get; set; }

        public DragAdorner(UIElement element)
            : base(element)
        {
            this.Template = element;
            this.TVB = new VisualBrush(Template);

            this.Height = 50;
            this.Width = 50;
            this.IsHitTestVisible = false;
            this.SnapsToDevicePixels = true;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (Template != null)
            {
                Point pos = new Point(System.Windows.Forms.Control.MousePosition.X, System.Windows.Forms.Control.MousePosition.Y);
                Logger.WriteLine(pos.ToString());
                pos = PointFromScreen(pos);
                Rect rect = new Rect(pos.X - 25, pos.Y - 25, this.Width, this.Height);
                drawingContext.DrawRectangle(TVB, new Pen(Brushes.Transparent, 0), rect);
            }
        }
    }
}
