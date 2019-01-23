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
using SimpleSongsPlayer.Log;
using SimpleSongsPlayer.Service;
using SimpleSongsPlayer.ViewModels;
using SimpleSongsPlayer.ViewModels.DataServers;
using SimpleSongsPlayer.ViewModels.Events;
using SimpleSongsPlayer.ViewModels.SettingProperties;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SimpleSongsPlayer.Views.Controllers
{
    public sealed partial class CustomMediaPlayerElement : UserControl
    {
        private static MediaPlayer player;

        public static MediaPlaybackItem CurrentItem { get; private set; }

        public static event TypedEventHandler<CustomMediaPlayerElement, PlayerPositionChangeEventArgs> PositionChanged;
        public static event TypedEventHandler<CustomMediaPlayerElement, PlayerNowPlaybackItemChangeEventArgs> NowPlaybackItemChanged;

        private bool? isPressPositionControlButton = false;
        private bool isUserChangePositon;

        private MusicFileDataServer dataServer = MusicFileDataServer.Current;
        private PlayerSettingProperties settings = PlayerSettingProperties.Current;

        public CustomMediaPlayerElement()
        {
            this.InitializeComponent();
            settings.PropertyChanged += Settings_PropertyChanged;
            player = Root_MediaPlayerElement.MediaPlayer;
            Install(player);

            MyTransportControls.RepeatMode_SelectedID = (int) settings.RepeatMode;
            MyTransportControls.CoverButton_Click += (s, e) => CoverButton_Click?.Invoke(this, e);

            NowPlaybackItemChanged += CustomMediaPlayerElement_NowPlaybackItemChanged;
        }

        public event RoutedEventHandler CoverButton_Click;

        public void SetMediaPlayer(MediaPlayer mediaPlayer)
        {
            Uninstall(player);
            player = mediaPlayer;
            Root_MediaPlayerElement.SetMediaPlayer(player);
            Install(player);
        }

        private void Install(MediaPlayer mediaPlayer)
        {
            mediaPlayer.SourceChanged += MediaPlayer_SourceChanged;
            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            mediaPlayer.MediaFailed += MediaPlayer_MediaFailed;
            mediaPlayer.VolumeChanged += MediaPlayer_VolumeChanged;
            
            var session = mediaPlayer.PlaybackSession;
            session.PlaybackStateChanged += MediaPlayer_Session_PlaybackStateChanged;
            session.PositionChanged += MediaPlayer_Session_PositionChanged;
            session.PlaybackRateChanged += Session_PlaybackRateChanged;
        }

        private void Uninstall(MediaPlayer mediaPlayer)
        {
            mediaPlayer.SourceChanged -= MediaPlayer_SourceChanged;
            mediaPlayer.MediaOpened -= MediaPlayer_MediaOpened;
            mediaPlayer.MediaEnded -= MediaPlayer_MediaEnded;
            mediaPlayer.MediaFailed -= MediaPlayer_MediaFailed;
            mediaPlayer.VolumeChanged -= MediaPlayer_VolumeChanged;
            var session = mediaPlayer.PlaybackSession;
            session.PlaybackStateChanged -= MediaPlayer_Session_PlaybackStateChanged;
            session.PositionChanged -= MediaPlayer_Session_PositionChanged;
            session.PlaybackRateChanged -= Session_PlaybackRateChanged;
        }

        private static bool TryGetSession(out MediaPlaybackSession session)
        {
            session = player.PlaybackSession;
            return session != null;
        }

        private void SetupPlayer()
        {
            var source = player.Source as MediaPlaybackList;
            // 循环模式
            {
                if (source != null)
                {
                    source.AutoRepeatEnabled = true;

                    switch ((int) settings.RepeatMode)
                    {
                        case 0:
                            player.IsLoopingEnabled = true;
                            break;
                        case 1:
                            source.ShuffleEnabled = false;
                            break;
                        case 2:
                            source.ShuffleEnabled = true;
                            break;
                    }
                }
            }
            // 音量
            {
                player.Volume = settings.Volume;
            }

            {
                if (source != null)
                    source.CurrentItemChanged += Source_CurrentItemChanged;
            }
        }

        private void SetPosition(TimeSpan position)
        {
            if (TryGetSession(out var session))
            {
                isUserChangePositon = true;
                session.Position = position;
                PositionChanged?.Invoke(this, new PlayerPositionChangeEventArgs(true, position));
                isUserChangePositon = false;
            }
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

            player.Pause();

            string optionInfo = isAdd ? "快进" : "快退";
            this.LogByObject($"正在执行 {optionInfo} 操作");

            while (isPressPositionControlButton == true)
            {
                if (isAdd)
                    SetPosition(player.PlaybackSession.Position + TimeSpan.FromSeconds(1));
                else
                    SetPosition(player.PlaybackSession.Position - TimeSpan.FromSeconds(1));
                this.LogByObject($"已 {optionInfo} 1秒");

                await Task.Delay(TimeSpan.FromMilliseconds(200));
            }

            this.LogByObject($"已完成 {optionInfo} 操作");
        }

        private void ReleasePositionButton(bool isNextSong)
        {
            bool? b = isPressPositionControlButton;
            isPressPositionControlButton = false;

            if (b == null && player.Source is MediaPlaybackList mpl)
            {
                string optionInfo = isNextSong ? "下一曲" : "上一曲";
                this.LogByObject($"正在执行 {optionInfo} 操作");

                if (isNextSong)
                    mpl.MoveNext();
                else
                    mpl.MovePrevious();

                this.LogByObject($"完成 {optionInfo} 操作");
            }
            else
                player.Play();
        }

        public static void SetPosition_Global(TimeSpan position)
        {
            if (TryGetSession(out var session))
            {
                session.Position = position;
                PositionChanged?.Invoke(null, new PlayerPositionChangeEventArgs(true, position));
            }
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(settings.PlaybackRate):
                    if (TryGetSession(out var session))
                        session.PlaybackRate = settings.PlaybackRate;
                    break;
            }
        }

        private void CustomTransportControls_OnRepeatModeSelectionChanged(CustomTransportControls sender, KeyValuePair<int, string> args)
        {
            settings.RepeatMode = (PlaybackRepeatModeEnum) args.Key;

            if (player.Source is MediaPlaybackList mpl)
            {
                switch (args.Key)
                {
                    case 0:
                        player.IsLoopingEnabled = true;
                        break;
                    case 1:
                        mpl.ShuffleEnabled = false;
                        break;
                    case 2:
                        mpl.ShuffleEnabled = true;
                        break;
                }
            }
        }

        private async void MediaPlayer_SourceChanged(MediaPlayer sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                if (sender.Source is null)
                {
                    NowPlaybackItemChanged?.Invoke(this, new PlayerNowPlaybackItemChangeEventArgs(CurrentItem, null));
                    CurrentItem = null;
                    return;
                }

                SetupPlayer();

                if (sender.Source is MediaPlaybackList mpl)
                    await NowPlayingDataServer.Current.SetUpSource(mpl);
            });
        }

        private async void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (sender.Source is MediaPlaybackItem mpi)
                {
                    NowPlaybackItemChanged?.Invoke(this, new PlayerNowPlaybackItemChangeEventArgs(CurrentItem, mpi));
                    CurrentItem = mpi;
                }
            });
        }

        private async void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => PositionChanged?.Invoke(this, new PlayerPositionChangeEventArgs(true, TimeSpan.Zero)));
        }

        private async void MediaPlayer_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => throw new Exception(args.ErrorMessage, args.ExtendedErrorCode));
        }

        private void MediaPlayer_VolumeChanged(MediaPlayer sender, object args)
        {
            settings.Volume = sender.Volume;
        }

        private async void MediaPlayer_Session_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                switch (sender.PlaybackState)
                {
                    case MediaPlaybackState.None:
                        this.LogByObject("播放会话被注销");
                        break;
                    case MediaPlaybackState.Opening:
                        this.LogByObject("正在打开文件");
                        break;
                    case MediaPlaybackState.Buffering:
                        this.LogByObject("正在加载文件");
                        break;
                    case MediaPlaybackState.Playing:
                        this.LogByObject("开始播放音乐");
                        break;
                    case MediaPlaybackState.Paused:
                        this.LogByObject("暂停音乐播放");
                        break;
                }
            });
        }

        private async void Source_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                NowPlaybackItemChanged?.Invoke(this, new PlayerNowPlaybackItemChangeEventArgs(CurrentItem, args.NewItem));
                CurrentItem = args.NewItem;

                if (TryGetSession(out var session))
                    session.PlaybackRate = settings.PlaybackRate;
            });
        }

        private async void Session_PlaybackRateChanged(MediaPlaybackSession sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    MyTransportControls.PlaybackRate = sender.PlaybackRate;
                });
        }

        #region Position

        private async void MediaPlayer_Session_PositionChanged(MediaPlaybackSession sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (!isUserChangePositon)
                    PositionChanged?.Invoke(this, new PlayerPositionChangeEventArgs(false, sender.Position));
            });
        }

        private void MyTransportControls_OnPositionSlider_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            isUserChangePositon = true;
        }

        private void MyTransportControls_OnPositionSlider_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (TryGetSession(out var session))
                PositionChanged?.Invoke(this, new PlayerPositionChangeEventArgs(true, session.Position));

            isUserChangePositon = false;
        }

        #endregion

        #region PositionControl

        private async void MyTransportControls_OnRewindButton_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            await PressPositionButton(false);
        }

        private void MyTransportControls_OnRewindButton_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ReleasePositionButton(false);
        }

        private async void MyTransportControls_OnFastForwardButton_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            await PressPositionButton(true);
        }

        private void MyTransportControls_OnFastForwardButton_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ReleasePositionButton(true);
        }

        #endregion

        private async void CustomMediaPlayerElement_NowPlaybackItemChanged(CustomMediaPlayerElement sender, PlayerNowPlaybackItemChangeEventArgs args)
        {
            bool isUpdated = false;
            foreach (var fileDto in dataServer.Data.Where(f => f.IsInitPlaybackItem))
            {
                if (!isUpdated && await fileDto.GetPlaybackItem() == args.NewItem)
                {
                    MyTransportControls.CoverSource = await fileDto.GetAlbumCover();

                    var mediaProperties = player.SystemMediaTransportControls.DisplayUpdater;
                    mediaProperties.Type = MediaPlaybackType.Music;
                    mediaProperties.Thumbnail = RandomAccessStreamReference.CreateFromStream(await fileDto.GetThumbnail());
                    var musicProperties = mediaProperties.MusicProperties;

                    musicProperties.Title = fileDto.Title;
                    musicProperties.Artist = fileDto.Artist;
                    musicProperties.AlbumTitle = fileDto.Album;
                    mediaProperties.Update();

                    fileDto.IsPlaying = true;
                    isUpdated = true;
                    continue;
                }
                fileDto.IsPlaying = false;
            }
        }

        private void MyTransportControls_OnRateValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            settings.PlaybackRate = e.NewValue;
        }
    }
}
