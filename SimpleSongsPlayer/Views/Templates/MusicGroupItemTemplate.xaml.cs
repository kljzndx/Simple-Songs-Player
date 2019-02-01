using System;
using System.Collections.Generic;
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
using SimpleSongsPlayer.Log;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.ViewModels.SettingProperties;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SimpleSongsPlayer.Views.Templates
{
    public sealed partial class MusicGroupItemTemplate : UserControl
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            nameof(Source), typeof(MusicFileGroup), typeof(MusicGroupItemTemplate), new PropertyMetadata(null));

        private MusicGroupViewSettingProperties settings = MusicGroupViewSettingProperties.Current;

        public MusicGroupItemTemplate()
        {
            this.InitializeComponent();
            DataContextChanged += MusicGroupItemTemplate_DataContextChanged;
        }

        public MusicFileGroup Source
        {
            get => (MusicFileGroup) GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public List<MusicItemMenuItem<MusicFileGroup>> MoreMenuItemList { get; set; }

        public event RoutedEventHandler PlayRequested;

        private void SetUpMenu()
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

        private async void MusicGroupItemTemplate_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (Source != null)
                Cover_Image.Source = await Source.GetCover();
        }

        private void MusicGroupItemTemplate_OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            SetUpMenu();
            More_MenuFlyout.ShowAt(this, e.GetPosition(this));
        }

        private void MoreMenu_Button_OnClick(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            SetUpMenu();
            More_MenuFlyout.ShowAt(MoreMenu_Button);
        }

        private void Play_Button_OnClick(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            PlayRequested?.Invoke(sender, e);
        }
    }
}
