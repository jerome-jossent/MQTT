using System;
using System.Windows.Markup;
using System.Windows.Data;
using System.Windows;

namespace MQTT_Messages_Generator
{
    public class NotExtension : MarkupExtension
    {
        public NotExtension() { }

        public NotExtension(object value)
        {
            Value = value;
        }

        [ConstructorArgument("value")]
        public object Value { get; set; }

        public IValueConverter Converter { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var binding = Value as Binding ?? new Binding();

            var inverseBooleanConverter = new InverseBooleanConverter();

            if (Converter != null)
            {
                binding.Converter = new ChainedConverter(inverseBooleanConverter, Converter);
            }
            else
            {
                binding.Converter = inverseBooleanConverter;
            }

            return binding.ProvideValue(serviceProvider);
        }
    }

    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return value;
        }
    }
}