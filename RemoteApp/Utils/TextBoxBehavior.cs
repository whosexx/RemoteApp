using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RemoteApp
{
    public class TextBoxBehavior
    {
        public static bool GetSelectedAllOnGotFocus(DependencyObject obj)
        {
            return (bool)obj.GetValue(SelectedAllOnGotFocusProperty);
        }

        public static void SetSelectedAllOnGotFocus(DependencyObject obj, bool value)
        {
            obj.SetValue(SelectedAllOnGotFocusProperty, value);
        }

        // Using a DependencyProperty as the backing store for SelectedAllOnGotFocus.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedAllOnGotFocusProperty =
            DependencyProperty.RegisterAttached("SelectedAllOnGotFocus", typeof(bool), typeof(TextBoxBehavior), new UIPropertyMetadata(new PropertyChangedCallback(OnSelectedAllOnGotFocusPropertyChanged)));

        private static void OnSelectedAllOnGotFocusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is TextBox)
            {
                TextBox box = d as TextBox;
                if ((bool)e.NewValue)
                {
                    box.AddHandler(TextBox.PreviewMouseDownEvent, new MouseButtonEventHandler(TextBox_PreviewMouseDown));
                    box.AddHandler(TextBox.GotFocusEvent, new RoutedEventHandler(TextBox_GotFocus));
                    box.AddHandler(TextBox.LostFocusEvent, new RoutedEventHandler(TextBox_LostFocus));
                }
                else
                {
                    box.RemoveHandler(TextBox.MouseUpEvent, new MouseButtonEventHandler(TextBox_PreviewMouseDown));
                    box.RemoveHandler(TextBox.GotFocusEvent, new RoutedEventHandler(TextBox_GotFocus));
                    box.RemoveHandler(TextBox.LostFocusEvent, new RoutedEventHandler(TextBox_LostFocus));
                }
            }
            else if (d is PasswordBox)
            {
                PasswordBox box = d as PasswordBox;
                if ((bool)e.NewValue)
                {
                    box.AddHandler(PasswordBox.PreviewMouseDownEvent, new MouseButtonEventHandler(TextBox_PreviewMouseDown));
                    box.AddHandler(PasswordBox.GotFocusEvent, new RoutedEventHandler(TextBox_GotFocus));
                    box.AddHandler(PasswordBox.LostFocusEvent, new RoutedEventHandler(TextBox_LostFocus));
                }
                else
                {
                    box.RemoveHandler(PasswordBox.MouseUpEvent, new MouseButtonEventHandler(TextBox_PreviewMouseDown));
                    box.RemoveHandler(PasswordBox.GotFocusEvent, new RoutedEventHandler(TextBox_GotFocus));
                    box.RemoveHandler(PasswordBox.LostFocusEvent, new RoutedEventHandler(TextBox_LostFocus));
                }
            }
        }

        private static void TextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBox box = sender as TextBox;
            if (box != null)
            {
                if (!box.IsKeyboardFocusWithin)
                {
                    box.Focus();
                    e.Handled = true;
                }
                return;
            }

            PasswordBox pass = sender as PasswordBox;
            if(pass != null)
            {
                if (!pass.IsKeyboardFocusWithin)
                {
                    pass.Focus();
                    e.Handled = true;
                }
                return;
            }            
        }

        private static void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = sender as TextBox;
            if (box != null)
            {
                box.PreviewMouseDown -= TextBox_PreviewMouseDown;
                box.SelectAll();
                return;
            }

            PasswordBox pass = sender as PasswordBox;
            if (pass != null)
            {
                pass.PreviewMouseDown -= TextBox_PreviewMouseDown;
                pass.SelectAll();
            }
        }

        private static void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = sender as TextBox;
            if (box != null)
            {
                box.PreviewMouseDown += TextBox_PreviewMouseDown;
                return;
            }

            PasswordBox pass = sender as PasswordBox;
            if (pass != null)
            {
                pass.PreviewMouseDown += TextBox_PreviewMouseDown;
                return;
            }
        }
    }
}
