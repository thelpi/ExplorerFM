using System;
using System.Globalization;
using System.Windows.Data;
using ExplorerFM.Datas;

namespace ExplorerFM.Converters
{
    class CountryDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null
                ? string.Empty
                : (value as Country).Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
