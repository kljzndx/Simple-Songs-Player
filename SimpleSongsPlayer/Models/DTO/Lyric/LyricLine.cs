using System;

namespace SimpleSongsPlayer.Models.DTO.Lyric
{
    public class LyricLine : IComparable<LyricLine>
    {
        public LyricLine(TimeSpan time)
        {
            Time = time;
        }

        public LyricLine(TimeSpan time, string content) : this(time)
        {
            Content = content;
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