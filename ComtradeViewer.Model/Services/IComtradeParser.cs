using System.Collections.Generic;
using ComtradeViewer.Model.Models;

namespace ComtradeViewer.Model.Services
{
    public interface IComtradeParser
    {
        Dictionary<string, List<SamplePoint>> Parse(string cfgPath, string datPath);
    }
}
