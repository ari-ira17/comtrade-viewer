using System.Collections.Generic;
using ComtradeViewer.Model.Models;

namespace ComtradeViewer.Model.Services
{
    public interface IComtradeParser
    {
        ComtradeParseResult Parse(string cfgPath, string datPath);
    }
}
