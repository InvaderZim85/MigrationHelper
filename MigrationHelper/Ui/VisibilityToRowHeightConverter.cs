using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MigrationHelper.Ui
{
    public class VisibilityToRowHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool tmpValue)
            {
                if (tmpValue)
                    return new GridLength(1, GridUnitType.Star);
                else
                    return new GridLength(0);
            }
            return new GridLength(1, GridUnitType.Star);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
