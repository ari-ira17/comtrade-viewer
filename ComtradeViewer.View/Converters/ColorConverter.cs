using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ComtradeViewer.View.Converters
{
    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string colorName && !string.IsNullOrEmpty(colorName))
            {
                try
                {
                    var color = (Color)System.Windows.Media.ColorConverter.ConvertFromString(colorName);
                    return new SolidColorBrush(color);
                }
                catch
                {

                }
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
