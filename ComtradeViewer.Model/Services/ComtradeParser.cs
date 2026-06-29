using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
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
                        MinValue = int.Parse(line[8].Trim(), CultureInfo.InvariantCulture),
                        MaxValue = int.Parse(line[9].Trim(), CultureInfo.InvariantCulture)
                    };
                    analogChannels.Add(channel);
                }
            }

            var result = new Dictionary<string, List<SamplePoint>>();
            foreach (var ch in analogChannels)
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
                }
            }

            return new ComtradeParseResult { Data = result, Channels = analogChannels };
        }
    }
}
