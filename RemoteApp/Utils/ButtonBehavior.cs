using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace RemoteApp
{
    public class ButtonBehavior
    {
        public static bool GetIsUseButtonAnimation(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsUseButtonAnimationProperty);
        }

        public static void SetIsUseButtonAnimation(DependencyObject obj, bool value)
        {
            obj.SetValue(IsUseButtonAnimationProperty, value);
        }

        public static readonly DependencyProperty IsUseButtonAnimationProperty =
            DependencyProperty.RegisterAttached("IsUseButtonAnimation", typeof(bool), typeof(ButtonBehavior), new UIPropertyMetadata(new PropertyChangedCallback(OnIsUseButtonAnimationPropertyChanged)));

        private static void OnIsUseButtonAnimationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is Button)
            {
                Button btn = d as Button;
                if((bool)e.NewValue)
                {
                    btn.MouseEnter += Btn_MouseEnter;
                    btn.MouseLeave += Btn_MouseLeave;
                    btn.MouseDown += Btn_MouseDown;
                    btn.MouseUp += Btn_MouseUp;
                }
                else
                {
                    btn.MouseEnter -= Btn_MouseEnter;
                    btn.MouseLeave -= Btn_MouseLeave;
                    btn.MouseDown -= Btn_MouseDown;
                    btn.MouseUp -= Btn_MouseUp;
                }
            }
        }

        private static void Btn_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void Btn_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void Btn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null)
                return;

            if (btn.Tag != null)
            {
                var ad = btn.Tag as ButtonShadowAdorner;
                if (ad == null)
                    return;

                ad.Visibility = Visibility.Collapsed;
            }
        }

        private static void Btn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null)
                return;

            if (btn.Tag == null)
            {
                AdornerLayer layer = AdornerLayer.GetAdornerLayer(btn);
                if (layer != null)
                {
                    var ad = new ButtonShadowAdorner(btn);
                    btn.Tag = ad;
                    layer.Add(ad);
                    ad.Visibility = Visibility.Visible;
                }
            }
            else
            {
                ButtonShadowAdorner ad = btn.Tag as ButtonShadowAdorner;
                if (ad == null)
                    return;

                ad.Visibility = Visibility.Visible;
            }
        }
    }   

    public class ButtonShadowChrome : Control
    {
        static ButtonShadowChrome()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ButtonShadowChrome), new FrameworkPropertyMetadata(typeof(ButtonShadowChrome)));
        }
    }

    public class ButtonShadowAdorner : Adorner
    {
        private ButtonShadowChrome Chrome;
        private VisualCollection Visuals;

        static ButtonShadowAdorner()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ButtonShadowAdorner), new FrameworkPropertyMetadata(typeof(ButtonShadowAdorner)));
        }

        public ButtonShadowAdorner(UIElement element)
            : base(element)
        {
            this.Chrome = new ButtonShadowChrome();
            this.Chrome.DataContext = element;
            this.Chrome.IsHitTestVisible = false;

            this.Visuals = new VisualCollection(this);
            this.Visuals.Add(this.Chrome);
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            this.Chrome.Arrange(new Rect(arrangeBounds));
            return arrangeBounds;
        }

        protected override int VisualChildrenCount
        {
            get { return this.Visuals.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return this.Visuals[index];
        }
    }
}
