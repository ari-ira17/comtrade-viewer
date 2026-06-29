using System.Collections.Generic;
using ComtradeViewer.Model.Models;

namespace ComtradeViewer.Model.Services
{
    public class ComtradeParseResult
    {
        public Dictionary<string, List<SamplePoint>> Data { get; set; }
        public List<ChannelInfo> Channels { get; set; }
    }
}
