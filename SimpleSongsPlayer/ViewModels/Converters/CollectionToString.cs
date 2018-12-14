using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace SimpleSongsPlayer.ViewModels.Converters
{
    public class CollectionToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            IEnumerable<string> l = (IEnumerable<string>)value;
            StringBuilder builder = new StringBuilder();
            foreach (var s in l)
                builder.Append(s + " ");

            return builder.ToString().Trim();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}