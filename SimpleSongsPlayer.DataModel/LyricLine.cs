using System;

namespace SimpleSongsPlayer.DataModel
{
    public class LyricLine : ObservableObject, IComparable<LyricLine>
    {
        internal LyricLine(TimeSpan time, string content)
        {
            Time = time;
            Content = content;
        }

        private bool isSelected;

        public bool IsSelected
        {
            get => isSelected;
            set => Set(ref isSelected, value);
        }
        public TimeSpan Time { get; }
        public string Content { get; }

        public int CompareTo(LyricLine other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return Time.CompareTo(other.Time);
        }
    }
}