namespace ComtradeViewer.Model.Models
{
    public struct SamplePoint
    {
        public double Time { get; set; }  
        public double Value { get; set; } 

        public SamplePoint(double time, double value)
        {
            Time = time;
            Value = value;
        }
    }
}
