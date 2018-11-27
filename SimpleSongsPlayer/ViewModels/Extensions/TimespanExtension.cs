using System;

namespace SimpleSongsPlayer.ViewModels.Extensions
{
    public static class TimespanExtension
    {
        public static string ToSongTimeString(this TimeSpan time)
        {
            if (time.Days > 0)
                return $"{time.Days}:{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
            else if (time.Hours > 0)
                return $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
            else
                return $"{time.Minutes:D2}:{time.Seconds:D2}";
        }
    }
}