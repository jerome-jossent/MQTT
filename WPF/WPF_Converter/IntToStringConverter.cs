using System;
using System.Globalization;
using System.Windows.Data;

namespace Converters_JJO
{
    public class IntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (int.TryParse(value as string, out int result))
            {
                return result;
            }
            return 0; // or handle the error as needed
        }
    }
}
