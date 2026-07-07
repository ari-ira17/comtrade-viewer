using System.Windows;
using ComtradeViewer.ViewModel.ViewModels;

namespace ComtradeViewer.View.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var viewModel = new MainViewModel();

            viewModel.OpenSettingsAction = () =>
            {
                var settingsDialog = new SettingsWindow(viewModel)
                {
                    Owner = this
                };
                if (settingsDialog.ShowDialog() == true)
                {
                    viewModel.ApplySettings();
                }
            };

            this.DataContext = viewModel;
        }
    }
}
