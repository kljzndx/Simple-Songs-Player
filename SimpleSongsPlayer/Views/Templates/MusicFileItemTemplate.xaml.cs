using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using GalaSoft.MvvmLight.Command;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Service;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SimpleSongsPlayer.Views.Templates
{
    public sealed partial class MusicFileItemTemplate : UserControl
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            nameof(Source), typeof(MusicFileDynamic), typeof(MusicFileItemTemplate), new PropertyMetadata(null));
        
        public MusicFileItemTemplate()
        {
            this.InitializeComponent();
        }

        public MusicFileDynamic Source
        {
            get => (MusicFileDynamic) GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public List<MusicListMenuItem> MoreMenuItemList { get; set; }

        public event RoutedEventHandler PlayButton_Click;

        private void Play_Button_OnClick(object sender, RoutedEventArgs e)
        {
            PlayButton_Click?.Invoke(this, e);
        }

        private void More_Button_OnClick(object sender, RoutedEventArgs e)
        {
            if (!More_MenuFlyout.Items.Any() && MoreMenuItemList != null)
            {
                foreach (var item in MoreMenuItemList)
                {
                    var flyoutItem = new MenuFlyoutItem { Text = item.Name };
                    flyoutItem.Click += async (s, a) =>
                    {
                        var theItem = s as MenuFlyoutItem;
                        if (theItem is null)
                            return;

                        this.LogByObject($"点击 {theItem.Text}");
                        await item.Action(Source);
                    };

                    More_MenuFlyout.Items.Add(flyoutItem);
                }
            }
        }
    }
}
