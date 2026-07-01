using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ComtradeViewer.Model.Models;
using ComtradeViewer.Model.Services;

namespace ComtradeViewer.ViewModel.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IComtradeParser _parser;
        private ObservableCollection<ChannelVisibilityItem> _allChannels;
        private ObservableCollection<ChannelPlotViewModel> _filteredChannelPlots;
        private string _statusText;
        private string _fileInfo;

        private double _timeMin;
        private double _timeMax;

        public MainViewModel() : this(new ComtradeParser()) { }

        public MainViewModel(IComtradeParser parser)
        {
            _parser = parser;
            OpenFileCommand = new RelayCommand(ExecuteOpenFile);
            ResetZoomCommand = new RelayCommand(_ => ResetZoom());
            SelectAllCommand = new RelayCommand(_ => SelectAll(true));
            DeselectAllCommand = new RelayCommand(_ => SelectAll(false));
            _allChannels = new ObservableCollection<ChannelVisibilityItem>();
            _filteredChannelPlots = new ObservableCollection<ChannelPlotViewModel>();
            StatusText = "Ожидание файла...";
            FileInfo = "Файл не загружен";
            TimeMin = 0;
            TimeMax = 1;
        }

        public ObservableCollection<ChannelVisibilityItem> AllChannels
        {
            get => _allChannels;
            set
            {
                _allChannels = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ChannelPlotViewModel> FilteredChannelPlots
        {
            get => _filteredChannelPlots;
            set
            {
                _filteredChannelPlots = value;
                OnPropertyChanged();
            }
        }

        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }

        public string FileInfo
        {
            get => _fileInfo;
            set
            {
                _fileInfo = value;
                OnPropertyChanged();
            }
        }

        public double TimeMin
        {
            get => _timeMin;
            set
            {
                if (_timeMin != value)
                {
                    _timeMin = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TimeRangeChanged));
                }
            }
        }

        public double TimeMax
        {
            get => _timeMax;
            set
            {
                if (_timeMax != value)
                {
                    _timeMax = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TimeRangeChanged));
                }
            }
        }

        public bool TimeRangeChanged { get; set; }

        public ICommand OpenFileCommand { get; }
        public ICommand ResetZoomCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand DeselectAllCommand { get; }

        private void ExecuteOpenFile(object parameter)
        {
            if (!(parameter is string[] paths) || paths.Length != 2)
                return;

            try
            {
                StatusText = "Загрузка...";
                var parseResult = _parser.Parse(paths[0], paths[1]);
                var data = parseResult.Data;
                var channels = parseResult.Channels;

                if (data == null || data.Count == 0)
                {
                    StatusText = "Нет данных";
                    FileInfo = "Файл пуст или неверный формат";
                    AllChannels.Clear();
                    FilteredChannelPlots.Clear();
                    return;
                }

                AllChannels.Clear();
                foreach (var ch in channels)
                {
                    if (data.TryGetValue(ch.Name, out var points))
                    {
                        var plotVm = new ChannelPlotViewModel(ch, points);
                        var item = new ChannelVisibilityItem(plotVm);
                        item.PropertyChanged += (s, e) =>
                        {
                            if (e.PropertyName == nameof(ChannelVisibilityItem.IsVisible))
                                UpdateFilteredChannels();
                        };
                        AllChannels.Add(item);
                    }
                }

                var allPoints = AllChannels.SelectMany(c => c.PlotViewModel.Points);
                if (allPoints.Any())
                {
                    TimeMin = allPoints.Min(p => p.Time);
                    TimeMax = allPoints.Max(p => p.Time);
                }
                else
                {
                    TimeMin = 0;
                    TimeMax = 1;
                }

                UpdateFilteredChannels();

                FileInfo = $"Файл: {System.IO.Path.GetFileName(paths[0])}, каналов: {AllChannels.Count}";
                StatusText = $"Загружено {AllChannels.Count} каналов";
            }
            catch (Exception ex)
            {
                StatusText = "Ошибка: " + ex.Message;
                FileInfo = "Ошибка загрузки";
                AllChannels.Clear();
                FilteredChannelPlots.Clear();
            }
        }

        private void UpdateFilteredChannels()
        {
            var visible = AllChannels
                .Where(item => item.IsVisible)
                .Select(item => item.PlotViewModel)
                .ToList();

            FilteredChannelPlots = new ObservableCollection<ChannelPlotViewModel>(visible);
            OnPropertyChanged(nameof(FilteredChannelPlots));
        }

        private void SelectAll(bool select)
        {
            foreach (var item in AllChannels)
                item.IsVisible = select;
        }

        private void ResetZoom()
        {
            if (AllChannels.Count == 0) return;
            var allPoints = AllChannels.SelectMany(c => c.PlotViewModel.Points);
            if (allPoints.Any())
            {
                TimeMin = allPoints.Min(p => p.Time);
                TimeMax = allPoints.Max(p => p.Time);
            }
        }

        public void SetTimeRange(double min, double max)
        {
            if (min < max && min >= 0 && max <= AllChannels.SelectMany(c => c.PlotViewModel.Points).Max(p => p.Time) + 0.1)
            {
                TimeMin = min;
                TimeMax = max;
            }
        }
    }
}
