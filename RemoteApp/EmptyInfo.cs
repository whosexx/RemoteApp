using LibRDP;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml.Serialization;

namespace RemoteApp
{
    public class EmptyInfo:RemoteInfo
    {
        public EmptyInfo(ObservableCollection<RemoteClient> list) : base(Protocol.Empty)
        {
            this.IsViewOnly = false;
            this.Alias = "打开新的标签";
            this.EHost = new EmptyHost(list);
        }

        public EmptyInfo() : base(Protocol.Empty)
        {
            this.IsViewOnly = false;
            this.Alias = "打开新的标签";
            this.EHost = new EmptyHost();
        }
    }

    public class EmptyHost: ElementHost
    {
        public ObservableCollection<RemoteClient> RInfos { get; set; }

        private const string Xaml1 = @"<DataTemplate 
                        xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                        xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
                        <Grid Height = ""100"" VerticalAlignment=""Center"" HorizontalAlignment=""Center"" Margin=""10"">
                            <Grid.RowDefinitions>
                                <RowDefinition Height=""auto"" />
                                <RowDefinition Height=""*"" />
                            </Grid.RowDefinitions >
                            <Image Source=""{Binding RDPBitmap}"" Height=""80"" Width=""100"" Margin=""0,2,0,0"" />
                            <TextBlock Text=""{Binding RInfo.Protocol}"" Margin=""15,7,0,0"" Foreground=""White"" FontWeight=""Bold"" FontStyle=""Italic"" />
                            <TextBlock Text=""{Binding RInfo.Alias}"" VerticalAlignment=""Top"" FontFamily=""微软雅黑""
                                       MaxWidth=""92"" TextWrapping=""Wrap"" Grid.Row=""1"" FontSize=""11""
                                       HorizontalAlignment=""Center"" Margin=""2,0,2,0"" />
                        </Grid>
                    </DataTemplate>";

        private const string Xaml2 = @"<ItemsPanelTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
                        <UniformGrid Columns=""8"" VerticalAlignment=""Center"" HorizontalAlignment=""Center"" />
                    </ItemsPanelTemplate>";

        public EmptyHost(ObservableCollection<RemoteClient> list = null) : base(null)
        {
            this.RInfos = list;            
        }

        public override void GenerateHost(object obj = null)
        {
            this.RZIndex = System.Threading.Interlocked.Decrement(ref _Index);
            var obj1 = XamlReader.Parse(Xaml1);
            var obj2 = XamlReader.Parse(Xaml2);

            ListBox box = new ListBox()
            {
                //box.AllowDrop = true;
                BorderThickness = new Thickness(0),
                BorderBrush = Brushes.Transparent,
                Padding = new Thickness(20),
                Margin = new Thickness(0),

                ItemContainerStyle = obj as Style,
                ItemTemplate = obj1 as DataTemplate,
                ItemsPanel = obj2 as ItemsPanelTemplate,
                SelectionMode = SelectionMode.Single
            };
            box.SelectionChanged += Box_SelectionChanged;
            box.ItemsSource = this.RInfos;

            this.RObject = null;
            this.Host = box;            
        }

        private void Box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Logger.WriteLine("Box_SelectionChanged");
            e.Handled = true;
        }

        public override void DisposeHost()
        {
            
        }
    }
}
