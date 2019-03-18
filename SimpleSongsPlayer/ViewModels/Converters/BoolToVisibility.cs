using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace SimpleSongsPlayer.ViewModels.Converters
{
    public class BoolToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is null)
                return Visibility.Collapsed;

            return (bool) value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is null)
                return false;

            return (Visibility) value == Visibility.Visible;
        }
    }
}