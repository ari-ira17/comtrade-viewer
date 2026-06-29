using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using ComtradeViewer.Model.Models;
using ComtradeViewer.ViewModel.ViewModels;

namespace ComtradeViewer.View.Converters
{
    public class PointsToGeometryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ChannelPlotViewModel vm)) return Geometry.Empty;
            var points = vm.Points;
            if (points == null || points.Count == 0) return Geometry.Empty;

            var channel = vm.Channel;
            double minTime = points[0].Time;
            double maxTime = points[points.Count - 1].Time;
            if (maxTime == minTime) maxTime = minTime + 1;

            double minVal = channel.MinValue;
            double maxVal = channel.MaxValue;
            if (maxVal == minVal) maxVal = minVal + 1;

            const double marginLeft = 0.08;
            const double marginRight = 0.02;
            const double marginBottom = 0.08;
            const double marginTop = 0.02;

            var figure = new PathFigure();
            bool first = true;
            foreach (var pt in points)
            {
                double x = (pt.Time - minTime) / (maxTime - minTime);
                double y = (pt.Value - minVal) / (maxVal - minVal);
                y = 1 - y;
                double px = marginLeft + x * (1 - marginLeft - marginRight);
                double py = marginTop + y * (1 - marginTop - marginBottom);

                var point = new Point(px, py);
                if (first)
                {
                    figure.StartPoint = point;
                    first = false;
                }
                else
                {
                    figure.Segments.Add(new LineSegment(point, true));
                }
            }

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            return geometry;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
