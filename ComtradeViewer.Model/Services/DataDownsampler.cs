using System;
using System.Collections.Generic;
using ComtradeViewer.Model.Models;

namespace ComtradeViewer.Model.Services
{
    public class DataDownsampler
    {
        private const int MinimumBucketSize = 1;
        private const int PointsPerBucket = 2;

        public static List<SamplePoint> MinMax(List<SamplePoint> source, int targetPointsCount)
        {
            if (source == null || source.Count == 0)
                return new List<SamplePoint>();

            if (source.Count <= targetPointsCount)
                return new List<SamplePoint>(source);

            int bucketCount = Math.Max(MinimumBucketSize, targetPointsCount / PointsPerBucket);
            int bucketSize = Math.Max(MinimumBucketSize, (source.Count + bucketCount - 1) / bucketCount);
            var result = new List<SamplePoint>(targetPointsCount * PointsPerBucket);

            for (int start = 0; start < source.Count; start += bucketSize)
            {
                int end = Math.Min(start + bucketSize, source.Count);
                if (end <= start)
                    continue;

                SamplePoint min = source[start];
                SamplePoint max = source[start];

                for (int index = start + 1; index < end; index++)
                {
                    var point = source[index];
                    if (point.Value < min.Value) min = point;
                    if (point.Value > max.Value) max = point;
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
