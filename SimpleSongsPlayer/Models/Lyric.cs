using System;
using GalaSoft.MvvmLight;

namespace SimpleSongsPlayer.Models
{
    public class Lyric : ObservableObject, IComparable<Lyric>
    {
        public Lyric(TimeSpan time, string content)
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

        public int CompareTo(Lyric other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return Time.CompareTo(other.Time);
        }
    }
}