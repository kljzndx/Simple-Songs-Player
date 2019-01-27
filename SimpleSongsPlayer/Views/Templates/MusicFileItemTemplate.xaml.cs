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
using SimpleSongsPlayer.Log;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;
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
            get => (MusicFileDynamic)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty IsShowMoreButtonProperty = DependencyProperty.Register(
            nameof(IsShowMoreButton), typeof(bool), typeof(MusicFileItemTemplate), new PropertyMetadata(true));

        public bool IsShowMoreButton
        {
            get => (bool) GetValue(IsShowMoreButtonProperty);
            set => SetValue(IsShowMoreButtonProperty, value);
        }

        public List<MusicItemMenuItem<MusicFileDynamic>> MoreMenuItemList { get; set; }

        public event RoutedEventHandler PlayRequested;
        
        private void SetUpMenu()
        {
            if (!More_MenuFlyout.Items.Any() && MoreMenuItemList != null)
            {
                foreach (var item in MoreMenuItemList)
                {
                    var flyoutItem = new MenuFlyoutItem {Text = item.Name};
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

        private void Play_Button_OnClick(object sender, RoutedEventArgs e)
        {
            PlayRequested?.Invoke(this, e);
        }

        private void More_Button_OnClick(object sender, RoutedEventArgs e)
        {
            SetUpMenu();
        }

        private void MusicFileItemTemplate_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            PlayRequested?.Invoke(this, null);
        }

        private void MusicFileItemTemplate_OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            SetUpMenu();
            More_MenuFlyout.ShowAt(this, e.GetPosition(this));
        }
    }
}
