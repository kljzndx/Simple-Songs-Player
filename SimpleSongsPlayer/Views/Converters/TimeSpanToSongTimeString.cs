using System;
using Windows.UI.Xaml.Data;

namespace SimpleSongsPlayer.Views.Converters
{
    public class TimeSpanToSongTimeString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            TimeSpan time = (TimeSpan) value;
            if (time.Days > 0)
                return $"{time.Days}:{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
            else if (time.Hours > 0)
                return $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
            else
                return $"{time.Minutes:D2}:{time.Seconds:D2}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}