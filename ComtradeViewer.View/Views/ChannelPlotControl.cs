using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using ComtradeViewer.ViewModel.ViewModels;

namespace ComtradeViewer.View.Views
{
    public class ChannelPlotControl : UserControl
    {
        private Canvas _plotCanvas;

        public ChannelPlotControl()
        {
            this.Height = 110;

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var infoBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1, 0, 0, 0),
                Padding = new Thickness(10)
            };
            var infoStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

            var nameBlock = new TextBlock { FontWeight = FontWeights.Bold, FontSize = 14, Margin = new Thickness(0, 0, 0, 5) };
            nameBlock.SetBinding(TextBlock.TextProperty, new Binding("Channel.Name"));
            infoStack.Children.Add(nameBlock);

            var unitBlock = new TextBlock { FontSize = 12 };
            unitBlock.SetBinding(TextBlock.TextProperty, new Binding("Channel.Unit") { StringFormat = "Ед. изм.: {0}" });
            infoStack.Children.Add(unitBlock);

            var minBlock = new TextBlock { FontSize = 12 };
            minBlock.SetBinding(TextBlock.TextProperty, new Binding("Channel.MinValue") { StringFormat = "Мин.: {0}" });
            infoStack.Children.Add(minBlock);

            var maxBlock = new TextBlock { FontSize = 12 };
            maxBlock.SetBinding(TextBlock.TextProperty, new Binding("Channel.MaxValue") { StringFormat = "Макс.: {0}" });
            infoStack.Children.Add(maxBlock);

            infoBorder.Child = infoStack;
            Grid.SetColumn(infoBorder, 0);
            grid.Children.Add(infoBorder);

            _plotCanvas = new Canvas
            {
                Background = Brushes.White,
                ClipToBounds = true,
                Margin = new Thickness(5, 0, 5, 5)
            };

            Grid.SetColumn(_plotCanvas, 1);
            grid.Children.Add(_plotCanvas);

            this.Content = grid;

            this.DataContextChanged += (s, e) => DrawPlot();
            this.Loaded += (s, e) => DrawPlot();
            this.SizeChanged += (s, e) => DrawPlot();

            if (this.DataContext != null)
                DrawPlot();
        }

        private void DrawPlot()
        {
            _plotCanvas.Children.Clear();

            if (!(DataContext is ChannelPlotViewModel vm)) return;
            var points = vm.Points;
            if (points == null || points.Count == 0) return;

            double canvasWidth = _plotCanvas.ActualWidth;
            double canvasHeight = _plotCanvas.ActualHeight;

            if (canvasWidth <= 0 || canvasHeight <= 0) return;

            var channel = vm.Channel;

            double maxAbs = points.Max(p => Math.Abs(p.Value));
            if (maxAbs == 0) maxAbs = 1;

            maxAbs *= 1.1;

            double minTime = points[0].Time;
            double maxTime = points[points.Count - 1].Time;
            if (maxTime == minTime) maxTime = minTime + 1;

            const double margin = 10;
            double plotWidth = canvasWidth - 2 * margin;
            double plotHeight = canvasHeight - 2 * margin;

            double zeroY = canvasHeight / 2;

            var yAxis = new Line
            {
                X1 = margin,
                Y1 = margin,
                X2 = margin,
                Y2 = canvasHeight - margin,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            _plotCanvas.Children.Add(yAxis);

            var xAxis = new Line
            {
                X1 = margin,
                Y1 = zeroY,
                X2 = canvasWidth - margin,
                Y2 = zeroY,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            _plotCanvas.Children.Add(xAxis);

            var zeroLabel = new TextBlock
            {
                Text = "0",
                FontSize = 10,
                Foreground = Brushes.Black
            };
            Canvas.SetLeft(zeroLabel, margin + 4);
            Canvas.SetTop(zeroLabel, zeroY - 16);
            _plotCanvas.Children.Add(zeroLabel);

            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                bool first = true;
                foreach (var pt in points)
                {
                    double x = margin + (pt.Time - minTime) / (maxTime - minTime) * plotWidth;
                    double y = zeroY - (pt.Value / maxAbs) * (plotHeight / 2);

                    y = Math.Max(margin, Math.Min(canvasHeight - margin, y));
                    var point = new Point(x, y);

                    if (first)
                    {
                        ctx.BeginFigure(point, false, false);
                        first = false;
                    }
                    else
                    {
                        ctx.LineTo(point, true, false);
                    }
                }
            }

            var path = new Path
            {
                Stroke = Brushes.SteelBlue,
                StrokeThickness = 1.5,
                Data = geometry
            };
            _plotCanvas.Children.Add(path);
        }
    }
}
