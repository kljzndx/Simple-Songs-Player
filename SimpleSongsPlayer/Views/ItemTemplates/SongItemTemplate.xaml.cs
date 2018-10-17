using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SimpleSongsPlayer.DataModel;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SimpleSongsPlayer.Views.ItemTemplates
{
    public sealed partial class SongItemTemplate : UserControl
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            nameof(Source), typeof(Song), typeof(SongItemTemplate), new PropertyMetadata(null));

        public static readonly DependencyProperty MenuProperty = DependencyProperty.Register(
            nameof(Menu), typeof(MenuFlyout), typeof(SongItemTemplate), new PropertyMetadata(null));

        public SongItemTemplate()
        {
            this.InitializeComponent();
            ControllerButtons_StackPanel.Visibility = Visibility.Collapsed;
        }
        
        public Song Source
        {
            get => (Song) GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public MenuFlyout Menu
        {
            get => (MenuFlyout) GetValue(MenuProperty);
            set => SetValue(MenuProperty, value);
        }
        
        public event TypedEventHandler<SongItemTemplate, EventArgs> PlayRequested;
        public event TypedEventHandler<SongItemTemplate, EventArgs> MenuOpening;

        protected override void OnRightTapped(RightTappedRoutedEventArgs e)
        {
            base.OnRightTapped(e);

            if (Menu != null)
            {
                Point position = e.GetPosition(this);
                Menu?.ShowAt(this, position);
                MenuOpening?.Invoke(this, EventArgs.Empty);
            }
        }

        private void Play_Button_Click(object sender, RoutedEventArgs e)
        {
            PlayRequested?.Invoke(this, EventArgs.Empty);
        }
        
        private void More_Button_OnClick(object sender, RoutedEventArgs e)
        {
            if (Menu != null)
                MenuOpening?.Invoke(this, EventArgs.Empty);
        }
    }
}
