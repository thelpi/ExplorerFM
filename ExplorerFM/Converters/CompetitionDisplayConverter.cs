using System;
using System.Globalization;
using System.Windows.Data;
using ExplorerFM.Datas;

namespace ExplorerFM.Converters
{
    class CompetitionDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // TODO: check if the way of 'ClubDisplayConverter' is better
            return value == null
                ? string.Empty
                : (value as Competition).Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
