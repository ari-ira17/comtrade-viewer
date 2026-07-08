using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using ComtradeViewer.ViewModel.ViewModels;

namespace ComtradeViewer.View.Views
{
    public partial class SettingsWindow : Window
    {
        public static IValueConverter ColorToBrushConverter { get; } = new ColorToBrushConverter();
        public static IValueConverter TimeFormatConverter { get; } = new TimeFormatToStringConverter();

        public SettingsWindow(MainViewModel mainViewModel)
        {
            InitializeComponent();
            this.DataContext = mainViewModel;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }

    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Color color)
                return new SolidColorBrush(color);
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TimeFormatToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is ComtradeViewer.ViewModel.Models.TimeFormat fmt)
            {
                switch (fmt)
                {
                    case ComtradeViewer.ViewModel.Models.TimeFormat.MinutesSecondsMilliseconds: return "мм:сс.мс";
                    case ComtradeViewer.ViewModel.Models.TimeFormat.SecondsMilliseconds: return "сс.мс";
                    case ComtradeViewer.ViewModel.Models.TimeFormat.Milliseconds: return "мс";
                }
            }
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
