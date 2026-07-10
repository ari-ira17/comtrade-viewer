using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Linq;
using ComtradeViewer.Model.Models;

namespace ComtradeViewer.Model.Services
{
    /// <summary>
    /// Индексы полей в строке конфигурационного файла для аналоговых каналов
    /// </summary>
    internal enum ConfigAnalogFieldIndex
    {
        /// <summary>Номер канала</summary>
        Index = 0,
        /// <summary>Название канала</summary>
        Name = 1,
        /// <summary>Единица измерения</summary>
        Unit = 4,
        /// <summary>Коэффициент масштабирования A</summary>
        FactorA = 5,
        /// <summary>Коэффициент масштабирования B</summary>
        FactorB = 6,
        /// <summary>Минимальное значение</summary>
        MinValue = 8,
        /// <summary>Максимальное значение</summary>
        MaxValue = 9
    }

    /// <summary>
    /// Индексы полей в строке конфигурационного файла для цифровых каналов
    /// </summary>
    internal enum ConfigDigitalFieldIndex
    {
        /// <summary>Номер канала</summary>
        Index = 0,
        /// <summary>Название канала</summary>
        Name = 1
    }

    /// <summary>
    /// Индексы полей в строке данных
    /// </summary>
    internal enum DataFileFieldIndex
    {
        /// <summary>Номер записи</summary>
        RecordNumber = 0,
        /// <summary>Временная метка</summary>
        TimeMs = 1,
        /// <summary>Начало данных аналоговых каналов</summary>
        AnalogDataStart = 2
    }

    /// <summary>
    /// Константы парсера COMTRADE
    /// </summary>
    internal static class ComtradeConstants
    {
        /// <summary>Разделитель полей в конфигурационном и файлах данных</summary>
        public const char FieldSeparator = ',';

        /// <summary>Суффикс для количества аналоговых каналов в строке конфигурации</summary>
        public const string AnalogChannelCountSuffix = "A";

        /// <summary>Суффикс для количества цифровых каналов в строке конфигурации</summary>
        public const string DigitalChannelCountSuffix = "D";

        /// <summary>Коэффициент для преобразования микросекунд в секунды</summary>
        public const double TimeConversionFactor = 1_000_000.0;

        /// <summary>Значение FactorA по умолчанию для цифровых каналов</summary>
        public const double DefaultDigitalFactorA = 1.0;

        /// <summary>Значение FactorB по умолчанию для цифровых каналов</summary>
        public const double DefaultDigitalFactorB = 0.0;

        /// <summary>Минимальное значение для цифровых каналов</summary>
        public const double DefaultDigitalMinValue = 0.0;

        /// <summary>Максимальное значение для цифровых каналов</summary>
        public const double DefaultDigitalMaxValue = 1.0;

        /// <summary>Пустая строка для цифровых каналов</summary>
        public const string EmptyUnit = "";

        /// <summary>Кодировка по умолчанию для COMTRADE файлов (Windows-1251)</summary>
        public const int DefaultEncodingCodePage = 1251;
    }

    public class ComtradeParser : IComtradeParser
    {
        private readonly Encoding _encoding;

        public ComtradeParser() : this(ComtradeConstants.DefaultEncodingCodePage)
        {
        }

        public ComtradeParser(int encodingCodePage)
        {
            try
            {
                _encoding = Encoding.GetEncoding(encodingCodePage);
            }
            catch (NotSupportedException)
            {
                _encoding = Encoding.Default;
            }
        }

        public ComtradeParseResult Parse(string cfgPath, string datPath)
        {
            var analogChannels = new List<ChannelInfo>();
            var digitalChannels = new List<ChannelInfo>();
            int totalAnalog = 0, totalDigital = 0;

            using (var reader = new StreamReader(cfgPath, _encoding))
            {
                reader.ReadLine(); // Пропускаем первую строку

                string[] summary = reader.ReadLine().Split(ComtradeConstants.FieldSeparator);
                totalAnalog = int.Parse(summary[1].Replace(ComtradeConstants.AnalogChannelCountSuffix, "").Trim());
                totalDigital = int.Parse(summary[2].Replace(ComtradeConstants.DigitalChannelCountSuffix, "").Trim());

                // Чтение аналоговых каналов
                for (int i = 0; i < totalAnalog; i++)
                {
                    string[] line = reader.ReadLine().Split(ComtradeConstants.FieldSeparator);
                    var channel = new ChannelInfo
                    {
                        Index = int.Parse(line[(int)ConfigAnalogFieldIndex.Index].Trim()),
                        Name = line[(int)ConfigAnalogFieldIndex.Name].Trim(),
                        Unit = line[(int)ConfigAnalogFieldIndex.Unit].Trim(),
                        FactorA = double.Parse(line[(int)ConfigAnalogFieldIndex.FactorA].Trim(), CultureInfo.InvariantCulture),
                        FactorB = double.Parse(line[(int)ConfigAnalogFieldIndex.FactorB].Trim(), CultureInfo.InvariantCulture),
                        MinValue = double.Parse(line[(int)ConfigAnalogFieldIndex.MinValue].Trim(), CultureInfo.InvariantCulture),
                        MaxValue = double.Parse(line[(int)ConfigAnalogFieldIndex.MaxValue].Trim(), CultureInfo.InvariantCulture),
                        IsDigital = false
                    };
                    analogChannels.Add(channel);
                }

                // Чтение цифровых каналов
                for (int i = 0; i < totalDigital; i++)
                {
                    string[] line = reader.ReadLine().Split(ComtradeConstants.FieldSeparator);
                    var channel = new ChannelInfo
                    {
                        Index = int.Parse(line[(int)ConfigDigitalFieldIndex.Index].Trim()),
                        Name = line[(int)ConfigDigitalFieldIndex.Name].Trim(),
                        Unit = ComtradeConstants.EmptyUnit,
                        FactorA = ComtradeConstants.DefaultDigitalFactorA,
                        FactorB = ComtradeConstants.DefaultDigitalFactorB,
                        MinValue = ComtradeConstants.DefaultDigitalMinValue,
                        MaxValue = ComtradeConstants.DefaultDigitalMaxValue,
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

            // Чтение данных
            using (var reader = new StreamReader(datPath, _encoding))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string[] tokens = line.Split(ComtradeConstants.FieldSeparator);

                    // Переводим микросекунды в секунды
                    double timeSec = double.Parse(tokens[(int)DataFileFieldIndex.TimeMs].Trim(), CultureInfo.InvariantCulture)
                        / ComtradeConstants.TimeConversionFactor;

                    // Обработка аналоговых каналов
                    for (int i = 0; i < totalAnalog; i++)
                    {
                        double rawValue = double.Parse(tokens[(int)DataFileFieldIndex.AnalogDataStart + i].Trim(), CultureInfo.InvariantCulture);
                        double physicalValue = rawValue * analogChannels[i].FactorA + analogChannels[i].FactorB;
                        string channelName = analogChannels[i].Name;
                        result[channelName].Add(new SamplePoint(timeSec, physicalValue));
                    }

                    // Обработка цифровых каналов
                    int digitalStartIndex = (int)DataFileFieldIndex.AnalogDataStart + totalAnalog;
                    for (int i = 0; i < totalDigital; i++)
                    {
                        string token = tokens[digitalStartIndex + i].Trim();
                        double value = double.Parse(token, CultureInfo.InvariantCulture);
                        string channelName = digitalChannels[i].Name;
                        result[channelName].Add(new SamplePoint(timeSec, value));
                    }
                }
            }

            // Определение диапазонов для цифровых каналов
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
