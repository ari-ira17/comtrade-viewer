using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Media;  

namespace ComtradeViewer.ViewModel.ViewModels
{
    public class ChannelVisibilityItem : ViewModelBase
    {
        public ChannelPlotViewModel PlotViewModel { get; }
        private bool _isVisible = true;

        private static readonly string[] DefaultPalette = new[]
        {
            "#1F77B4", 
            "#FF7F0E", 
            "#2CA02C", 
            "#D62728", 
            "#9467BD", 
            "#8C564B", 
            "#E377C2", 
            "#17BECF", 
        };

        public static Color[] ColorPaletteColors { get; } = 
            DefaultPalette.Select(hex => (Color)ColorConverter.ConvertFromString(hex)).ToArray();

        private static int _colorIndex = -1;

        public static string[] ColorPalette => DefaultPalette;

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _selectedColor;
        public string SelectedColor
        {
            get => _selectedColor;
            set
            {
                if (_selectedColor != value)
                {
                    _selectedColor = value;
                    OnPropertyChanged();
                    if (PlotViewModel != null)
                        PlotViewModel.Color = value;
                }
            }
        }

        public ChannelVisibilityItem(ChannelPlotViewModel plot)
        {
            PlotViewModel = plot;

            if (!string.IsNullOrWhiteSpace(PlotViewModel?.Color) &&
                !PlotViewModel.Color.Equals("Gray", StringComparison.OrdinalIgnoreCase))
            {
                SelectedColor = PlotViewModel.Color;
            }
            else
            {
                int index = Interlocked.Increment(ref _colorIndex) % DefaultPalette.Length;
                SelectedColor = DefaultPalette[index];
                if (PlotViewModel != null)
                    PlotViewModel.Color = SelectedColor;
            }
        }
    }
}
