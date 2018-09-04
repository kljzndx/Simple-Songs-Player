using System;
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

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SimpleSongsPlayer.Views.Controllers
{
    public sealed partial class PlayerController : UserControl
    {
        public static readonly DependencyProperty AllSongsProperty = DependencyProperty.Register(
            nameof(AllSongs), typeof(List<Song>), typeof(PlayerController), new PropertyMetadata(null));

        private MediaPlaybackList playerSource;

        public PlayerController()
        {
            this.InitializeComponent();
            App.Player.SourceChanged += Player_SourceChanged;
            App.Player.PlaybackSession.PositionChanged += Player_PositionChanged;
        }

        public List<Song> AllSongs
        {
            get => (List<Song>) GetValue(AllSongsProperty);
            set => SetValue(AllSongsProperty, value);
        }

        private void ShowSongInfo(MediaPlaybackItem media)
        {
            Song theSong = AllSongs.Find(s => s.PlaybackItem == media);
            SongInfoShowed.Title = theSong.Title;
            SongInfoShowed.Artist = theSong.Singer;
            SongInfoShowed.CoverSource = theSong.AlbumCover;

            // 测试失败，获取不到标题和歌手信息 --2018/09/04
            //var mediaProperties = media.GetDisplayProperties();
            //var audioProperties = mediaProperties.MusicProperties;
            //var cover = new BitmapImage();
            //if (mediaProperties.Thumbnail != null)
            //    cover.SetSource(await mediaProperties.Thumbnail.OpenReadAsync());

            //SongInfoShowed.Title = audioProperties.Title;
            //SongInfoShowed.Artist = audioProperties.Artist;
            //SongInfoShowed.CoverSource = cover;
        }

        private void Player_PositionChanged(MediaPlaybackSession sender, object args)
        {
        }

        private void Player_SourceChanged(MediaPlayer sender, object args)
        {
            if (playerSource != null)
                playerSource.CurrentItemChanged -= PlayerSource_CurrentItemChanged;

            playerSource = (MediaPlaybackList) sender.Source;
            playerSource.CurrentItemChanged += PlayerSource_CurrentItemChanged;
        }

        private async void PlayerSource_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                ShowSongInfo(args.NewItem);
            });
        }
    }
}
