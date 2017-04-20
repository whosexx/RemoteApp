using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using LibRDP;
using System.Windows.Media.Imaging;

namespace RemoteApp
{   
    public class Client2ToolTip : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            RemoteClient element = value as RemoteClient;
            if (element == null || element.RInfo is EmptyInfo)
                return null;

            string ret = "别名：" + element.RInfo.Alias + "\r\n地址：" + element.RInfo.Ip + "\r\n协议：" + element.RInfo.Protocol;
            if (string.IsNullOrWhiteSpace(element.RInfo.Memo))
                return ret;

            const int mode = 13;
            int yu = element.RInfo.Memo.Length % mode;
            int count = element.RInfo.Memo.Length / mode;
            if (yu > 0)
                count++;

            string memo = string.Empty;
            for(int i = 0; i< count; i++)
            {
                int len = element.RInfo.Memo.Length - i * mode;
                memo += element.RInfo.Memo.Substring(i * mode, len > mode ? mode : len) + "\r\n          ";
            }
            memo = memo.Trim();
            memo = memo.TrimEnd('\n');
            memo = memo.TrimEnd('\r');

            ret += "\r\n备注：" + memo;            
            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class Int2Visibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;

            int x = (int)value;
            if (x <= 0)
                return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LengthConverter : IValueConverter
    {
        public double Standard { get; set; } = 40.0;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return 0.0;

            double val = (double)value;
            Logger.WriteLine("Length:" + val);
            return val - Standard;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ThicknessConverter : IValueConverter
    {
        public Thickness Default { get; set; } = new Thickness(0);
        public Thickness Setting { get; set; } = new Thickness(0, 0, 0, 1);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Default;

            int count = (int)value;
            if (count <= 0)
                return Default;

            return Setting;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BitmapVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            RemoteClient client = value as RemoteClient;
            if (client == null)
                return Visibility.Collapsed;

            if (!(client.RInfo is EmptyInfo))
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BitmapSourceWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            RemoteClient rc = value as RemoteClient;
            if (rc.RInfo is EmptyInfo)
                return 0.0;

            return 16.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class WidthConverter : IMultiValueConverter
    {
        public double Default { get; set; } = 154;
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int count = (int)values[0];
            double width = (double)values[1] - 50;

            if (count <= 0)
                return Default;

            width /= count;
            return width < Default ? width : Default;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double d = (double)value;
            return d - 4;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class Visibility2BooleanConverter : IValueConverter
    {
        public bool Reverse { get; set; } = false;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;

            if (!Reverse)
            {
                Visibility vis = (Visibility)value;
                if (vis == Visibility.Visible)
                    return true;

                return false;
            }
            else
            {
                Visibility vis = (Visibility)value;
                if (vis != Visibility.Visible)
                    return true;

                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class VisibilityConverter : IValueConverter
    {
        public bool Reverse { get; set; }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool b = (bool)value;
            if (this.Reverse)
            {
                if (b)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }
            else
            {
                if (b)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return Brushes.Gray;

            ConnectedStatus statu = (ConnectedStatus)value;
            switch (statu)
            {
                case ConnectedStatus.正在连接:
                case ConnectedStatus.断开连接:
                return Brushes.Red;
                case ConnectedStatus.正常:
                return Brushes.Green;
                default:
                return Brushes.Gray;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;

            bool b = (bool)value;
            return !b;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
