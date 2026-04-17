using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Converters_JJO
{
    /// <summary>
    /// AUTEURS - Jerome JOSSENT / Karl TROUILLET
    /// DESCRIPTION -     *********************************************************
    /// Convert a boolean to a visibility action in *.xaml
    /// - If true: xaml element is Visible
    /// - If false: xaml element is Collapse
    /// IF PARAMETER is not NULL:
    /// - If true: xaml element is Collapse
    /// - If false: xaml element is Visible
    /// HISTORIQUE -      *********************************************************
    /// * V1 - 01/06/2021 -      ==================================================
    /// => add negation
    /// * V0 - 01/06/2021 -      ==================================================
    /// => Creation du document
    /// </summary>
    public class BoolToVisibilyConverter : IValueConverter
    {
        /// <summary>
        /// Convert boolean to visibility
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;
            boolValue = (parameter != null) ? !boolValue : boolValue;
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
