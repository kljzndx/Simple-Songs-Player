using System;
using SimpleSongsPlayer.DataModel;

namespace SimpleSongsPlayer.ViewModels.Events
{
    public class LyricsPreviewRefreshEventArgs : EventArgs
    {
        public LyricsPreviewRefreshEventArgs(LyricLine currentLyric)
        {
            CurrentLyric = currentLyric;
        }

        public LyricLine CurrentLyric { get; set; }
    }
}