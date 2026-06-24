using System;
using System.Collections.Generic;
using ComtradeViewer.Model.Models;

namespace ComtradeViewer.Model.Services
{
    public class DataDownsampler
    {
        public static List<SamplePoint> MinMax(List<SamplePoint> source, int targetPointsCount)
        {
            if (source.Count <= targetPointsCount) return source;

            var result = new List<SamplePoint>();
            int bucketSize = source.Count / (targetPointsCount / 2);

            for (int i = 0; i < source.Count; i += bucketSize)
            {
                int end = Math.Min(i + bucketSize, source.Count);
                SamplePoint min = source[i];
                SamplePoint max = source[i];

                for (int j = i + 1; j < end; j++)
                {
                    if (source[j].Value < min.Value) min = source[j];
                    if (source[j].Value > max.Value) max = source[j];
                }

                if (min.Time < max.Time)
                {
                    result.Add(min);
                    result.Add(max);
                }
                else
                {
                    result.Add(max);
                    result.Add(min);
                }
            }
            return result;
        }
    }
}
