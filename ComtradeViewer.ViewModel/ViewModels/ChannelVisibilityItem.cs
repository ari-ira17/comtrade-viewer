using System;
using System.ComponentModel;

namespace ComtradeViewer.ViewModel.ViewModels
{
    public class ChannelVisibilityItem : ViewModelBase
    {
        public ChannelPlotViewModel PlotViewModel { get; }
        private bool _isVisible = true;

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
                    // Обновляем цвет в PlotViewModel
                    if (PlotViewModel != null)
                        PlotViewModel.Color = value;
                }
            }
        }

        public ChannelVisibilityItem(ChannelPlotViewModel plot)
        {
            PlotViewModel = plot;
            // Назначаем случайный цвет из палитры при создании
            var colors = new[]
            {
                "SteelBlue", "DarkRed", "DarkGreen", "DarkOrange",
                "Purple", "Teal", "Crimson", "Olive",
                "Navy", "Maroon", "DarkSlateBlue", "Sienna"
            };
            var rnd = new Random();
            SelectedColor = colors[rnd.Next(colors.Length)];
        }
    }
}
