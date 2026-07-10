using System;
using System.Windows.Media;
using Newtonsoft.Json;

namespace ComtradeViewer.ViewModel.Converters
{
    public class MediaColorConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Color);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string hex = reader.Value?.ToString();
            if (string.IsNullOrEmpty(hex))
                return Colors.Gray;

            try
            {
                return (Color)ColorConverter.ConvertFromString(hex);
            }
            catch
            {
                return Colors.Gray;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((Color)value).ToString());
        }
    }
}
