using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MQTT_Messages_Generator
{
    public class ChainedConverter : IValueConverter
    {
        private readonly IValueConverter _converter1;
        private readonly IValueConverter _converter2;

        public ChainedConverter(IValueConverter converter1, IValueConverter converter2)
        {
            _converter1 = converter1;
            _converter2 = converter2;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var result = _converter1.Convert(value, targetType, parameter, culture);
            return _converter2.Convert(result, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var result = _converter2.ConvertBack(value, targetType, parameter, culture);
            return _converter1.ConvertBack(result, targetType, parameter, culture);
        }
    }
}
