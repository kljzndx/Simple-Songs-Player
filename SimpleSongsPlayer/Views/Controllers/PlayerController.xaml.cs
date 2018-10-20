using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using HappyStudio.UwpToolsLibrary.Auxiliarys;
using NCEWalkman.Models;
using SimpleSongsPlayer.DataModel;
using SimpleSongsPlayer.DataModel.Exceptions;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.ViewModels;
using SimpleSongsPlayer.ViewModels.Controllers;
using SimpleSongsPlayer.ViewModels.Events;

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

        public event RoutedEventHandler CoverClick;

        public PlayerController()
        {
            this.InitializeComponent();
            vm = ((ViewModelLocator)Application.Current.Resources["Locator"]).PlayerController;
            player = App.Player;
            settingProperties = SettingProperties.Current;
            LoopingMode_ListBox.SelectedIndex = (int) settingProperties.LoopingMode;

            player.SourceChanged += Player_SourceChanged;
            player.PlaybackSession.PositionChanged += Player_PositionChanged;
            player.PlaybackSession.PlaybackStateChanged += Player_PlaybackStateChanged;

            settingProperties.PropertyChanged += SettingProperties_PropertyChanged;
            AlbumCover_Button.Click += (s, e) => CoverClick?.Invoke(s, e);

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
            get => (List<Song>)GetValue(AllSongsProperty);
            set => SetValue(AllSongsProperty, value);
        }

        private void UpdateInfo(MediaPlaybackItem media)
        {
            vm.CurrentSong = AllSongs.Find(s => s.PlaybackItem == media);
            AllSongs.ForEach(s => s.IsPlaying = false);
            vm.CurrentSong.IsPlaying = true;

            player.Volume = settingProperties.Volume;
            player.PlaybackSession.PlaybackRate = settingProperties.PlaybackSpeed;
            PlayItemChangeNotifier.SendChangeNotification(vm.CurrentSong);

            // 更新锁屏信息
            var mediaProperties = player.SystemMediaTransportControls.DisplayUpdater;
            mediaProperties.Type = MediaPlaybackType.Music;
            mediaProperties.MusicProperties.Title = vm.CurrentSong.Title;
            mediaProperties.MusicProperties.Artist = vm.CurrentSong.Singer;
            mediaProperties.MusicProperties.AlbumTitle = vm.CurrentSong.Album;
            mediaProperties.Thumbnail = RandomAccessStreamReference.CreateFromStream(vm.CurrentSong.CoverStream);
            mediaProperties.Update();
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
            PositionChangeNotifier.SendChangeNotification(true, newTime);
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

        private void SwitchLoopingMode(LoopingModeEnum mode)
        {
            player.IsLoopingEnabled = false;
            if (vm.PlayerSource != null)
                vm.PlayerSource.ShuffleEnabled = false;

            switch (mode)
            {
                case LoopingModeEnum.Single:
                    player.IsLoopingEnabled = true;
                    break;
                case LoopingModeEnum.Random:
                    if (vm.PlayerSource != null)
                        vm.PlayerSource.ShuffleEnabled = true;
                    break;
            }
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
                case nameof(settingProperties.LoopingMode):
                    SwitchLoopingMode(settingProperties.LoopingMode);
                    break;
            }
        }

        private void Player_SourceChanged(MediaPlayer sender, object args)
        {
            if (vm.PlayerSource != null)
            {
                vm.PlayerSource.CurrentItemChanged -= PlayerSource_CurrentItemChanged;
                vm.PlayerSource.ItemFailed -= PlayerSource_ItemFailed;
                vm.PlayerSource = null;
            }

            if (sender.Source == null)
                return;

            vm.PlayerSource = (MediaPlaybackList)sender.Source;
            vm.PlayerSource.CurrentItemChanged += PlayerSource_CurrentItemChanged;
            vm.PlayerSource.ItemFailed += PlayerSource_ItemFailed;
            vm.PlayerSource.AutoRepeatEnabled = true;

            SwitchLoopingMode(settingProperties.LoopingMode);
        }

        private async void PlayerSource_ItemFailed(MediaPlaybackList sender, MediaPlaybackItemFailedEventArgs args)
        {
            if (!App.isInBackground)
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    MessageBox.ShowAsync(ExceptionResource.ErrorInfoStrings.GetString("SongLoadFail"), args.Error.ExtendedError.ToShortString(),
                        App.MessageBoxResourceLoader.GetString("Close"));
                });
        }

        private async void PlayerSource_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (args.NewItem == null)
                    return;

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
                    {
                        Position_Slider.Value = sender.Position.TotalMinutes;
                        PositionChangeNotifier.SendChangeNotification(false, sender.Position);
                    }
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

        private void PlayList_Flyout_Opened(object sender, object e)
        {
            if (vm.PlayerSource == null)
                return;

            List<Song> playList = new List<Song>();
            foreach (var item in vm.PlayerSource.Items)
            {
                var fundSong = AllSongs.FirstOrDefault(s => s.PlaybackItem == item);
                if (fundSong != null)
                    playList.Add(fundSong);
            }

            PlayList_ListView.ItemsSource = playList;
            PlayList_ListView.ScrollIntoView(vm.CurrentSong, ScrollIntoViewAlignment.Leading);
        }

        private void PlayList_ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Song song = e.ClickedItem as Song;
            if (song == null || vm.CurrentSong.Equals(song))
                return;

            int id = vm.PlayerSource.Items.IndexOf(song.PlaybackItem);
            vm.PlayerSource.MoveTo((uint)id);
            Play();
        }

        private void PlayList_Flyout_OnClosed(object sender, object e)
        {
            PlayList_ListView.ItemsSource = null;
        }

        private void PlayList_Close_Button_Click(object sender, RoutedEventArgs e)
        {
            PlayList_Flyout.Hide();
        }

        private void LoopingMode_ListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            settingProperties.LoopingMode = (LoopingModeEnum) LoopingMode_ListBox.SelectedIndex;
            LoopingMode_Flyout.Hide();
        }

        private void SavePlayingList_Button_OnClick(object sender, RoutedEventArgs e)
        {
            if (PlayList_ListView.ItemsSource is List<Song> songs)
            {
                var items = new List<SongItem>();
                foreach (var song in songs)
                    items.Add(new SongItem(song));

                PlayingListOperationNotifier.RequestAdd(items);
            }
        }
    }
}
