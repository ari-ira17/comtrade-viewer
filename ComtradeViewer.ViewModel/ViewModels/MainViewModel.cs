#nullable disable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ComtradeViewer.Model.Models;
using ComtradeViewer.Model.Services;
using ComtradeViewer.ViewModel.Models;
using ComtradeViewer.ViewModel.Resources;
using ComtradeViewer.ViewModel.Services;

namespace ComtradeViewer.ViewModel.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Fields

        private const double MaximumTimeRangeTolerance = 0.1;
        private readonly IComtradeParser _parser;
        private AppSettings _appSettings;
        private ObservableCollection<ComtradeFile> _openFiles;
        private ComtradeFile _selectedFile;
        private ObservableCollection<SettingsChannelItem> _settingsChannels;
        private double? _rangeLeft;
        private double? _rangeRight;
        private double? _hoverTime;

        #endregion

        #region Properties

        public ObservableCollection<ComtradeFile> OpenFiles
        {
            get => _openFiles;
            set { _openFiles = value; OnPropertyChanged(); }
        }

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
            set
            {
                if (_appSettings != null)
                    _appSettings.PropertyChanged -= AppSettings_PropertyChanged;

                _appSettings = value;
                if (_appSettings != null)
                    _appSettings.PropertyChanged += AppSettings_PropertyChanged;

                OnPropertyChanged();
                ApplyCurrentLanguage();
            }
        }

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
            get => _selectedFile?.StatusText ?? AppResources.Get("WaitingForFile");
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
            get => _selectedFile?.FileInfo ?? AppResources.Get("FileNotLoaded");
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
                if (_selectedFile?.AllChannels == null || !_selectedFile.AllChannels.Any())
                    return 1;

                return _selectedFile.AllChannels.SelectMany(c => c.PlotViewModel.Points).Max(p => p.Time);
            }
        }

        public double ScrollValue
        {
            get => TimeMin;
            set
            {
                if (_selectedFile == null)
                    return;

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
                return string.Format(AppResources.Get("RangeInfoFormat"), _rangeLeft.Value, _rangeRight.Value, diffMs);
            }
        }

        public bool HasRangeInfo => _rangeLeft.HasValue && _rangeRight.HasValue;

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

        public string MainWindowTitle => AppResources.Get("MainWindowTitle");
        public string OpenCfgButtonText => AppResources.Get("OpenCfgButton");
        public string SettingsButtonText => AppResources.Get("SettingsButton");
        public string ResetZoomButtonText => AppResources.Get("ResetZoomButton");
        public string ClearRangeButtonText => AppResources.Get("ClearRangeButton");
        public string SettingsWindowTitleText => AppResources.Get("SettingsWindowTitle");
        public string TimeFormatLabelText => AppResources.Get("TimeFormatLabel");
        public string LanguageLabelText => AppResources.Get("LanguageLabel");
        public string OkButtonText => AppResources.Get("OkButton");
        public string CancelButtonText => AppResources.Get("CancelButton");
        public string RussianOptionText => AppResources.Get("RussianOption");
        public string EnglishOptionText => AppResources.Get("EnglishOption");

        public ICommand OpenFileCommand { get; }
        public ICommand CloseFileCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand ResetZoomCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand DeselectAllCommand { get; }
        public ICommand ClearRangeCommand { get; }

        public Action OpenSettingsAction { get; set; }

        #endregion

        #region Constructors

        public MainViewModel() : this(new ComtradeParser())
        {
        }

        public MainViewModel(IComtradeParser parser)
        {
            _parser = parser;
            AppSettings = SettingsService.Load();
            if (AppSettings.SelectedLanguage == default(AppLanguage))
                AppSettings.SelectedLanguage = AppLanguage.Russian;
            AppResources.SetCulture(GetCultureInfo(AppSettings.SelectedLanguage));

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

        #endregion

        #region Public API

        public void MoveFile(int oldIndex, int newIndex)
        {
            if (oldIndex < 0 || oldIndex >= OpenFiles.Count ||
                newIndex < 0 || newIndex >= OpenFiles.Count)
                return;

            OpenFiles.Move(oldIndex, newIndex);
            SelectedFile = OpenFiles[newIndex];
        }

        public void RefreshSettingsChannels()
        {
            SettingsChannels.Clear();
            if (SelectedFile?.AllChannels == null)
                return;

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

                settingsItem.PropertyChanged += SettingsChannel_PropertyChanged;
                SettingsChannels.Add(settingsItem);
            }
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
            ApplyCurrentLanguage();
            OnPropertyChanged(nameof(AllChannels));
            OnPropertyChanged(nameof(FilteredChannelPlots));
        }

        public void ResetZoom()
        {
            if (_selectedFile?.AllChannels == null || !_selectedFile.AllChannels.Any())
                return;

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
            if (_selectedFile == null)
                return;

            var points = _selectedFile.AllChannels.SelectMany(c => c.PlotViewModel.Points);
            if (!points.Any())
                return;

            double maxTime = points.Max(p => p.Time);
            if (min < max && min >= 0 && max <= maxTime + MaximumTimeRangeTolerance)
            {
                TimeMin = min;
                TimeMax = max;
                OnPropertyChanged(nameof(ScrollValue));
                OnPropertyChanged(nameof(ViewportSize));
            }
        }

        #endregion

        #region Command Handlers

        private void ExecuteOpenFile(object parameter)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = AppResources.Get("OpenFileFilter"),
                Multiselect = true
            };

            if (dlg.ShowDialog() != true)
                return;

            foreach (string cfgPath in dlg.FileNames)
            {
                try
                {
                    if (!TryLoadFile(cfgPath))
                        continue;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(AppResources.Get("LoadError"), cfgPath, ex.Message), AppResources.Get("ErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExecuteCloseFile(object parameter)
        {
            if (parameter is ComtradeFile file)
                OpenFiles.Remove(file);
        }

        private void ExecuteOpenSettings(object parameter)
        {
            RefreshSettingsChannels();
            OpenSettingsAction?.Invoke();
        }

        private void ExecuteResetZoom(object parameter) { ResetZoom(); }

        private void ExecuteClearRange(object parameter) { ClearRange(); }

        private void ExecuteSelectAll(object parameter) { SelectAll(true); }

        private void ExecuteDeselectAll(object parameter) { SelectAll(false); }

        #endregion

        #region Private Helpers

        private bool TryLoadFile(string cfgPath)
        {
            string datPath = System.IO.Path.ChangeExtension(cfgPath, ".dat");
            if (!System.IO.File.Exists(datPath))
            {
                MessageBox.Show(string.Format(AppResources.Get("MissingDatFile"), cfgPath), AppResources.Get("WarningTitle"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            var parseResult = _parser.Parse(cfgPath, datPath);
            var file = CreateComtradeFile(cfgPath, parseResult);
            if (file == null)
                return false;

            OpenFiles.Add(file);
            SelectedFile = file;
            return true;
        }

        private ComtradeFile CreateComtradeFile(string cfgPath, ComtradeParseResult parseResult)
        {
            var file = new ComtradeFile
            {
                FileName = System.IO.Path.GetFileName(cfgPath),
                FilePath = cfgPath,
                StatusText = AppResources.Get("LoadingStatus"),
                FileInfo = string.Empty
            };

            file.AllChannels = new ObservableCollection<ChannelVisibilityItem>();
            int channelIndex = 0;
            string[] palette = ChannelVisibilityItem.ColorPalette;

            foreach (var ch in parseResult.Channels)
            {
                if (!parseResult.Data.TryGetValue(ch.Name, out var points))
                    continue;

                var plotVm = new ChannelPlotViewModel(ch, points);
                ApplyChannelColor(plotVm, ch.Name, channelIndex, palette);
                plotVm.TimeFormat = AppSettings.SelectedTimeFormat;

                var item = CreateChannelVisibilityItem(plotVm, file);
                file.AllChannels.Add(item);
                channelIndex++;
            }

            InitializeFileTimeRange(file);
            file.UpdateFilteredChannels();
            file.StatusText = string.Format(AppResources.Get("LoadedChannelsStatus"), file.AllChannels.Count);
            file.FileInfo = string.Format(AppResources.Get("FileInfoFormat"), file.FileName, file.AllChannels.Count);
            return file;
        }

        private void ApplyChannelColor(ChannelPlotViewModel plotVm, string channelName, int channelIndex, IList<string> palette)
        {
            if (AppSettings == null)
                return;

            Color savedColor = AppSettings.GetChannelColor(channelName);
            bool hasSavedColor = AppSettings.ChannelColors != null &&
                                 AppSettings.ChannelColors.Any(c => c != null && c.ChannelName == channelName && c.Color != Colors.Gray);

            if (hasSavedColor)
            {
                plotVm.Color = savedColor.ToString();
            }
            else
            {
                string defaultColorHex = palette[channelIndex % palette.Count];
                plotVm.Color = defaultColorHex;
            }
        }

        private ChannelVisibilityItem CreateChannelVisibilityItem(ChannelPlotViewModel plotVm, ComtradeFile file)
        {
            var item = new ChannelVisibilityItem(plotVm)
            {
                IsVisible = true
            };

            item.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ChannelVisibilityItem.IsVisible))
                    file.UpdateFilteredChannels();
            };

            return item;
        }

        private void InitializeFileTimeRange(ComtradeFile file)
        {
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
        }

        private void SettingsChannel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!(sender is SettingsChannelItem settingsItem))
                return;

            if (e.PropertyName == "IsVisible")
            {
                var chItem = SelectedFile?.AllChannels.FirstOrDefault(c => c.PlotViewModel.Channel.Name == settingsItem.ChannelName);
                if (chItem != null)
                    chItem.IsVisible = settingsItem.IsVisible;
            }

            if (e.PropertyName == "SelectedColor")
            {
                var chItem = SelectedFile?.AllChannels.FirstOrDefault(c => c.PlotViewModel.Channel.Name == settingsItem.ChannelName);
                if (chItem != null)
                    chItem.PlotViewModel.Color = settingsItem.SelectedColor.ToString();

                if (AppSettings.ChannelColors == null)
                    AppSettings.ChannelColors = new ObservableCollection<ChannelColorEntry>();

                var entry = AppSettings.ChannelColors.FirstOrDefault(c => c != null && c.ChannelName == settingsItem.ChannelName);
                if (entry != null)
                    entry.Color = settingsItem.SelectedColor;
                else
                    AppSettings.ChannelColors.Add(new ChannelColorEntry
                    {
                        ChannelName = settingsItem.ChannelName,
                        Color = settingsItem.SelectedColor
                    });
            }
        }

        private void ClearRange()
        {
            RangeLeft = null;
            RangeRight = null;
        }

        private void SelectAll(bool select)
        {
            if (_selectedFile?.AllChannels == null)
                return;

            foreach (var item in _selectedFile.AllChannels)
                item.IsVisible = select;
        }

        private void ApplyCurrentLanguage()
        {
            if (_appSettings == null)
                return;

            AppResources.SetCulture(GetCultureInfo(_appSettings.SelectedLanguage));
            OnPropertyChanged(nameof(MainWindowTitle));
            OnPropertyChanged(nameof(OpenCfgButtonText));
            OnPropertyChanged(nameof(SettingsButtonText));
            OnPropertyChanged(nameof(ResetZoomButtonText));
            OnPropertyChanged(nameof(ClearRangeButtonText));
            OnPropertyChanged(nameof(SettingsWindowTitleText));
            OnPropertyChanged(nameof(TimeFormatLabelText));
            OnPropertyChanged(nameof(LanguageLabelText));
            OnPropertyChanged(nameof(OkButtonText));
            OnPropertyChanged(nameof(CancelButtonText));
            OnPropertyChanged(nameof(RussianOptionText));
            OnPropertyChanged(nameof(EnglishOptionText));
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(FileInfo));
            OnPropertyChanged(nameof(RangeInfoText));
            OnPropertyChanged(nameof(ScrollMaximum));
            OnPropertyChanged(nameof(ScrollValue));
        }

        private void AppSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AppSettings.SelectedLanguage))
                ApplyCurrentLanguage();
        }

        private static CultureInfo GetCultureInfo(AppLanguage language)
        {
            switch (language)
            {
                case AppLanguage.Russian:
                    return new CultureInfo("ru-RU");
                default:
                    return new CultureInfo("en-US");
            }
        }

        #endregion
    }
}
