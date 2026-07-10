using System.ComponentModel;
using System.Windows.Media;

namespace ComtradeViewer.ViewModel.Models
{
    public class SettingsChannelItem : INotifyPropertyChanged
    {
        public string ChannelName { get; set; }

        private bool _isVisible;
        public bool IsVisible
        {
            get => _isVisible;
            set { _isVisible = value; OnPropertyChanged(nameof(IsVisible)); }
        }

        private Color _selectedColor;
        public Color SelectedColor
        {
            get => _selectedColor;
            set { _selectedColor = value; OnPropertyChanged(nameof(SelectedColor)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
