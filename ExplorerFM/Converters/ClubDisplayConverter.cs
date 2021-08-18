using System;
using System.Globalization;
using System.Windows.Data;
using ExplorerFM.Datas;

namespace ExplorerFM.Converters
{
    class ClubDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null
                ? (parameter != null ? parameter.ToString() : string.Empty)
                : (value as Club).Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
