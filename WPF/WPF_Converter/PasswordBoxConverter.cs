using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace Converters_JJO
{
    // https://stackoverflow.com/questions/1483892/how-to-bind-to-a-passwordbox-in-mvvm
    /// <summary>
    /// AUTEUR - Karl TROUILLET
    /// DESCRIPTION -     *********************************************************
    /// This file is used to get password from view, and to check it in ViewModel
    /// HISTORIQUE -      *********************************************************
    /// * V0 - 01/06/2021 -      ==================================================
    /// => Creation du document
    /// </summary>
    public interface IWrappedParameter<T>
    {
        T Value { get; }
    }

    /// <summary>
    /// process element which is get from view, to extract password
    /// </summary>
    public class PasswordBoxWrapper : IWrappedParameter<string>
    {
        private readonly PasswordBox _source;

        public string Value
        {
            get { return _source != null ? _source.Password : string.Empty; }
        }

        public PasswordBoxWrapper(PasswordBox source)
        {
            _source = source;
        }
    }

    /// <summary>
    /// Converter used in view to extract element PasswordBox
    /// </summary>
    public class PasswordBoxConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Implement type and value check here...
            return new PasswordBoxWrapper((PasswordBox)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("No conversion.");
        }
    }
}
