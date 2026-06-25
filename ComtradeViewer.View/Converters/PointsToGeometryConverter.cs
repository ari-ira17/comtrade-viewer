using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using ComtradeViewer.Model.Models;

namespace ComtradeViewer.View.Converters
{
    public class PointsToGeometryConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is List<SamplePoint> points) || points.Count == 0) return null;

            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                context.BeginFigure(new Point(points[0].Time, points[0].Value), false, false);
                for (int i = 1; i < points.Count; i++)
                {
                    context.LineTo(new Point(points[i].Time, points[i].Value), true, true);
                }
            }
            geometry.Freeze();
            return geometry;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) 
            => throw new NotImplementedException();
    }
}
