using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.UI.Xaml.Data;

namespace SimpleSongsPlayer.Views.Converters
{
    public class EnumToInt32Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (int)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return Enum.ToObject(targetType, value);
        }
    }
}
