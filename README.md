# Проект ComtradeViewer (WPF)

ComtradeViewer — приложение для просмотра и анализа файлов COMTRADE с WPF-интерфейсом и MVVM-архитектурой.  
Решение содержит три основных проекта: Model, ViewModel и View, а также дополнительные проекты для ручной проверки и тестов.

Целевая платформа: .NET Framework 4.0.

---
```text
comtrade-viewer/
│
├── .editorconfig                          # Правила форматирования кода
├── .gitignore                             # Исключения для Git
├── .github/                               # CI/CD workflow
│   └── workflows/
│       └── ci.yml                         # Настройки непрерывной интеграции
├── README.md                              # Описание проекта
├── ComtradeViewer.slnx                    # Решение проекта
│
├── ComtradeViewer.Model/                  # Бизнес-логика и парсинг COMTRADE
│   ├── ComtradeViewer.Model.csproj        # Проект модели
│   ├── Models/                            # Модели данных каналов и точек
│   │   ├── ChannelInfo.cs                 # Описание канала
│   │   └── SamplePoint.cs                 # Точка выборки сигнала
│   └── Services/                          # Сервисы парсинга и обработки данных
│       ├── ComtradeParser.cs              # Парсер COMTRADE-файлов
│       ├── ComtradeParseResult.cs         # Результат разбора файла
│       ├── DataDownsampler.cs             # Снижение количества точек
│       └── IComtradeParser.cs             # Интерфейс парсера
│
├── ComtradeViewer.ViewModel/              # ViewModel, команды, настройки и ресурсы
│   ├── ComtradeViewer.ViewModel.csproj    # Проект ViewModel
│   ├── Converters/                        # Конвертеры значений
│   ├── Models/                            # Модели настроек и файлов
│   │   ├── AppSettings.cs                 # Настройки приложения
│   │   ├── ComtradeFile.cs                # Представление открытого файла
│   │   └── SettingsChannelItem.cs         # Настройка канала
│   ├── RelayCommand.cs                    # Реализация команды
│   ├── Resources/                         # Локализация и строки интерфейса
│   ├── Services/                          # Сервисы работы с настройками
│   │   └── SettingsService.cs             # Загрузка/сохранение настроек
│   ├── ViewModelBase.cs                   # Базовый класс ViewModel
│   └── ViewModels/                        # Основные ViewModel
│       ├── ChannelPlotViewModel.cs        # Модель графика канала
│       ├── ChannelVisibilityItem.cs       # Элемент видимости канала
│       └── MainViewModel.cs               # Главная логика приложения
│
├── ComtradeViewer.View/                   # WPF-интерфейс приложения
│   ├── App.xaml                           # Описание приложения
│   ├── App.xaml.cs                        # Код запуска приложения
│   ├── AssemblyInfo.cs                    # Сведения о сборке
│   ├── ComtradeViewer.View.csproj         # Проект интерфейса
│   ├── Converters/                        # Конвертеры для XAML
│   │   ├── ColorConverter.cs              # Конвертация цвета
│   │   ├── PointsToGeometryConverter.cs   # Построение геометрии графика
│   │   └── StringToBrushConverter.cs      # Преобразование строки в кисть
│   ├── MainWindow.xaml                    # Главное окно
│   ├── MainWindow.xaml.cs                 # Код главного окна
│   └── Views/                             # Пользовательские представления
│       ├── ChannelPlotControl.cs          # Контрол графика канала
│       ├── SettingsWindow.xaml            # Окно настроек
│       └── SettingsWindow.xaml.cs         # Код окна настроек
│
├── ComtradeViewer.TestConsole/            # Консольное приложение для ручной проверки
│   ├── ComtradeViewer.TestConsole.csproj  # Проект консольного тестирования
│   └── TestConsole.cs                     # Точка входа консоли
│
└── ComtradeViewer.Tests/                  # Автотесты проекта
    ├── ModelTests.cs                      # Тесты модели
    └── ViewModelTests.cs                  # Тесты ViewModel
```

---
Запуск приложения:
```bash
dotnet run --project ComtradeViewer.View/ComtradeViewer.View.csproj
```
