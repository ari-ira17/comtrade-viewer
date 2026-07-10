using System;
using System.Collections.ObjectModel;
using System.IO;
using ComtradeViewer.ViewModel.Models;
using Newtonsoft.Json;

namespace ComtradeViewer.ViewModel.Services
{
    public static class SettingsService
    {
        private static readonly string SettingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ComtradeViewer",
            "settings.json");

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    var settings = JsonConvert.DeserializeObject<AppSettings>(json);
                    if (settings != null)
                    {
                        if (settings.ChannelColors == null)
                            settings.ChannelColors = new ObservableCollection<ChannelColorEntry>();
                        return settings;
                    }
                }
            }
            catch
            {
            }

            return new AppSettings();
        }

        public static void Save(AppSettings settings)
        {
            try
            {
                if (settings == null)
                    return;

                if (settings.ChannelColors == null)
                    settings.ChannelColors = new ObservableCollection<ChannelColorEntry>();

                string directory = Path.GetDirectoryName(SettingsFilePath);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(SettingsFilePath, json);
            }
            catch
            {
            }
        }
    }
}
