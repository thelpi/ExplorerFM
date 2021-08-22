using System;
using System.Globalization;
using System.Windows.Data;
using ExplorerFM.RuleEngine;

namespace ExplorerFM.Converters
{
    class ComparatorDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() != typeof(Comparator))
                return null;

            var realValue = (Comparator)value;

            switch (realValue)
            {
                case Comparator.Equal:
                    return "=";
                case Comparator.Greater:
                    return ">";
                case Comparator.GreaterEqual:
                    return ">=";
                case Comparator.Lower:
                    return "<";
                case Comparator.LowerEqual:
                    return "<=";
                case Comparator.NotEqual:
                    return "<>";
                case Comparator.Like:
                    return "⊆";
                case Comparator.NotLike:
                    return "⊈";
                default:
                    throw new NotSupportedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
