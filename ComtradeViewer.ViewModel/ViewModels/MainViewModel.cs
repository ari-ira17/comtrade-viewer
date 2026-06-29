using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ComtradeViewer.Model.Models;
using ComtradeViewer.Model.Services;

namespace ComtradeViewer.ViewModel.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IComtradeParser _parser;
        private ObservableCollection<ChannelPlotViewModel> _channelPlots;
        private string _statusText;
        private string _fileInfo;

        public MainViewModel() : this(new ComtradeParser()) { }

        public MainViewModel(IComtradeParser parser)
        {
            _parser = parser;
            OpenFileCommand = new RelayCommand(ExecuteOpenFile);
            _channelPlots = new ObservableCollection<ChannelPlotViewModel>();
            StatusText = "Ожидание файла...";
            FileInfo = "Файл не загружен";
        }

        public ObservableCollection<ChannelPlotViewModel> ChannelPlots
        {
            get => _channelPlots;
            set
            {
                _channelPlots = value;
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

        public ICommand OpenFileCommand { get; }

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
                    ChannelPlots.Clear();
                    return;
                }

                ChannelPlots.Clear();
                foreach (var ch in channels)
                {
                    if (data.TryGetValue(ch.Name, out var points))
                    {
                        ChannelPlots.Add(new ChannelPlotViewModel(ch, points));
                    }
                }

                FileInfo = $"Файл: {System.IO.Path.GetFileName(paths[0])}, каналов: {ChannelPlots.Count}";
                StatusText = $"Загружено {ChannelPlots.Count} каналов";
            }
            catch (Exception ex)
            {
                StatusText = "Ошибка: " + ex.Message;
                FileInfo = "Ошибка загрузки";
                ChannelPlots.Clear();
            }
        }
    }
}
