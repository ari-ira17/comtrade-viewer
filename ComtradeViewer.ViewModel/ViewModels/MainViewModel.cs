using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ComtradeViewer.Model.Models;
using ComtradeViewer.Model.Services;

namespace ComtradeViewer.ViewModel.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IComtradeParser _parser;
        private Dictionary<string, List<SamplePoint>> _parsedData;
        private string _selectedChannel;
        private List<SamplePoint> _chartPoints;
        private double _chartWidth = 1000;
        private string _statusText = "Готов";
        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        private string _fileInfo = "Файл не загружен";
        public string FileInfo
        {
            get => _fileInfo;
            set { _fileInfo = value; OnPropertyChanged(); }
        }

        public MainViewModel() : this(new ComtradeParser()) { }

        public MainViewModel(IComtradeParser parser)
        {
            _parser = parser;
            Channels = new ObservableCollection<string>();
            OpenFileCommand = new RelayCommand(ExecuteOpenFile);
            _chartPoints = new List<SamplePoint>();
        }

        public ObservableCollection<string> Channels { get; private set; }

        public string SelectedChannel
        {
            get => _selectedChannel;
            set { if (_selectedChannel != value) { _selectedChannel = value; OnPropertyChanged(); UpdateChartPoints(); } }
        }

        public List<SamplePoint> ChartPoints
        {
            get => _chartPoints;
            set { if (_chartPoints != value) { _chartPoints = value; OnPropertyChanged(); } }
        }

        public double ChartWidth
        {
            get => _chartWidth;
            set { if (_chartWidth != value && value > 10) { _chartWidth = value; OnPropertyChanged(); UpdateChartPoints(); } }
        }

        public ICommand OpenFileCommand { get; }

        private void ExecuteOpenFile(object parameter)
        {
            if (parameter is string[] paths && paths.Length == 2)
            {
                try
                {
                    StatusText = "Загрузка...";
                    _parsedData = _parser.Parse(paths[0], paths[1]);
                    Channels.Clear();
                    if (_parsedData != null && _parsedData.Count > 0)
                    {
                        foreach (var name in _parsedData.Keys)
                            Channels.Add(name);
                        FileInfo = $"Файл: {System.IO.Path.GetFileName(paths[0])}, каналов: {Channels.Count}";
                        StatusText = $"Загружено {Channels.Count} каналов";
                    }
                    else
                    {
                        StatusText = "Нет данных";
                        FileInfo = "Файл пуст или неверный формат";
                    }
                    SelectedChannel = Channels.Count > 0 ? Channels[0] : null;
                }
                catch (Exception ex)
                {
                    StatusText = "Ошибка: " + ex.Message;
                    FileInfo = "Ошибка загрузки";
                    _parsedData = null;
                    Channels.Clear();
                    SelectedChannel = null;
                }
            }
        }

        private void UpdateChartPoints()
        {
            if (string.IsNullOrEmpty(SelectedChannel) || _parsedData == null)
            {
                ChartPoints = new List<SamplePoint>();
                return;
            }
            ChartPoints = DataDownsampler.MinMax(_parsedData[SelectedChannel], (int)ChartWidth * 2);
        }
    }
}
