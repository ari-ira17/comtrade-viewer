using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using Newtonsoft.Json;
using ComtradeViewer.ViewModel.Converters;

namespace ComtradeViewer.ViewModel.Models
{
    public enum TimeFormat
    {
        MinutesSecondsMilliseconds,
        SecondsMilliseconds,
        Milliseconds
    }

    public class ChannelColorEntry : INotifyPropertyChanged
    {
        public string ChannelName { get; set; }

        [JsonConverter(typeof(MediaColorConverter))]
        public Color Color { get; set; } = Colors.Gray;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class AppSettings : INotifyPropertyChanged
    {
        public ObservableCollection<ChannelColorEntry> ChannelColors { get; set; }
            = new ObservableCollection<ChannelColorEntry>();

        public TimeFormat SelectedTimeFormat { get; set; }
            = TimeFormat.MinutesSecondsMilliseconds;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public Color GetChannelColor(string channelName)
        {
            foreach (var entry in ChannelColors)
                if (entry.ChannelName == channelName)
                    return entry.Color;
            return Colors.Gray;
        }
    }
}
