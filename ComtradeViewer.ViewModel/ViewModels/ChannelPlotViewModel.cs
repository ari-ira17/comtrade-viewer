using System.Collections.Generic;
using ComtradeViewer.Model.Models;

namespace ComtradeViewer.ViewModel.ViewModels
{
    public class ChannelPlotViewModel : ViewModelBase
    {
        public ChannelInfo Channel { get; }
        public List<SamplePoint> Points { get; }

        public ChannelPlotViewModel(ChannelInfo channel, List<SamplePoint> points)
        {
            Channel = channel;
            Points = points;
        }
    }
}
