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
├── ComtradeViewer.Model/                 # Парсинг и бизнес-логика (net40)
│   ├── ComtradeViewer.Model.csproj
│   ├── Models/
│   │   ├── ChannelInfo.cs                # Канал: имя, коэф., мин/макс из .cfg
│   │   └── SamplePoint.cs                # Точка (время, значение)
│   └── Services/
│       ├── IComtradeParser.cs            # Интерфейс парсера
│       ├── ComtradeParser.cs             # Реализация: чтение .cfg и .dat
│       ├── ComtradeParseResult.cs        # Результат парсинга (данные + каналы)
│       └── DataDownsampler.cs            # Прореживание Min-Max
│
├── ComtradeViewer.ViewModel/             # Логика и команды (net40)
│   ├── ComtradeViewer.ViewModel.csproj
│   ├── RelayCommand.cs                   # ICommand
│   ├── ViewModelBase.cs                  # INotifyPropertyChanged
│   └── ViewModels/
│       ├── MainViewModel.cs              # Команды, коллекция каналов
│       └── ChannelPlotViewModel.cs       # Модель представления одного канала
│
└── ComtradeViewer.View/                  # WPF-интерфейс (net40)
    ├── ComtradeViewer.View.csproj
    ├── App.xaml
    ├── App.xaml.cs
    ├── MainWindow.xaml                   # Главное окно (ItemsControl + ScrollViewer)
    ├── MainWindow.xaml.cs
    ├── Views/
    │   └── ChannelPlotControl.cs         # Полностью кодогенерируемый контрол графика
    └── Converters/
        └── PointsToGeometryConverter.cs  # (опционально) конвертер для привязок, если используется
```

---
Запуск `dotnet run --project ComtradeViewer.View/ComtradeViewer.View.csproj`
