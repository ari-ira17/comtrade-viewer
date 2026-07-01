using System.Collections.Generic;
using ComtradeViewer.Model.Models;

namespace ComtradeViewer.ViewModel.ViewModels
{
    public class ChannelPlotViewModel : ViewModelBase
    {
        public ChannelInfo Channel { get; }
        public List<SamplePoint> Points { get; }

        private SamplePoint? _selectedPoint;
        public SamplePoint? SelectedPoint
        {
            get => _selectedPoint;
            set
            {
                _selectedPoint = value;
                OnPropertyChanged();
            }
        }

        public ChannelPlotViewModel(ChannelInfo channel, List<SamplePoint> points)
        {
            Channel = channel;
            Points = points;
        }
    }
}
