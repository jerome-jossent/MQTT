using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Converters_JJO
{
    /// <summary>
    /// AUTEUR - Karl TROUILLET
    /// DESCRIPTION -     *********************************************************
    /// Used by view to convert null value to visible
    /// HISTORIQUE -      *********************************************************
    /// * V0 - 01/06/2021 -      ==================================================
    /// => Creation du document
    /// </summary>
   public class NullToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// when value is null, component is visible, hidden otherwise
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType">[NOT USED]</param>
        /// <param name="parameter">[NOT USED]</param>
        /// <param name="culture">[NOT USED]</param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>
        /// NOT USED
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
