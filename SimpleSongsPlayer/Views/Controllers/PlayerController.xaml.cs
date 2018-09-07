﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using SimpleSongsPlayer.DataModel;
using SimpleSongsPlayer.ViewModels;
using SimpleSongsPlayer.ViewModels.Controllers;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SimpleSongsPlayer.Views.Controllers
{
    public sealed partial class PlayerController : UserControl
    {
        public static readonly DependencyProperty AllSongsProperty = DependencyProperty.Register(
            nameof(AllSongs), typeof(List<Song>), typeof(PlayerController), new PropertyMetadata(null));

        private bool? isPressPositionControlButton = false;
        private bool isPressPositionSlider;

        private readonly PlayerControllerViewModel vm;
        private readonly MediaPlayer player;
        private readonly SettingProperties settingProperties;

        public PlayerController()
        {
            this.InitializeComponent();
            vm = ((ViewModelLocator) Application.Current.Resources["Locator"]).PlayerController;
            player = App.Player;
            settingProperties = SettingProperties.Current;

            player.SourceChanged += Player_SourceChanged;
            player.PlaybackSession.PositionChanged += Player_PositionChanged;
            player.PlaybackSession.PlaybackStateChanged += Player_PlaybackStateChanged;

            settingProperties.PropertyChanged += SettingProperties_PropertyChanged;

            Rewind_Button.AddHandler(PointerPressedEvent, new PointerEventHandler(async (s, e) => await PressPositionButton(false)), true);
            Rewind_Button.AddHandler(PointerReleasedEvent, new PointerEventHandler((s, e) => ReleasePositionButton(false)), true);
            Rewind_Button.AddHandler(PointerCanceledEvent, new PointerEventHandler((s, e) => ReleasePositionButton(false)), true);
            Rewind_Button.AddHandler(PointerCaptureLostEvent, new PointerEventHandler((s, e) => ReleasePositionButton(false)), true);

            FastForward_Button.AddHandler(PointerPressedEvent, new PointerEventHandler(async (s, e) => await PressPositionButton(true)), true);
            FastForward_Button.AddHandler(PointerReleasedEvent, new PointerEventHandler((s, e) => ReleasePositionButton(true)), true);
            FastForward_Button.AddHandler(PointerCanceledEvent, new PointerEventHandler((s, e) => ReleasePositionButton(true)), true);
            FastForward_Button.AddHandler(PointerCaptureLostEvent, new PointerEventHandler((s, e) => ReleasePositionButton(true)), true);

            Position_Slider.AddHandler(PointerPressedEvent, new PointerEventHandler(Position_Slider_PointerPressed), true);
            Position_Slider.AddHandler(PointerReleasedEvent, new PointerEventHandler(Position_Slider_PointerReleased), true);
            Position_Slider.AddHandler(PointerCanceledEvent, new PointerEventHandler(Position_Slider_PointerReleased), true);
            Position_Slider.AddHandler(PointerCaptureLostEvent, new PointerEventHandler(Position_Slider_PointerReleased), true);
        }

        public List<Song> AllSongs
        {
            get => (List<Song>) GetValue(AllSongsProperty);
            set => SetValue(AllSongsProperty, value);
        }

        private void UpdateInfo(MediaPlaybackItem media)
        {
            vm.CurrentSong = AllSongs.Find(s => s.PlaybackItem == media);
            player.Volume = settingProperties.Volume;
            player.PlaybackSession.PlaybackRate = settingProperties.PlaybackSpeed;
        }

        private void Play()
        {
            if (player.PlaybackSession.PlaybackState != MediaPlaybackState.Playing)
                player.Play();
        }

        private void Pause()
        {
            if (player.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
                player.Pause();
        }

        private void SetPosition(TimeSpan newTime)
        {
            player.PlaybackSession.Position = newTime;
        }

        private async Task PressPositionButton(bool isAdd)
        {
            if (isPressPositionControlButton != false)
                return;

            isPressPositionControlButton = null;
            await Task.Delay(TimeSpan.FromSeconds(1));

            if (isPressPositionControlButton == null)
                isPressPositionControlButton = true;
            else return;

            Pause();

            while (isPressPositionControlButton == true)
            {
                if (isAdd)
                    SetPosition(player.PlaybackSession.Position + TimeSpan.FromSeconds(1));
                else
                    SetPosition(player.PlaybackSession.Position - TimeSpan.FromSeconds(1));

                await Task.Delay(TimeSpan.FromMilliseconds(200));
            }
        }

        private void ReleasePositionButton(bool isNextSong)
        {
            bool? b = isPressPositionControlButton;
            isPressPositionControlButton = false;

            if (b == null)
                if (isNextSong)
                    vm.PlayerSource.MoveNext();
                else
                    vm.PlayerSource.MovePrevious();
            else
                Play();
        }

        private void SettingProperties_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(settingProperties.Volume):
                    player.Volume = settingProperties.Volume;
                    break;
                case nameof(settingProperties.PlaybackSpeed):
                    player.PlaybackSession.PlaybackRate = settingProperties.PlaybackSpeed;
                    break;
            }
        }

        private void Player_SourceChanged(MediaPlayer sender, object args)
        {
            if (vm.PlayerSource != null)
                vm.PlayerSource.CurrentItemChanged -= PlayerSource_CurrentItemChanged;

            vm.PlayerSource = (MediaPlaybackList) sender.Source;
            vm.PlayerSource.CurrentItemChanged += PlayerSource_CurrentItemChanged;
        }

        private async void PlayerSource_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                UpdateInfo(args.NewItem);
                Position_Slider.Maximum = args.NewItem.Source.Duration.GetValueOrDefault(TimeSpan.Zero).TotalMinutes;
            });
        }

        private async void Player_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                switch (sender.PlaybackState)
                {
                    case MediaPlaybackState.Opening:
                    case MediaPlaybackState.Buffering:
                        break;
                    case MediaPlaybackState.Playing:
                        PlayOrPause_ToggleButton.IsChecked = true;
                        break;
                    case MediaPlaybackState.None:
                    case MediaPlaybackState.Paused:
                        PlayOrPause_ToggleButton.IsChecked = false;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });
        }

        private async void Player_PositionChanged(MediaPlaybackSession sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    if (!isPressPositionSlider)
                        Position_Slider.Value = sender.Position.TotalMinutes;
                });
        }

        private void PlayOrPause_ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            var theButton = sender as ToggleButton;
            if (theButton != null)
                theButton.Content = '\uE103';

            Play();
        }

        private void PlayOrPause_ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            var theButton = sender as ToggleButton;
            if (theButton != null)
                theButton.Content = '\uE102';

            Pause();
        }

        private void Position_Slider_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            isPressPositionSlider = true;
        }
        
        private void Position_Slider_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            SetPosition(TimeSpan.FromMinutes(Position_Slider.Value));
            isPressPositionSlider = false;
        }
    }
}
