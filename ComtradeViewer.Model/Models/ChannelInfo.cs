namespace ComtradeViewer.Model.Models
{
    public class ChannelInfo
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public double FactorA { get; set; }
        public double FactorB { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public bool IsDigital { get; set; }
    }
}
