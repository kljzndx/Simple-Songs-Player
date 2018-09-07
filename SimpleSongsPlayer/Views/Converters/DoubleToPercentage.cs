using System;
using Windows.UI.Xaml.Data;

namespace SimpleSongsPlayer.Views.Converters
{
    public class DoubleToPercentage : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((double) value) * 100;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return ((double) value) / 100;
        }
    }
}