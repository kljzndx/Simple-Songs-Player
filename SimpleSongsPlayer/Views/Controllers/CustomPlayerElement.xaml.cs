using CommunityToolkit.Mvvm.DependencyInjection;

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
        private bool _isPressSlider;

        public CustomPlayerElement()
        {
            this.InitializeComponent();
            _mediaPlayer = Ioc.Default.GetRequiredService<MediaPlayer>();
            _mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            _mediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
            _mediaPlayer.PlaybackSession.PositionChanged += PlaybackSession_PositionChanged;

            //Position_Slider.AddHandler(PointerPressedEvent, new PointerEventHandler((s, e) => _isPressSlider = true), true);
            //Position_Slider.AddHandler(PointerReleasedEvent, new PointerEventHandler((s, e) => _isPressSlider = false), true);

            Position_Slider.GotFocus += (s, e) => _isPressSlider = true;
            Position_Slider.LostFocus += (s, e) => _isPressSlider = false;
        }

        private async void PlaybackSession_PositionChanged(MediaPlaybackSession sender, object args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (!_isPressSlider)
                    Position_Slider.Value = sender.Position.TotalMinutes;
            });
        }

        private async void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Position_Slider.Maximum = sender.PlaybackSession.NaturalDuration.TotalMinutes;
                sender.Play();
            });
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

        private void Previous_Button_Click(object sender, RoutedEventArgs e)
        {
            ((MediaPlaybackList)_mediaPlayer.Source).MovePrevious();
        }

        private void Next_Button_Click(object sender, RoutedEventArgs e)
        {
            ((MediaPlaybackList)_mediaPlayer.Source).MoveNext();
        }

        private void Position_Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            _mediaPlayer.PlaybackSession.Position = TimeSpan.FromMinutes(Position_Slider.Value);
        }
    }
}
