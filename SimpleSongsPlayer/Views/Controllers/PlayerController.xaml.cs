using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using HappyStudio.UwpToolsLibrary.Auxiliarys;
using NCEWalkman.Models;
using SimpleSongsPlayer.DataModel;
using SimpleSongsPlayer.DataModel.Exceptions;
using SimpleSongsPlayer.Log;
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

            LoggerMembers.PlayerLogger.Info("开始初始化控制条， 正在获取必要的数据");

            vm = ((ViewModelLocator)Application.Current.Resources["Locator"]).PlayerController;
            player = App.Player;
            settingProperties = SettingProperties.Current;
            LoopingMode_ListBox.SelectedIndex = (int) settingProperties.LoopingMode;

            LoggerMembers.PlayerLogger.Info("正在关联事件");

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

            LoggerMembers.PlayerLogger.Info("已完成播放控制条初始化");
        }

        public List<Song> AllSongs
        {
            get => (List<Song>)GetValue(AllSongsProperty);
            set => SetValue(AllSongsProperty, value);
        }

        private void UpdateInfo(MediaPlaybackItem media)
        {
            LoggerMembers.PlayerLogger.Info("正在刷新歌曲信息");
            vm.CurrentSong = AllSongs.Find(s => s.PlaybackItem == media);
            AllSongs.ForEach(s => s.IsPlaying = false);
            vm.CurrentSong.IsPlaying = true;
            PlayItemChangeNotifier.SendChangeNotification(vm.CurrentSong);

            LoggerMembers.PlayerLogger.Info("正在设置音量和速率");
            player.Volume = settingProperties.Volume;
            player.PlaybackSession.PlaybackRate = settingProperties.PlaybackSpeed;

            LoggerMembers.PlayerLogger.Info("正在更新锁屏信息");
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
            LoggerMembers.PlayerLogger.Info("正在设置进度");
            player.PlaybackSession.Position = newTime;
            PositionChangeNotifier.SendChangeNotification(true, newTime);
            LoggerMembers.PlayerLogger.Info("完成进度设置");
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

            string optionInfo = isAdd ? "快进" : "快退";
            LoggerMembers.PlayerLogger.Info($"正在执行 {optionInfo} 操作");

            while (isPressPositionControlButton == true)
            {
                if (isAdd)
                    SetPosition(player.PlaybackSession.Position + TimeSpan.FromSeconds(1));
                else
                    SetPosition(player.PlaybackSession.Position - TimeSpan.FromSeconds(1));
                LoggerMembers.PlayerLogger.Info($"已 {optionInfo} 1秒");

                await Task.Delay(TimeSpan.FromMilliseconds(200));
            }

            LoggerMembers.PlayerLogger.Info($"已完成 {optionInfo} 操作");
        }

        private void ReleasePositionButton(bool isNextSong)
        {
            bool? b = isPressPositionControlButton;
            isPressPositionControlButton = false;

            if (b == null)
            {
                string optionInfo = isNextSong ? "下一曲" : "上一曲";
                LoggerMembers.PlayerLogger.Info($"正在执行 {optionInfo} 操作");

                if (isNextSong)
                    vm.PlayerSource.MoveNext();
                else
                    vm.PlayerSource.MovePrevious();

                LoggerMembers.PlayerLogger.Info($"完成 {optionInfo} 操作");
            }
            else
                Play();

        }

        private void SwitchLoopingMode(LoopingModeEnum mode)
        {
            player.IsLoopingEnabled = false;
            if (vm.PlayerSource != null)
                vm.PlayerSource.ShuffleEnabled = false;

            LoggerMembers.PlayerLogger.Info($"重置循环模式为 列表循环");
            
            switch (mode)
            {
                case LoopingModeEnum.Single:
                    player.IsLoopingEnabled = true;
                    LoggerMembers.PlayerLogger.Info($"设置循环模式为 单曲循环");
                    break;
                case LoopingModeEnum.Random:
                    if (vm.PlayerSource != null)
                    {
                        vm.PlayerSource.ShuffleEnabled = true;
                        LoggerMembers.PlayerLogger.Info($"设置循环模式为 随机播放");
                    }
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
            LoggerMembers.PlayerLogger.Info("正在更新歌曲组信息");

            if (vm.PlayerSource != null)
            {
                LoggerMembers.PlayerLogger.Info("正在取消监听旧歌曲组");
                vm.PlayerSource.CurrentItemChanged -= PlayerSource_CurrentItemChanged;
                vm.PlayerSource.ItemFailed -= PlayerSource_ItemFailed;
                vm.PlayerSource = null;
            }

            if (sender.Source == null)
                return;

            LoggerMembers.PlayerLogger.Info("正在监听新歌曲组");

            vm.PlayerSource = (MediaPlaybackList)sender.Source;
            vm.PlayerSource.CurrentItemChanged += PlayerSource_CurrentItemChanged;
            vm.PlayerSource.ItemFailed += PlayerSource_ItemFailed;
            vm.PlayerSource.AutoRepeatEnabled = true;

            LoggerMembers.PlayerLogger.Info("正在对新歌曲组应用循环模式");
            SwitchLoopingMode(settingProperties.LoopingMode);
            LoggerMembers.PlayerLogger.Info("完成更新歌曲组信息");
        }

        private async void PlayerSource_ItemFailed(MediaPlaybackList sender, MediaPlaybackItemFailedEventArgs args)
        {
            if (!App.isInBackground)
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    LoggerMembers.PlayerLogger.Error(args.Error.ExtendedError);

                    await MessageBox.ShowAsync(ExceptionResource.ErrorInfoStrings.GetString("SongLoadFail"), args.Error.ExtendedError.ToShortString(),
                        App.MessageBoxResourceLoader.GetString("Close"));
                });
        }

        private async void PlayerSource_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (args.NewItem == null)
                    return;

                LoggerMembers.PlayerLogger.Info("正在更换歌曲");
                UpdateInfo(args.NewItem);

                LoggerMembers.PlayerLogger.Info("正在获取歌曲持续时间");
                Position_Slider.Maximum = args.NewItem.Source.Duration.GetValueOrDefault(TimeSpan.Zero).TotalMinutes;

                LoggerMembers.PlayerLogger.Info("完成更换");
            });
        }

        private async void Player_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                switch (sender.PlaybackState)
                {
                    case MediaPlaybackState.None:
                        LoggerMembers.PlayerLogger.Info("注销播放线程");
                        break;
                    case MediaPlaybackState.Opening:
                        LoggerMembers.PlayerLogger.Info("正在打开歌曲");
                        break;
                    case MediaPlaybackState.Buffering:
                        LoggerMembers.PlayerLogger.Info("正在读取歌曲数据");
                        break;
                    case MediaPlaybackState.Playing:
                        LoggerMembers.PlayerLogger.Info("开始播放歌曲");

                        PlayOrPause_ToggleButton.IsChecked = true;
                        break;
                    case MediaPlaybackState.Paused:
                        LoggerMembers.PlayerLogger.Info("暂停播放歌曲");

                        PlayOrPause_ToggleButton.IsChecked = false;
                        break;
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
            if (theButton is null)
                return;

            LoggerMembers.PlayerLogger.Info("点击播放按钮");

            theButton.Content = '\uE103';

            Play();
        }

        private void PlayOrPause_ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            var theButton = sender as ToggleButton;
            if (theButton is null)
                return;

            LoggerMembers.PlayerLogger.Info("点击暂停按钮");

            theButton.Content = '\uE102';

            Pause();
        }

        private void Position_Slider_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            LoggerMembers.PlayerLogger.Info("正在拖动进度条");

            isPressPositionSlider = true;
        }

        private void Position_Slider_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            LoggerMembers.PlayerLogger.Info("正在更改播放进度");

            SetPosition(TimeSpan.FromMinutes(Position_Slider.Value));
            isPressPositionSlider = false;

            LoggerMembers.PlayerLogger.Info("完成进度更改");
        }

        private void PlayList_Flyout_Opened(object sender, object e)
        {
            if (vm.PlayerSource == null)
                return;

            LoggerMembers.PlayerLogger.Info("已打开正在播放列表， 正在加载数据");

            List<Song> playList = new List<Song>();
            foreach (var item in vm.PlayerSource.Items)
            {
                var fundSong = AllSongs.FirstOrDefault(s => s.PlaybackItem == item);
                if (fundSong != null)
                    playList.Add(fundSong);
            }

            PlayList_ListView.ItemsSource = playList;
            PlayList_ListView.ScrollIntoView(vm.CurrentSong, ScrollIntoViewAlignment.Leading);

            LoggerMembers.PlayerLogger.Info("正在播放列表数据加载完成");
        }

        private void PlayList_ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Song song = e.ClickedItem as Song;
            if (song == null || vm.CurrentSong.Equals(song))
                return;

            LoggerMembers.PlayerLogger.Info("正在通过正在播放列表更换歌曲");

            int id = vm.PlayerSource.Items.IndexOf(song.PlaybackItem);
            vm.PlayerSource.MoveTo((uint)id);
            Play();

            LoggerMembers.PlayerLogger.Info("完成更换");
        }

        private void PlayList_Flyout_OnClosed(object sender, object e)
        {
            LoggerMembers.PlayerLogger.Info("已关闭正在播放列表");

            PlayList_ListView.ItemsSource = null;
        }

        private void PlayList_Close_Button_Click(object sender, RoutedEventArgs e)
        {
            LoggerMembers.PlayerLogger.Info("点击按钮 关闭正在播放列表");

            PlayList_Flyout.Hide();
        }

        private void LoopingMode_ListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoggerMembers.PlayerLogger.Info("正在更改循环模式");

            settingProperties.LoopingMode = (LoopingModeEnum) LoopingMode_ListBox.SelectedIndex;
            LoopingMode_Flyout.Hide();

        }

        private void SavePlayingList_Button_OnClick(object sender, RoutedEventArgs e)
        {
            if (PlayList_ListView.ItemsSource is List<Song> songs)
            {
                LoggerMembers.PlayerLogger.Info("点击按钮 保存正在播放列表， 正在提取歌曲");

                var items = new List<SongItem>();
                foreach (var song in songs)
                    items.Add(new SongItem(song));

                LoggerMembers.PlayerLogger.Info("完成歌曲提取， 正在发送保存请求");

                PlayingListOperationNotifier.RequestAdd(items);

                LoggerMembers.PlayerLogger.Info("已发送保存请求");
            }
        }
    }
}
