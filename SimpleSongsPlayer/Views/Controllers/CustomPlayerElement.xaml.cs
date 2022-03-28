﻿using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;

using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SimpleSongsPlayer.Views.Controllers
{
    public sealed partial class CustomPlayerElement : UserControl
    {
        private readonly MediaPlayer _mediaPlayer;
        private readonly ConfigurationService _configService;
        private bool _isPressSlider;

        public event RoutedEventHandler CoverButtonClick;

        public CustomPlayerElement()
        {
            this.InitializeComponent();
            _configService = Ioc.Default.GetRequiredService<ConfigurationService>();
            _configService.PropertyChanged += ConfigService_PropertyChanged;

            WeakReferenceMessenger.Default.Register<CustomPlayerElement, string, string>(this, nameof(PlaybackListManageService), async (ctor, mes) =>
            {
                if (mes == "CurrentPlayChanged")
                {
                    var manageService = Ioc.Default.GetRequiredService<MusicFileManageService>();
                    Cover_Image.Source = await manageService.GetCurrentPlayItem()?.GetCoverAsync();

                    ctor.Position_Slider.Maximum = _mediaPlayer.PlaybackSession.NaturalDuration.TotalMinutes;
                    ctor._mediaPlayer.PlaybackSession.PlaybackRate = _configService.PlaybackRate;

                    PlayList_ListView.ItemsSource = manageService.GetPlaybackList();

                    var item = Ioc.Default.GetRequiredService<PlaybackListManageService>().CurrentPlayItem;
                    if (item != null)
                        PlayList_ListView.SelectedIndex = item.TrackId;

                    if (ctor._mediaPlayer.PlaybackSession.PlaybackState != MediaPlaybackState.Playing)
                        ctor._mediaPlayer.Play();
                }
            });

            _mediaPlayer = Ioc.Default.GetRequiredService<MediaPlayer>();
            _mediaPlayer.Source = Ioc.Default.GetRequiredService<PlaybackListManageService>().GetPlaybackList();
            _mediaPlayer.Volume = _configService.Volume;
            _mediaPlayer.IsLoopingEnabled = _configService.LoopingMode == LoopingModeEnum.Single;

            _mediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
            _mediaPlayer.PlaybackSession.PositionChanged += PlaybackSession_PositionChanged;

            //Position_Slider.AddHandler(PointerPressedEvent, new PointerEventHandler((s, e) => _isPressSlider = true), true);
            //Position_Slider.AddHandler(PointerReleasedEvent, new PointerEventHandler((s, e) => _isPressSlider = false), true);

            Position_Slider.GotFocus += (s, e) => _isPressSlider = true;
            Position_Slider.LostFocus += (s, e) =>
            {
                WeakReferenceMessenger.Default.Send($"PositionChangedByUser:{Position_Slider.Value}", "MediaPlayer");
                _isPressSlider = false;
            };
        }

        private MediaPlaybackList GetPlayList()
        {
            return (MediaPlaybackList)_mediaPlayer.Source;
        }

        private void Cover_Button_Click(object sender, RoutedEventArgs e)
        {
            CoverButtonClick?.Invoke(this, e);
        }

        private void Previous_Button_Click(object sender, RoutedEventArgs e)
        {
            GetPlayList().MovePrevious();
        }

        private void Next_Button_Click(object sender, RoutedEventArgs e)
        {
            GetPlayList().MoveNext();
        }

        private void Position_Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_isPressSlider)
            {
                _mediaPlayer.PlaybackSession.Position = TimeSpan.FromMinutes(Position_Slider.Value);
            }
        }

        private void ConfigService_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_configService.Volume):
                    _mediaPlayer.Volume = _configService.Volume;
                    break;
                case nameof(_configService.LoopingMode):
                    _mediaPlayer.IsLoopingEnabled = _configService.LoopingMode == LoopingModeEnum.Single;
                    GetPlayList().ShuffleEnabled = _configService.LoopingMode == LoopingModeEnum.Random;
                    break;
                case nameof(_configService.PlaybackRate):
                    _mediaPlayer.PlaybackSession.PlaybackRate = _configService.PlaybackRate;
                    break;
            }
        }

        private async void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                switch (sender.PlaybackState)
                {
                    case MediaPlaybackState.Opening:
                    case MediaPlaybackState.Buffering:
                    case MediaPlaybackState.Playing:
                        Play_Button.Visibility = Visibility.Collapsed;
                        Pause_Button.Visibility = Visibility.Visible;
                        break;
                    case MediaPlaybackState.Paused:
                    case MediaPlaybackState.None:
                    default:
                        Play_Button.Visibility = Visibility.Visible;
                        Pause_Button.Visibility = Visibility.Collapsed;
                        break;
                }
            });
        }

        private async void PlaybackSession_PositionChanged(MediaPlaybackSession sender, object args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (!_isPressSlider)
                {
                    Position_Slider.Value = sender.Position.TotalMinutes;
                    WeakReferenceMessenger.Default.Send($"PositionChangedBySystem:{sender.Position.TotalMinutes}", "MediaPlayer");
                }
            });
        }

        private async void PlayList_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var newItem = e.AddedItems.FirstOrDefault() as MusicUi;
            if (newItem != null)
                await Ioc.Default.GetRequiredService<PlaybackListManageService>().PushGroup(PlayList_ListView.Items.Cast<MusicUi>(), newItem);
        }

        private async void PlayList_ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var newItem = e.ClickedItem as MusicUi;
            if (newItem != null)
                await Ioc.Default.GetRequiredService<PlaybackListManageService>().PushGroup(PlayList_ListView.Items.Cast<MusicUi>(), newItem);
        }
    }
}
