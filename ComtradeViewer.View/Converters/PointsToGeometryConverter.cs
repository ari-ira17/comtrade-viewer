using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using ComtradeViewer.Model.Models;

namespace ComtradeViewer.View.Converters
{
    public class PointsToGeometryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Geometry.Empty;

            var points = value as IEnumerable<SamplePoint>;
            if (points == null) return Geometry.Empty;

            var figure = new PathFigure();
            bool first = true;
            foreach (var pt in points)
            {
                if (first)
                {
                    figure.StartPoint = new Point(pt.Time, pt.Value);
                    first = false;
                }
                else
                {
                    figure.Segments.Add(new LineSegment(new Point(pt.Time, pt.Value), true));
                }
            }

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            return geometry;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}