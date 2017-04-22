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
using Xceed.Wpf.Toolkit;

namespace RemoteApp.UI
{
    /// <summary>
    /// DConfig.xaml 的交互逻辑
    /// </summary>
    public partial class RConfig : Window
    {
        public event Action<object> AdjustResizeEvent;
        public RConfig()
        {
            InitializeComponent();
        }        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Mode.ItemsSource = Enum.GetNames(typeof(Mode));
            this.DataContext = Properties.Settings.Default;            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var exp = this.SmartSize.GetBindingExpression(CheckBox.IsCheckedProperty);
            if (exp != null)
                exp.UpdateSource();

            exp = this.NormalCount.GetBindingExpression(IntegerUpDown.ValueProperty);
            if (exp != null)
                exp.UpdateSource();

            exp = this.MaxCount.GetBindingExpression(IntegerUpDown.ValueProperty);
            if (exp != null)
                exp.UpdateSource();

            exp = this.Factor.GetBindingExpression(DoubleUpDown.ValueProperty);
            if (exp != null)
                exp.UpdateSource();

            exp = this.Screen.GetBindingExpression(CheckBox.IsCheckedProperty);
            if (exp != null)
                exp.UpdateSource();

            exp = this.Hidden.GetBindingExpression(CheckBox.IsCheckedProperty);
            if (exp != null)
                exp.UpdateSource();

            exp = this.MinClosed.GetBindingExpression(CheckBox.IsCheckedProperty);
            if (exp != null)
                exp.UpdateSource();

            exp = this.Single.GetBindingExpression(CheckBox.IsCheckedProperty);
            if (exp != null)
                exp.UpdateSource();

            exp = this.Update.GetBindingExpression(CheckBox.IsCheckedProperty);
            if (exp != null)
                exp.UpdateSource();

            bool max = false;
            if (Properties.Settings.Default.MaxWhenOpened != this.MaxOpen.IsChecked)
            {
                if (!Properties.Settings.Default.MaxWhenOpened)
                    max = true;

                exp = this.MaxOpen.GetBindingExpression(CheckBox.IsCheckedProperty);
                if (exp != null)
                    exp.UpdateSource();
            }

            if(Properties.Settings.Default.Mode != this.Mode.Text)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(Properties.Settings.Default.Mode + ", " + this.Mode.Text);
                exp = this.Mode.GetBindingExpression(ComboBox.SelectedItemProperty);
                if (exp != null)
                    exp.UpdateSource();
            }

            this.AdjustResizeEvent?.Invoke(new {Max = max});
            Properties.Settings.Default.Save();
            this.Close();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            // 获取鼠标相对标题栏位置  
            this.DragMove();
        }
    }
}
