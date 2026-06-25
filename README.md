# Проект ComtradeViewer (WPF)

Решение (Solution) разделено на 3 независимых проекта согласно паттерну MVVM (Model-View-ViewModel). 
Целевая платформа: .NET Framework 4.0.

---
```text
ComtradeViewer/
│
├── .gitignore                      # Исключает временные файлы Visual Studio и VS Code
├── .github/
│   └── workflows/
│       └── ci.yml                  # Конфигурация GitHub Actions
├── README.md                       # Документация проекта
├── ComtradeViewer.slnx             # Общий файл решения
│
├── ComtradeViewer.Model/           # Слой парсинга и бизнес-логики (Class Library, net40)
│   ├── ComtradeViewer.Model.csproj 
│   ├── Models/
│   │   ├── ChannelInfo.cs          # Описание канала (название, фаза, коэффициенты из .cfg)
│   │   └── SamplePoint.cs          # Точка осциллограммы (Время, Значение)
│   └── Services/
│       ├── ComtradeParser.cs       # Парсер: чтение текстового .cfg и бинарного .dat
│       └── DataDownsampler.cs      # Алгоритм прореживания (Min-Max)
│
├── ComtradeViewer.ViewModel/       # Слой логики управления (Class Library, net40)
│   ├── ComtradeViewer.ViewModel.csproj 
│   ├── RelayCommand.cs             # Реализация ICommand для привязки кнопок
│   ├── ViewModelBase.cs            # Базовый класс с реализацией INotifyPropertyChanged
│   └── ViewModels/
│       └── MainViewModel.cs        # Обрабатывает команды кнопок и готовит данные для графиков
│
└── ComtradeViewer.View/            # Графический интерфейс (WPF Application, net40)
    ├── ComtradeViewer.View.csproj  
    ├── App.xaml                    
    ├── App.xaml.cs                 
    └── Views/
        ├── MainWindow.xaml         # Разметка главного окна на XAML (кнопки, графики)
        └── MainWindow.xaml.cs
```