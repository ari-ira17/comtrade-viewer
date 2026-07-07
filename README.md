# Проект ComtradeViewer (WPF)

Решение (Solution) разделено на 3 независимых проекта согласно паттерну MVVM (Model-View-ViewModel). 
Целевая платформа: .NET Framework 4.0.

---
```text
ComtradeViewer/
│
├── .gitignore
├── .github/
│   └── workflows/
│       └── ci.yml
├── README.md
├── ComtradeViewer.slnx
│
├── ComtradeViewer.Model/                 # Бизнес-логика и парсинг COMTRADE (net40)
│   ├── ComtradeViewer.Model.csproj
│   ├── Models/
│   │   ├── ChannelInfo.cs
│   │   └── SamplePoint.cs
│   └── Services/
│       ├── ComtradeParser.cs
│       ├── ComtradeParseResult.cs
│       ├── DataDownsampler.cs
│       └── IComtradeParser.cs
│
├── ComtradeViewer.ViewModel/             # ViewModel, команды и вспомогательные модели (net40)
│   ├── ComtradeViewer.ViewModel.csproj
│   ├── Converters/
│   ├── Models/
│   │   ├── AppSettings.cs
│   │   ├── ComtradeFile.cs
│   │   └── SettingsChannelItem.cs
│   ├── RelayCommand.cs
│   ├── Services/
│   │   └── SettingsService.cs
│   ├── ViewModelBase.cs
│   └── ViewModels/
│       ├── ChannelPlotViewModel.cs
│       ├── ChannelVisibilityItem.cs
│       └── MainViewModel.cs
│
├── ComtradeViewer.View/                  # WPF-интерфейс (net40)
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── AssemblyInfo.cs
│   ├── ComtradeViewer.View.csproj
│   ├── Converters/
│   │   ├── ColorConverter.cs
│   │   ├── PointsToGeometryConverter.cs
│   │   └── StringToBrushConverter.cs
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   └── Views/
│       ├── ChannelPlotControl.cs
│       ├── SettingsWindow.xaml
│       └── SettingsWindow.xaml.cs
│
├── ComtradeViewer.TestConsole/           # Консольное приложение для ручной проверки
│   └── ComtradeViewer.TestConsole.csproj
│
└── ComtradeViewer.Tests/                 # Автотесты проекта
    └── ComtradeViewer.Tests.csproj.bak
```

---
Запуск:
```bash
dotnet run --project ComtradeViewer.View/ComtradeViewer.View.csproj
```
