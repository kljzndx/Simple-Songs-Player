using System;
using GalaSoft.MvvmLight;

namespace SimpleSongsPlayer.Models.DTO.Lyric
{
    public class LyricLine : ObservableObject, IComparable<LyricLine>
    {
        public static readonly LyricLine Empty = new LyricLine(TimeSpan.Zero, String.Empty);

        private bool isSelected;

        public LyricLine(TimeSpan time)
        {
            Time = time;
        }

        public LyricLine(TimeSpan time, string content) : this(time)
        {
            Content = content;
        }

        public bool IsSelected
        {
            get => isSelected;
            set => Set(ref isSelected, value);
        }

        public TimeSpan Time { get; }
        public string Content { get; set; }

        public int CompareTo(LyricLine other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return Time.CompareTo(other.Time);
        }
    }
}