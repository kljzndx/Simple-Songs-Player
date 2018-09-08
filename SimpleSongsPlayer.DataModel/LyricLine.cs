using System;

namespace SimpleSongsPlayer.DataModel
{
    public class LyricLine : ObservableObject, IComparable<LyricLine>
    {
        public static LyricLine Empty = new LyricLine(TimeSpan.Zero);

        private bool isSelected;

        internal LyricLine(TimeSpan time)
        {
            Time = time;
            Content = String.Empty;
        }

        internal LyricLine(TimeSpan time, string content) : this(time)
        {
            Content = content;
        }

        public bool IsSelected
        {
            get => isSelected;
            set => Set(ref isSelected, value);
        }
        public TimeSpan Time { get; }
        public string Content { get; internal set; }

        public int CompareTo(LyricLine other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return Time.CompareTo(other.Time);
        }
    }
}