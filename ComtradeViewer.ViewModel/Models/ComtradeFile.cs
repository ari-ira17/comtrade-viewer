using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using ComtradeViewer.ViewModel.ViewModels;

namespace ComtradeViewer.ViewModel.Models
{
    public class ComtradeFile : INotifyPropertyChanged
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public ObservableCollection<ChannelPlotViewModel> Channels { get; set; }
            = new ObservableCollection<ChannelPlotViewModel>();

        private ObservableCollection<ChannelVisibilityItem> _allChannels;
        public ObservableCollection<ChannelVisibilityItem> AllChannels
        {
            get => _allChannels;
            set { _allChannels = value; OnPropertyChanged(); }
        }

        private ObservableCollection<ChannelPlotViewModel> _filteredChannelPlots;
        public ObservableCollection<ChannelPlotViewModel> FilteredChannelPlots
        {
            get => _filteredChannelPlots;
            set { _filteredChannelPlots = value; OnPropertyChanged(); }
        }

        private string _statusText;
        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        private string _fileInfo;
        public string FileInfo
        {
            get => _fileInfo;
            set { _fileInfo = value; OnPropertyChanged(); }
        }

        private double _timeMin;
        public double TimeMin
        {
            get => _timeMin;
            set { _timeMin = value; OnPropertyChanged(); }
        }

        private double _timeMax;
        public double TimeMax
        {
            get => _timeMax;
            set { _timeMax = value; OnPropertyChanged(); }
        }

        public bool TimeRangeChanged { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void UpdateFilteredChannels()
        {
            if (AllChannels == null)
            {
                FilteredChannelPlots = new ObservableCollection<ChannelPlotViewModel>();
                return;
            }
            var visible = AllChannels
                .Where(item => item.IsVisible)
                .Select(item => item.PlotViewModel)
                .ToList();
            FilteredChannelPlots = new ObservableCollection<ChannelPlotViewModel>(visible);
        }
    }
}
