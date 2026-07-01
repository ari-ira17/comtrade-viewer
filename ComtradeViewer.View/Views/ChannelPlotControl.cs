using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ComtradeViewer.Model.Models;
using ComtradeViewer.Model.Services;
using ComtradeViewer.ViewModel.ViewModels;

namespace ComtradeViewer.View.Views
{
    public class ChannelPlotControl : UserControl
    {
        private Canvas _plotCanvas;
        private Canvas _overlayCanvas;
        private MainViewModel? _mainVm;
        private ChannelPlotViewModel? _vm;
        private bool _isDrawn = false;

        private Point _dragStartPoint;
        private bool _isDragging = false;
        private double _dragStartTimeMin;
        private double _dragStartTimeMax;

        private SamplePoint? _hoverPoint;

        private const double MarginLeft = 55;
        private const double MarginRight = 15;
        private const double MarginTop = 15;
        private const double MarginBottom = 15;

        public ChannelPlotControl()
        {
            this.Height = 200;

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Левая панель
            var infoBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1, 0, 0, 0),
                Padding = new Thickness(10)
            };
            var infoStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

            var nameBlock = new TextBlock { FontWeight = FontWeights.Bold, FontSize = 18, Margin = new Thickness(0, 0, 0, 5) };
            nameBlock.SetBinding(TextBlock.TextProperty, new Binding("Channel.Name"));
            infoStack.Children.Add(nameBlock);

            var unitBlock = new TextBlock { FontSize = 14 };
            unitBlock.SetBinding(TextBlock.TextProperty, new Binding("Channel.Unit") { StringFormat = "Ед. изм.: {0}" });
            infoStack.Children.Add(unitBlock);

            var minBlock = new TextBlock { FontSize = 14 };
            minBlock.SetBinding(TextBlock.TextProperty, new Binding("Channel.MinValue") { StringFormat = "Мин.: {0}" });
            infoStack.Children.Add(minBlock);

            var maxBlock = new TextBlock { FontSize = 14 };
            maxBlock.SetBinding(TextBlock.TextProperty, new Binding("Channel.MaxValue") { StringFormat = "Макс.: {0}" });
            infoStack.Children.Add(maxBlock);

            infoBorder.Child = infoStack;
            Grid.SetColumn(infoBorder, 0);
            grid.Children.Add(infoBorder);

            var canvasGrid = new Grid();
            canvasGrid.Children.Add(new Canvas { Background = Brushes.White }); 

            _plotCanvas = new Canvas
            {
                Background = Brushes.White,
                ClipToBounds = true
            };
            _plotCanvas.SetValue(Grid.RowProperty, 0);
            _plotCanvas.SetValue(Grid.ColumnProperty, 0);
            canvasGrid.Children.Add(_plotCanvas);

            _overlayCanvas = new Canvas
            {
                Background = Brushes.Transparent,
                ClipToBounds = true
            };
            _overlayCanvas.SetValue(Grid.RowProperty, 0);
            _overlayCanvas.SetValue(Grid.ColumnProperty, 0);
            canvasGrid.Children.Add(_overlayCanvas);

            Grid.SetColumn(canvasGrid, 1);
            grid.Children.Add(canvasGrid);

            this.Content = grid;

            this.DataContextChanged += (s, e) =>
            {
                _vm = DataContext as ChannelPlotViewModel;
                if (_vm != null)
                {
                    _mainVm = FindMainViewModel();
                    if (_mainVm != null)
                    {
                        _mainVm.PropertyChanged += (sender, args) =>
                        {
                            if (args.PropertyName == nameof(MainViewModel.TimeRangeChanged))
                            {
                                _isDrawn = false;
                                DrawPlot();
                            }
                        };
                    }
                }
                DrawPlot();
            };

            this.Loaded += (s, e) => DrawPlot();
            this.SizeChanged += (s, e) =>
            {
                _isDrawn = false;
                DrawPlot();
            };

            _overlayCanvas.MouseWheel += OnMouseWheel;
            _overlayCanvas.MouseLeftButtonDown += OnMouseLeftButtonDown;
            _overlayCanvas.MouseLeftButtonUp += OnMouseLeftButtonUp;
            _overlayCanvas.MouseMove += OnMouseMove;
            _overlayCanvas.MouseLeave += OnMouseLeave;

