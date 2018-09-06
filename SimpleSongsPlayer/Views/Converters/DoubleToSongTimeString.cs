using System;
using Windows.UI.Xaml.Data;
using SimpleSongsPlayer.Models;

namespace SimpleSongsPlayer.Views.Converters
{
    public class DoubleToSongTimeString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return TimeSpan.FromMinutes((double) value).ToSongTimeString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}