using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ComtradeViewer.ViewModel.Models;
using ComtradeViewer.ViewModel.ViewModels;

namespace ComtradeViewer.View.Views
{
    public partial class MainWindow : Window
    {
        private Point _tabDragStartPoint;
        private bool _tabDragStarted;

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

        private void TabItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _tabDragStartPoint = e.GetPosition(null);
            _tabDragStarted = true;
        }

        private void TabItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_tabDragStarted || e.LeftButton != MouseButtonState.Pressed)
                return;

            Point currentPos = e.GetPosition(null);
            if (Math.Abs(currentPos.X - _tabDragStartPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(currentPos.Y - _tabDragStartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                _tabDragStarted = false;
                if (sender is TabItem tabItem && tabItem.DataContext is ComtradeFile)
                {
                    DragDrop.DoDragDrop(tabItem, tabItem, DragDropEffects.Move);
                }
            }
        }

        private void TabItem_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }

        private void TabItem_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(TabItem)) is TabItem draggedItem &&
                sender is TabItem targetItem &&
                draggedItem != targetItem &&
                DataContext is MainViewModel vm)
            {
                var draggedFile = draggedItem.DataContext as ComtradeFile;
                var targetFile = targetItem.DataContext as ComtradeFile;
                if (draggedFile != null && targetFile != null)
                {
                    int oldIndex = vm.OpenFiles.IndexOf(draggedFile);
                    int newIndex = vm.OpenFiles.IndexOf(targetFile);
                    if (oldIndex >= 0 && newIndex >= 0)
                        vm.MoveFile(oldIndex, newIndex);
                }
            }
        }
    }
}
