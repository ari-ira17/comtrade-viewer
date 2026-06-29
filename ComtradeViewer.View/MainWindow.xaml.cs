using System.Windows;
using Microsoft.Win32;
using ComtradeViewer.ViewModel.ViewModels;

namespace ComtradeViewer.View
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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
                    vm.OpenFileCommand.Execute(new string[] { cfgPath, datPath });
                }
            }
        }
    }
}
