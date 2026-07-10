using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ComtradeViewer.View.Converters
{
    public class StringToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string colorName)
            {
                try
                {
                    return (Brush)new BrushConverter().ConvertFromString(colorName);
                }
                catch { }
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
