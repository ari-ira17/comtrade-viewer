using System;
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

    public enum AppLanguage
    {
        Russian,
        English
    }

    public class ChannelColorEntry : INotifyPropertyChanged
    {
        private string _channelName;
        private Color _color = Colors.Gray;

        public string ChannelName
        {
            get => _channelName;
            set
            {
                if (_channelName != value)
                {
                    _channelName = value;
                    OnPropertyChanged(nameof(ChannelName));
                }
            }
        }

        [JsonConverter(typeof(MediaColorConverter))]
        public Color Color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    _color = value;
                    OnPropertyChanged(nameof(Color));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class AppSettings : INotifyPropertyChanged
    {
        private ObservableCollection<ChannelColorEntry> _channelColors = new ObservableCollection<ChannelColorEntry>();
        private TimeFormat _selectedTimeFormat = TimeFormat.MinutesSecondsMilliseconds;
        private AppLanguage _selectedLanguage = AppLanguage.Russian;

        public ObservableCollection<ChannelColorEntry> ChannelColors
        {
            get => _channelColors;
            set
            {
                _channelColors = value ?? new ObservableCollection<ChannelColorEntry>();
                OnPropertyChanged(nameof(ChannelColors));
            }
        }

        public TimeFormat SelectedTimeFormat
        {
            get => _selectedTimeFormat;
            set
            {
                if (_selectedTimeFormat != value)
                {
                    _selectedTimeFormat = value;
                    OnPropertyChanged(nameof(SelectedTimeFormat));
                }
            }
        }

        public AppLanguage SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (_selectedLanguage != value)
                {
                    _selectedLanguage = value;
                    OnPropertyChanged(nameof(SelectedLanguage));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public Color GetChannelColor(string channelName)
        {
            if (string.IsNullOrWhiteSpace(channelName) || ChannelColors == null)
                return Colors.Gray;

            foreach (var entry in ChannelColors)
            {
                if (entry != null && string.Equals(entry.ChannelName, channelName, StringComparison.OrdinalIgnoreCase))
                    return entry.Color;
            }

            return Colors.Gray;
        }
    }
}