            if (this.DataContext != null)
                DrawPlot();
        }

        private MainViewModel? FindMainViewModel()
        {
            DependencyObject parent = this;
            while (parent != null)
            {
                if (parent is FrameworkElement fe && fe.DataContext is MainViewModel vm)
                    return vm;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        private void DrawPlot()
        {
            if (_isDrawn) return;

            _plotCanvas.Children.Clear();

            if (_vm == null || _mainVm == null || _vm.Points == null || _vm.Points.Count == 0)
                return;

            double canvasWidth = _plotCanvas.ActualWidth;
            double canvasHeight = _plotCanvas.ActualHeight;
            if (canvasWidth <= 0 || canvasHeight <= 0)
                return;

            var points = _vm.Points;
            var channel = _vm.Channel;

            double timeMin = _mainVm.TimeMin;
            double timeMax = _mainVm.TimeMax;

            if (timeMax - timeMin < 0.00001)
            {
                timeMin = points[0].Time;
                timeMax = points[points.Count - 1].Time;
            }

            var visiblePoints = points.Where(p => p.Time >= timeMin && p.Time <= timeMax).ToList();
            if (visiblePoints.Count == 0)
                return;

            const int maxPoints = 2000;
            if (visiblePoints.Count > maxPoints)
            {
                visiblePoints = DataDownsampler.MinMax(visiblePoints, maxPoints);
            }

            double maxAbs = visiblePoints.Max(p => Math.Abs(p.Value));
            if (maxAbs == 0) maxAbs = 1;
            maxAbs *= 1.1;

            double plotWidth = canvasWidth - MarginLeft - MarginRight;
            double plotHeight = canvasHeight - MarginTop - MarginBottom;
            double zeroY = canvasHeight / 2;

            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                bool first = true;
                foreach (var pt in visiblePoints)
                {
                    double x = MarginLeft + (pt.Time - timeMin) / (timeMax - timeMin) * plotWidth;
                    double y = zeroY - (pt.Value / maxAbs) * (plotHeight / 2);
                    y = Math.Max(MarginTop, Math.Min(canvasHeight - MarginBottom, y));
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
                Stroke = Brushes.Gray,
                StrokeThickness = 1.5,
                Data = geometry
            };
            _plotCanvas.Children.Add(path);

            var yAxis = new Line
            {
                X1 = MarginLeft,
                Y1 = MarginTop,
                X2 = MarginLeft,
                Y2 = canvasHeight - MarginBottom,
                Stroke = Brushes.Black,
                StrokeThickness = 1.5
            };
            _plotCanvas.Children.Add(yAxis);

            var xAxis = new Line
            {
                X1 = MarginLeft,
                Y1 = zeroY,
                X2 = canvasWidth - MarginRight,
                Y2 = zeroY,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            _plotCanvas.Children.Add(xAxis);

            double minVal = channel.MinValue;
            double maxVal = channel.MaxValue;

            double yMin = zeroY - (minVal / maxAbs) * (plotHeight / 2);
            yMin = Math.Max(MarginTop, Math.Min(canvasHeight - MarginBottom, yMin));

            double yMax = zeroY - (maxVal / maxAbs) * (plotHeight / 2);
            yMax = Math.Max(MarginTop, Math.Min(canvasHeight - MarginBottom, yMax));

            var minLine = new Line
            {
                X1 = MarginLeft,
                Y1 = yMin,
                X2 = canvasWidth - MarginRight,
                Y2 = yMin,
                Stroke = Brushes.Green,
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 4, 4 }
            };
            _plotCanvas.Children.Add(minLine);

            var maxLine = new Line
            {
                X1 = MarginLeft,
                Y1 = yMax,
                X2 = canvasWidth - MarginRight,
                Y2 = yMax,
                Stroke = Brushes.Red,
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 4, 4 }
            };
            _plotCanvas.Children.Add(maxLine);

            var zeroLabel = new TextBlock
            {
                Text = "0",
                FontSize = 12,
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold
            };
            Canvas.SetLeft(zeroLabel, MarginLeft - 22);
            Canvas.SetTop(zeroLabel, zeroY - 10);
            _plotCanvas.Children.Add(zeroLabel);

            var minLabel = new TextBlock
            {
                Text = $"{minVal}",
                FontSize = 11,
                Foreground = Brushes.DarkGreen
            };
            Canvas.SetLeft(minLabel, MarginLeft - 50);
            Canvas.SetTop(minLabel, yMin - 8);
            _plotCanvas.Children.Add(minLabel);

            var maxLabel = new TextBlock
            {
                Text = $"{maxVal}",
                FontSize = 11,
                Foreground = Brushes.DarkRed
            };
            Canvas.SetLeft(maxLabel, MarginLeft - 50);
            Canvas.SetTop(maxLabel, yMax - 8);
            _plotCanvas.Children.Add(maxLabel);

            if (_vm.SelectedPoint.HasValue)
            {
                var sp = _vm.SelectedPoint.Value;
                double selX = MarginLeft + (sp.Time - timeMin) / (timeMax - timeMin) * plotWidth;
                double selY = zeroY - (sp.Value / maxAbs) * (plotHeight / 2);
                selY = Math.Max(MarginTop, Math.Min(canvasHeight - MarginBottom, selY));

                var marker = new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = Brushes.Red,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                Canvas.SetLeft(marker, selX - 4);
                Canvas.SetTop(marker, selY - 4);
                _plotCanvas.Children.Add(marker);

                var tooltip = new Border
                {
                    Background = Brushes.LightYellow,
                    Padding = new Thickness(6),
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Child = new TextBlock
                    {
                        Text = $"t={sp.Time:F6} с\nval={sp.Value:F6}",
                        FontSize = 14
                    }
                };
                Canvas.SetLeft(tooltip, selX + 5);
                Canvas.SetTop(tooltip, selY - 15);
                _plotCanvas.Children.Add(tooltip);
            }

            _isDrawn = true;
            UpdateOverlay();
        }

        private void UpdateOverlay()
        {
            _overlayCanvas.Children.Clear();

            if (_vm == null || _mainVm == null || !_hoverPoint.HasValue)
                return;

            double canvasWidth = _overlayCanvas.ActualWidth;
            double canvasHeight = _overlayCanvas.ActualHeight;
            if (canvasWidth <= 0 || canvasHeight <= 0)
                return;

            var sp = _hoverPoint.Value;
            double timeMin = _mainVm.TimeMin;
            double timeMax = _mainVm.TimeMax;
            double plotWidth = canvasWidth - MarginLeft - MarginRight;
            double plotHeight = canvasHeight - MarginTop - MarginBottom;
            double zeroY = canvasHeight / 2;

            double x = MarginLeft + (sp.Time - timeMin) / (timeMax - timeMin) * plotWidth;
            double maxAbs = _vm.Points.Max(p => Math.Abs(p.Value));
            if (maxAbs == 0) maxAbs = 1;
            double y = zeroY - (sp.Value / maxAbs) * (plotHeight / 2);
            y = Math.Max(MarginTop, Math.Min(canvasHeight - MarginBottom, y));

            var marker = new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = Brushes.Orange,
                Stroke = Brushes.Black,
                StrokeThickness = 1.5
            };
            Canvas.SetLeft(marker, x - 5);
            Canvas.SetTop(marker, y - 5);
            _overlayCanvas.Children.Add(marker);

            var tooltip = new Border
            {
                Background = Brushes.LightYellow,
                Padding = new Thickness(6),
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Child = new TextBlock
                {
                    Text = $"t={sp.Time:F6} с\nval={sp.Value:F6}",
                    FontSize = 14
                }
            };

            double tooltipX = x + 10;
            double tooltipY = y - 10;
            if (tooltipX + 160 > canvasWidth - MarginRight)
                tooltipX = x - 170;
            if (tooltipY + 50 > canvasHeight - MarginBottom)
                tooltipY = y - 50;

            Canvas.SetLeft(tooltip, tooltipX);
            Canvas.SetTop(tooltip, tooltipY);
            _overlayCanvas.Children.Add(tooltip);
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_mainVm == null) return;

            double canvasWidth = _overlayCanvas.ActualWidth;
            if (canvasWidth <= 0) return;

            var pos = e.GetPosition(_overlayCanvas);
            double plotWidth = canvasWidth - MarginLeft - MarginRight;

            double mouseTime = _mainVm.TimeMin + (pos.X - MarginLeft) / plotWidth * (_mainVm.TimeMax - _mainVm.TimeMin);
            mouseTime = Math.Max(_mainVm.TimeMin, Math.Min(_mainVm.TimeMax, mouseTime));

            double factor = e.Delta > 0 ? 0.9 : 1.1;
            double range = _mainVm.TimeMax - _mainVm.TimeMin;
            double newRange = range * factor;
            if (newRange < 0.00001) newRange = 0.00001;

            double globalMin = _mainVm.AllChannels.SelectMany(c => c.PlotViewModel.Points).Min(p => p.Time);
            double globalMax = _mainVm.AllChannels.SelectMany(c => c.PlotViewModel.Points).Max(p => p.Time);
            double maxRange = globalMax - globalMin;
            if (newRange > maxRange) newRange = maxRange;

            double newMin = mouseTime - (mouseTime - _mainVm.TimeMin) / range * newRange;
            double newMax = mouseTime + (_mainVm.TimeMax - mouseTime) / range * newRange;

            if (newMin < globalMin) newMin = globalMin;
            if (newMax > globalMax) newMax = globalMax;

            _mainVm.SetTimeRange(newMin, newMax);
            e.Handled = true;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_mainVm == null) return;
            _dragStartPoint = e.GetPosition(_overlayCanvas);
            _isDragging = true;
            _dragStartTimeMin = _mainVm.TimeMin;
            _dragStartTimeMax = _mainVm.TimeMax;
            _overlayCanvas.CaptureMouse();
            e.Handled = true;
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                _overlayCanvas.ReleaseMouseCapture();
                e.Handled = true;
            }
            else
            {
                SelectPoint(e);
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && _mainVm != null)
            {
                double canvasWidth = _overlayCanvas.ActualWidth;
                if (canvasWidth <= 0) return;

                var pos = e.GetPosition(_overlayCanvas);
                double deltaX = pos.X - _dragStartPoint.X;
                double plotWidth = canvasWidth - MarginLeft - MarginRight;
                double range = _dragStartTimeMax - _dragStartTimeMin;
                double deltaTime = (deltaX / plotWidth) * range;

                double newMin = _dragStartTimeMin - deltaTime;
                double newMax = _dragStartTimeMax - deltaTime;

                double globalMin = _mainVm.AllChannels.SelectMany(c => c.PlotViewModel.Points).Min(p => p.Time);
                double globalMax = _mainVm.AllChannels.SelectMany(c => c.PlotViewModel.Points).Max(p => p.Time);
                if (newMin < globalMin) { newMin = globalMin; newMax = globalMin + range; }
                if (newMax > globalMax) { newMax = globalMax; newMin = globalMax - range; }
                if (newMin >= globalMin && newMax <= globalMax)
                    _mainVm.SetTimeRange(newMin, newMax);
            }
            else if (!_isDragging && _vm != null && _mainVm != null)
            {
                double canvasWidth = _overlayCanvas.ActualWidth;
                if (canvasWidth <= 0) return;

                var pos = e.GetPosition(_overlayCanvas);
                double plotWidth = canvasWidth - MarginLeft - MarginRight;
                double time = _mainVm.TimeMin + (pos.X - MarginLeft) / plotWidth * (_mainVm.TimeMax - _mainVm.TimeMin);

                var closest = _vm.Points.OrderBy(p => Math.Abs(p.Time - time)).FirstOrDefault();
                if (_vm.Points.Any())
                {
                    _hoverPoint = closest;
                    UpdateOverlay();
                }
                else
                {
                    _hoverPoint = null;
                    UpdateOverlay();
                }
            }
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            _hoverPoint = null;
            UpdateOverlay();
        }

        private void SelectPoint(MouseButtonEventArgs e)
        {
            if (_vm == null || _mainVm == null) return;
            double canvasWidth = _overlayCanvas.ActualWidth;
            if (canvasWidth <= 0) return;

            var pos = e.GetPosition(_overlayCanvas);
            double plotWidth = canvasWidth - MarginLeft - MarginRight;
            double time = _mainVm.TimeMin + (pos.X - MarginLeft) / plotWidth * (_mainVm.TimeMax - _mainVm.TimeMin);

            var closest = _vm.Points.OrderBy(p => Math.Abs(p.Time - time)).FirstOrDefault();
            if (_vm.Points.Any())
            {
                _vm.SelectedPoint = closest;
                _isDrawn = false;
                DrawPlot();
            }
        }
    }
}
