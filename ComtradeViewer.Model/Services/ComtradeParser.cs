using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Linq;
using ComtradeViewer.Model.Models;

namespace ComtradeViewer.Model.Services
{
    public class ComtradeParser : IComtradeParser
    {
        public ComtradeParseResult Parse(string cfgPath, string datPath)
        {
            Encoding encoding;
            try
            {
                encoding = Encoding.GetEncoding(1251);
            }
            catch (NotSupportedException)
            {
                encoding = Encoding.Default;
            }

            var analogChannels = new List<ChannelInfo>();
            var digitalChannels = new List<ChannelInfo>();
            int totalAnalog = 0, totalDigital = 0;

            using (var reader = new StreamReader(cfgPath, encoding))
            {
                reader.ReadLine(); 

                string[] summary = reader.ReadLine().Split(',');
                totalAnalog = int.Parse(summary[1].Replace("A", "").Trim());
                totalDigital = int.Parse(summary[2].Replace("D", "").Trim());

                for (int i = 0; i < totalAnalog; i++)
                {
                    string[] line = reader.ReadLine().Split(',');
                    var channel = new ChannelInfo
                    {
                        Index = int.Parse(line[0].Trim()),
                        Name = line[1].Trim(),
                        Unit = line[4].Trim(),
                        FactorA = double.Parse(line[5].Trim(), CultureInfo.InvariantCulture),
                        FactorB = double.Parse(line[6].Trim(), CultureInfo.InvariantCulture),
                        MinValue = double.Parse(line[8].Trim(), CultureInfo.InvariantCulture), // правильный индекс
                        MaxValue = double.Parse(line[9].Trim(), CultureInfo.InvariantCulture), // правильный индекс
                        IsDigital = false
                    };
                    analogChannels.Add(channel);
                }

                for (int i = 0; i < totalDigital; i++)
                {
                    string[] line = reader.ReadLine().Split(',');
                    var channel = new ChannelInfo
                    {
                        Index = int.Parse(line[0].Trim()),
                        Name = line[1].Trim(),
                        Unit = "",
                        FactorA = 1.0,
                        FactorB = 0.0,
                        MinValue = 0, 
                        MaxValue = 1,
                        IsDigital = true
                    };
                    digitalChannels.Add(channel);
                }
            }

            var allChannels = new List<ChannelInfo>();
            allChannels.AddRange(analogChannels);
            allChannels.AddRange(digitalChannels);

            var result = new Dictionary<string, List<SamplePoint>>();
            foreach (var ch in allChannels)
                result[ch.Name] = new List<SamplePoint>();

            using (var reader = new StreamReader(datPath, encoding))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string[] tokens = line.Split(',');

                    double timeMs = double.Parse(tokens[1].Trim()) / 1000.0;

                    for (int i = 0; i < totalAnalog; i++)
                    {
                        double rawValue = double.Parse(tokens[2 + i].Trim(), CultureInfo.InvariantCulture);
                        double physicalValue = rawValue * analogChannels[i].FactorA + analogChannels[i].FactorB;
                        string channelName = analogChannels[i].Name;
                        result[channelName].Add(new SamplePoint(timeMs, physicalValue));
                    }

                    int digitalStartIndex = 2 + totalAnalog;
                    for (int i = 0; i < totalDigital; i++)
                    {
                        string token = tokens[digitalStartIndex + i].Trim();
                        double value = double.Parse(token, CultureInfo.InvariantCulture);
                        string channelName = digitalChannels[i].Name;
                        result[channelName].Add(new SamplePoint(timeMs, value));
                    }
                }
            }

            foreach (var digitalChannel in digitalChannels)
            {
                if (result.TryGetValue(digitalChannel.Name, out var points) && points.Count > 0)
                {
                    digitalChannel.MinValue = points.Min(p => p.Value);
                    digitalChannel.MaxValue = points.Max(p => p.Value);
                }
            }

            return new ComtradeParseResult { Data = result, Channels = allChannels };
        }
    }
}
