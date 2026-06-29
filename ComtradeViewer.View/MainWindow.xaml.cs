using System.Windows;
using Microsoft.Win32;
using ComtradeViewer.ViewModel.ViewModels;

namespace ComtradeViewer.View.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            DataContext = new MainViewModel();
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "COMTRADE Configuration (*.cfg)|*.cfg",
                Title = "Выберите файл конфигурации COMTRADE"
            };

            if (dialog.ShowDialog() == true)
            {
                string cfgPath = dialog.FileName;
                string datPath = cfgPath.Replace(".cfg", ".dat");

                if (DataContext is MainViewModel vm)
                {
                    if (vm.OpenFileCommand.CanExecute(new string[] { cfgPath, datPath }))
                    {
                        vm.OpenFileCommand.Execute(new string[] { cfgPath, datPath });
                    }
                }
            }
        }
    }
}