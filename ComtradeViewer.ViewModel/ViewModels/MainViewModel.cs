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
        private Dictionary<string, List<SamplePoint>> _parsedData;

        private string _selectedChannel;
        private List<SamplePoint> _chartPoints;
        private double _chartWidth = 1000;

        private readonly IComtradeParser _parser;

        public MainViewModel() : this(new ComtradeParser())
        {
        }

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
            set
            {
                if (_selectedChannel != value)
                {
                    _selectedChannel = value;
                    OnPropertyChanged();

                    UpdateChartPoints();
                }
            }
        }

        public List<SamplePoint> ChartPoints
        {
            get => _chartPoints;
            set
            {
                if (_chartPoints != value)
                {
                    _chartPoints = value;
                    OnPropertyChanged();
                }
            }
        }

        public double ChartWidth
        {
            get => _chartWidth;
            set
            {
                if (_chartWidth != value && value > 10)
                {
                    _chartWidth = value;
                    OnPropertyChanged();

                    UpdateChartPoints();
                }
            }
        }

        public ICommand OpenFileCommand { get; }

        private void ExecuteOpenFile(object parameter)
        {
            if (parameter is string[] paths && paths.Length == 2)
            {
                try
                {
                    string cfgPath = paths[0];
                    string datPath = paths[1];

                    _parsedData = _parser.Parse(cfgPath, datPath);

                    Channels.Clear();
                    if (_parsedData != null)
                    {
                        foreach (var channelName in _parsedData.Keys)
                        {
                            Channels.Add(channelName);
                        }
                    }

                    if (Channels.Count > 0)
                    {
                        SelectedChannel = Channels[0];
                    }
                    else
                    {
                        SelectedChannel = null;
                        ChartPoints = new List<SamplePoint>();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при открытии и парсинге файлов COMTRADE: {ex.Message}");
                }
            }
        }

        private void UpdateChartPoints()
        {
            if (string.IsNullOrEmpty(SelectedChannel) || _parsedData == null || !_parsedData.ContainsKey(SelectedChannel))
            {
                ChartPoints = new List<SamplePoint>();
                return;
            }

            try
            {
                List<SamplePoint> rawPoints = _parsedData[SelectedChannel];

                int targetPointsCount = (int)ChartWidth * 2;

                List<SamplePoint> optimizedPoints = DataDownsampler.MinMax(rawPoints, targetPointsCount);

                ChartPoints = optimizedPoints;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при оптимизации точек для графика: {ex.Message}");
                ChartPoints = new List<SamplePoint>();
            }
        }
    }
}
