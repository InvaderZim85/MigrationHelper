using System;
using System.Globalization;
using System.Windows.Data;

namespace MigrationHelper.Ui
{
    public class FileSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is long length))
                return 0;


            switch (length)
            {
                case var _ when length < 1024:
                    return "1 KB";
                case var _ when length >= 1024 && length < Math.Pow(1024, 2):
                    return $"{length / 1024d:N2} KB";
                case var _ when length >= Math.Pow(1024, 2) && length < Math.Pow(1024, 3):
                    return $"{length / Math.Pow(1024, 2):N2} MB";
                case var _ when length >= Math.Pow(1024, 3):
                    return $"{length / Math.Pow(1024, 2):N2} GB";
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
