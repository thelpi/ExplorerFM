using System;
using System.Globalization;
using System.Windows.Data;

namespace ExplorerFM.Converters
{
    class DateDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null
                ? string.Empty
                : ((DateTime)value).ToString("yyyy-MM-dd");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
