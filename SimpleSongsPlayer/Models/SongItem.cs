using System;
using System.ComponentModel;
using Windows.Media.Playback;
using Windows.UI.Xaml.Media.Imaging;
using SimpleSongsPlayer.DataModel;
using ObservableObject = GalaSoft.MvvmLight.ObservableObject;

namespace SimpleSongsPlayer.Models
{
    public class SongItem : ObservableObject
    {
        private readonly Song _song;

        private bool isPlaying;
        private bool isSelected;

        public SongItem(Song song)
        {
            _song = song;

            Title = song.Title;
            Singer = song.Singer;
            Album = song.Album;
            AlbumCover = song.AlbumCover;
            Duration = song.Duration;
            PlaybackItem = song.PlaybackItem;
            Path = song.Path;

            _song.PropertyChanged += Song_PropertyChanged;
        }

        private void Song_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_song.IsPlaying):
                    IsPlaying = _song.IsPlaying;
                    break;
            }
        }

        public bool IsPlaying
        {
            get => isPlaying;
            private set => Set(ref isPlaying, value);
        }

        public bool IsSelected
        {
            get => isSelected;
            set => Set(ref isSelected, value);
        }

        public string Title { get; }
        public string Singer { get; }
        public string Album { get; }
        public BitmapSource AlbumCover { get; }
        public TimeSpan Duration { get; }
        public MediaPlaybackItem PlaybackItem { get; }
        public string Path { get; }
    }
}