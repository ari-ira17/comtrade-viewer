#nullable disable

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;
using ComtradeViewer.Model.Models;
using ComtradeViewer.Model.Services;
using ComtradeViewer.ViewModel.Models;
using ComtradeViewer.ViewModel.Services;

namespace ComtradeViewer.ViewModel.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IComtradeParser _parser;
        private AppSettings _appSettings;

        private ObservableCollection<ComtradeFile> _openFiles;
        public ObservableCollection<ComtradeFile> OpenFiles
        {
            get => _openFiles;
            set { _openFiles = value; OnPropertyChanged(); }
        }

        private ComtradeFile _selectedFile;
        public ComtradeFile SelectedFile
        {
            get => _selectedFile;
            set
            {
                if (_selectedFile != value)
                {
                    _selectedFile = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(AllChannels));
                    OnPropertyChanged(nameof(FilteredChannelPlots));
                    OnPropertyChanged(nameof(TimeMin));
                    OnPropertyChanged(nameof(TimeMax));
                    OnPropertyChanged(nameof(ScrollMaximum));
                    OnPropertyChanged(nameof(ViewportSize));
                    OnPropertyChanged(nameof(ScrollValue));
                    OnPropertyChanged(nameof(FileInfo));
                    OnPropertyChanged(nameof(StatusText));
                    ClearRange();
                }
            }
        }

        public AppSettings AppSettings
        {
            get => _appSettings;
            set { _appSettings = value; OnPropertyChanged(); }
        }

        public void MoveFile(int oldIndex, int newIndex)
        {
            if (oldIndex < 0 || oldIndex >= OpenFiles.Count ||
                newIndex < 0 || newIndex >= OpenFiles.Count)
                return;
            OpenFiles.Move(oldIndex, newIndex);
            SelectedFile = OpenFiles[newIndex];
        }

        private ObservableCollection<SettingsChannelItem> _settingsChannels;
        public ObservableCollection<SettingsChannelItem> SettingsChannels
        {
            get => _settingsChannels;
            set { _settingsChannels = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ChannelVisibilityItem> AllChannels
        {
            get => _selectedFile?.AllChannels;
            set
            {
                if (_selectedFile != null)
                {
                    _selectedFile.AllChannels = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ScrollMaximum));
                    OnPropertyChanged(nameof(ViewportSize));
                    OnPropertyChanged(nameof(ScrollValue));
                }
            }
        }

        public ObservableCollection<ChannelPlotViewModel> FilteredChannelPlots
        {
            get => _selectedFile?.FilteredChannelPlots;
            set
            {
                if (_selectedFile != null)
                {
                    _selectedFile.FilteredChannelPlots = value;
                    OnPropertyChanged();
                }
            }
        }

        public string StatusText
        {
            get => _selectedFile?.StatusText ?? "Ожидание файла...";
            set
            {
                if (_selectedFile != null)
                {
                    _selectedFile.StatusText = value;
                    OnPropertyChanged();
                }
            }
        }

        public string FileInfo
        {
            get => _selectedFile?.FileInfo ?? "Файл не загружен";
            set
            {
                if (_selectedFile != null)
                {
                    _selectedFile.FileInfo = value;
                    OnPropertyChanged();
                }
            }
        }

        public double TimeMin
        {
            get => _selectedFile?.TimeMin ?? 0;
            set
            {
                if (_selectedFile != null && Math.Abs(_selectedFile.TimeMin - value) > 0.000001)
                {
                    _selectedFile.TimeMin = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TimeRangeChanged));
                    OnPropertyChanged(nameof(ScrollValue));
                    OnPropertyChanged(nameof(ViewportSize));
                }
            }
        }

        public double TimeMax
        {
            get => _selectedFile?.TimeMax ?? 1;
            set
            {
                if (_selectedFile != null && Math.Abs(_selectedFile.TimeMax - value) > 0.000001)
                {
                    _selectedFile.TimeMax = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TimeRangeChanged));
                    OnPropertyChanged(nameof(ViewportSize));
                }
            }
        }

        public bool TimeRangeChanged
        {
            get => _selectedFile?.TimeRangeChanged ?? false;
            set
            {
                if (_selectedFile != null)
                {
                    _selectedFile.TimeRangeChanged = value;
                    OnPropertyChanged();
                }
            }
        }

        public double ScrollMinimum => 0;
        public double ScrollMaximum
        {
            get
            {
                if (_selectedFile?.AllChannels == null || !_selectedFile.AllChannels.Any()) return 1;
                return _selectedFile.AllChannels.SelectMany(c => c.PlotViewModel.Points).Max(p => p.Time);
            }
        }

        public double ScrollValue
        {
            get => TimeMin;
            set
            {
                if (_selectedFile == null) return;
                double range = TimeMax - TimeMin;
                double newMin = Math.Max(ScrollMinimum, Math.Min(value, ScrollMaximum - range));
                if (newMin + range > ScrollMaximum)
                    newMin = ScrollMaximum - range;
                if (Math.Abs(TimeMin - newMin) > 0.000001)
                {
                    TimeMin = newMin;
                    TimeMax = newMin + range;
                    OnPropertyChanged(nameof(ScrollValue));
                }
            }
        }

        public double ViewportSize => TimeMax - TimeMin;

        private double? _rangeLeft;
        private double? _rangeRight;

        public double? RangeLeft
        {
            get => _rangeLeft;
            set
            {
                if (_rangeLeft != value)
                {
                    _rangeLeft = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(RangeLeftChanged));
                    OnPropertyChanged(nameof(RangeInfoText));
                    OnPropertyChanged(nameof(HasRangeInfo));
                }
            }
        }
        public bool RangeLeftChanged { get; set; }

        public double? RangeRight
        {
            get => _rangeRight;
            set
            {
                if (_rangeRight != value)
                {
                    _rangeRight = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(RangeRightChanged));
                    OnPropertyChanged(nameof(RangeInfoText));
                    OnPropertyChanged(nameof(HasRangeInfo));
                }
            }
        }
        public bool RangeRightChanged { get; set; }

        public string RangeInfoText
        {
            get
            {
                if (!_rangeLeft.HasValue || !_rangeRight.HasValue)
                    return string.Empty;
                double diffMs = (_rangeRight.Value - _rangeLeft.Value) * 1000.0;
                return $"L: {_rangeLeft.Value:F3} с\nR: {_rangeRight.Value:F3} с\nΔ = {diffMs:F3} мс";
            }
        }

        public bool HasRangeInfo => _rangeLeft.HasValue && _rangeRight.HasValue;

        private double? _hoverTime;
        public double? HoverTime
        {
            get => _hoverTime;
            set
            {
                if (_hoverTime != value)
                {
                    _hoverTime = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HoverTimeChanged));
                }
            }
        }
        public bool HoverTimeChanged { get; set; }

        public ICommand OpenFileCommand { get; }
        public ICommand CloseFileCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand ResetZoomCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand DeselectAllCommand { get; }
        public ICommand ClearRangeCommand { get; }

        public Action OpenSettingsAction { get; set; }

        public MainViewModel() : this(new ComtradeParser()) { }

        public MainViewModel(IComtradeParser parser)
        {
            _parser = parser;
            AppSettings = SettingsService.Load();

            OpenFiles = new ObservableCollection<ComtradeFile>();
            SettingsChannels = new ObservableCollection<SettingsChannelItem>();

            OpenFileCommand = new RelayCommand(ExecuteOpenFile);
            CloseFileCommand = new RelayCommand(ExecuteCloseFile);
            OpenSettingsCommand = new RelayCommand(ExecuteOpenSettings);
            ResetZoomCommand = new RelayCommand(ExecuteResetZoom);
            SelectAllCommand = new RelayCommand(ExecuteSelectAll);
            DeselectAllCommand = new RelayCommand(ExecuteDeselectAll);
            ClearRangeCommand = new RelayCommand(ExecuteClearRange);
        }

        private void ExecuteOpenFile(object parameter)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "COMTRADE CFG files (*.cfg)|*.cfg|All files (*.*)|*.*",
                Multiselect = true
            };
            if (dlg.ShowDialog() == true)
            {
                foreach (string cfgPath in dlg.FileNames)
                {
                    try
                    {
                        string datPath = System.IO.Path.ChangeExtension(cfgPath, ".dat");
                        if (!System.IO.File.Exists(datPath))
                        {
                            MessageBox.Show($"Не найден DAT-файл для {cfgPath}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            continue;
                        }

                        var parseResult = _parser.Parse(cfgPath, datPath);
                        var file = new ComtradeFile
                        {
                            FileName = System.IO.Path.GetFileName(cfgPath),
                            FilePath = cfgPath,
                            StatusText = "Загрузка...",
                            FileInfo = ""
                        };

                        file.AllChannels = new ObservableCollection<ChannelVisibilityItem>();
                        int channelIndex = 0;
                        string[] palette = ChannelVisibilityItem.ColorPalette;

                        foreach (var ch in parseResult.Channels)
                        {
                            if (parseResult.Data.TryGetValue(ch.Name, out var points))
                            {
                                var plotVm = new ChannelPlotViewModel(ch, points);

                                Color savedColor = AppSettings.GetChannelColor(ch.Name);
                                bool hasSavedColor = AppSettings.ChannelColors.Any(c => c.ChannelName == ch.Name && c.Color != Colors.Gray);

                                if (hasSavedColor)
                                {
                                    plotVm.Color = savedColor.ToString();
                                }
                                else
                                {
                                    string defaultColorHex = palette[channelIndex % palette.Length];
                                    plotVm.Color = defaultColorHex;
                                }
                                plotVm.TimeFormat = AppSettings.SelectedTimeFormat;

                                var item = new ChannelVisibilityItem(plotVm)
                                {
                                    IsVisible = true
                                };
                                item.PropertyChanged += (s, e) =>
                                {
                                    if (e.PropertyName == nameof(ChannelVisibilityItem.IsVisible))
                                        file.UpdateFilteredChannels();
                                };
                                file.AllChannels.Add(item);
                                channelIndex++;
                            }
                        }

                        var allPoints = file.AllChannels.SelectMany(c => c.PlotViewModel.Points);
                        if (allPoints.Any())
                        {
                            file.TimeMin = allPoints.Min(p => p.Time);
                            file.TimeMax = allPoints.Max(p => p.Time);
                        }
                        else
                        {
                            file.TimeMin = 0;
                            file.TimeMax = 1;
                        }

                        file.UpdateFilteredChannels();
                        file.StatusText = $"Загружено {file.AllChannels.Count} каналов";
                        file.FileInfo = $"Файл: {file.FileName}, каналов: {file.AllChannels.Count}";

                        OpenFiles.Add(file);
                        SelectedFile = file;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка загрузки {cfgPath}: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ExecuteCloseFile(object parameter)
        {
            if (parameter is ComtradeFile file)
                OpenFiles.Remove(file);
        }

        public void RefreshSettingsChannels()
        {
            SettingsChannels.Clear();
            if (SelectedFile?.AllChannels == null) return;

            foreach (var chItem in SelectedFile.AllChannels)
            {
                string colorStr = chItem.PlotViewModel.Color;
                Color currentColor;
                try
                {
                    currentColor = (Color)ColorConverter.ConvertFromString(colorStr);
                }
                catch
                {
                    currentColor = Colors.Gray;
                }

                var settingsItem = new SettingsChannelItem
                {
                    ChannelName = chItem.PlotViewModel.Channel.Name,
                    IsVisible = chItem.IsVisible,
                    SelectedColor = currentColor
                };

                settingsItem.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == "IsVisible")
                    {
                        chItem.IsVisible = settingsItem.IsVisible;
                    }
                    if (e.PropertyName == "SelectedColor")
                    {
                        chItem.PlotViewModel.Color = settingsItem.SelectedColor.ToString();
                        var entry = AppSettings.ChannelColors.FirstOrDefault(c => c.ChannelName == settingsItem.ChannelName);
                        if (entry != null)
                            entry.Color = settingsItem.SelectedColor;
                        else
                            AppSettings.ChannelColors.Add(new ChannelColorEntry
                            {
                                ChannelName = settingsItem.ChannelName,
                                Color = settingsItem.SelectedColor
                            });
                    }
                };

                SettingsChannels.Add(settingsItem);
            }
        }

        private void ExecuteOpenSettings(object parameter)
        {
            RefreshSettingsChannels();
            OpenSettingsAction?.Invoke();
        }

        public void ApplySettings()
        {
            foreach (var file in OpenFiles)
            {
                foreach (var chItem in file.AllChannels)
                {
                    chItem.PlotViewModel.TimeFormat = AppSettings.SelectedTimeFormat;
                }
            }
            SettingsService.Save(AppSettings);
            OnPropertyChanged(nameof(AllChannels));
            OnPropertyChanged(nameof(FilteredChannelPlots));
        }

        private void ExecuteResetZoom(object parameter) { ResetZoom(); }

        private void ResetZoom()
        {
            if (_selectedFile?.AllChannels == null || !_selectedFile.AllChannels.Any()) return;
            var allPoints = _selectedFile.AllChannels.SelectMany(c => c.PlotViewModel.Points);
            if (allPoints.Any())
            {
                double min = allPoints.Min(p => p.Time);
                double max = allPoints.Max(p => p.Time);
                TimeMin = min;
                TimeMax = max;
                OnPropertyChanged(nameof(ScrollValue));
                OnPropertyChanged(nameof(ViewportSize));
            }
        }

        public void SetTimeRange(double min, double max)
        {
            if (_selectedFile == null) return;
            if (min < max && min >= 0 && max <= _selectedFile.AllChannels.SelectMany(c => c.PlotViewModel.Points).Max(p => p.Time) + 0.1)
            {
                TimeMin = min;
                TimeMax = max;
                OnPropertyChanged(nameof(ScrollValue));
                OnPropertyChanged(nameof(ViewportSize));
            }
        }

        private void ExecuteClearRange(object parameter) { ClearRange(); }
        private void ClearRange()
        {
            RangeLeft = null;
            RangeRight = null;
        }

        private void ExecuteSelectAll(object parameter) { SelectAll(true); }
        private void ExecuteDeselectAll(object parameter) { SelectAll(false); }
        private void SelectAll(bool select)
        {
            if (_selectedFile?.AllChannels == null) return;
            foreach (var item in _selectedFile.AllChannels)
                item.IsVisible = select;
        }
    }
}
