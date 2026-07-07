using System.Collections.Generic;
using ComtradeViewer.Model.Models;
using ComtradeViewer.ViewModel.Models;  

namespace ComtradeViewer.ViewModel.ViewModels
{
    public class ChannelPlotViewModel : ViewModelBase
    {
        public ChannelInfo Channel { get; }
        public List<SamplePoint> Points { get; }

        private TimeFormat _timeFormat = TimeFormat.MinutesSecondsMilliseconds;
        public TimeFormat TimeFormat
        {
            get => _timeFormat;
            set
            {
                if (_timeFormat != value)
                {
                    _timeFormat = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _color = "Gray";
        public string Color
        {
            get => _color;
            set
            {
                _color = value;
                OnPropertyChanged();
            }
        }

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
