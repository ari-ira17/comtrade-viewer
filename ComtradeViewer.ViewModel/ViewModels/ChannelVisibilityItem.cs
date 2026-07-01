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

        public ChannelVisibilityItem(ChannelPlotViewModel plot)
        {
            PlotViewModel = plot;
        }
    }
}
