using System.Text; 
using ComtradeViewer.Model.Services;

namespace ComtradeViewer.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string cfgPath = Path.Combine(baseDir, "../../../TestFiles/test.cfg");
            string datPath = Path.Combine(baseDir, "../../../TestFiles/test.dat");

            try
            {
                var parser = new ComtradeParser();
                
                Console.WriteLine("Читаем файлы...");
                var parsedData = parser.Parse(cfgPath, datPath);

                Console.WriteLine($"\nУспешно распарсено каналов: {parsedData.Count}");

                foreach (var channel in parsedData)
                {
                    string channelName = channel.Key;
                    var points = channel.Value;

                    Console.WriteLine($"Канал: {channelName} | Всего точек: {points.Count}");

                    int pointsToShow = Math.Min(3, points.Count);
                    for (int i = 0; i < pointsToShow; i++)
                    {
                        Console.WriteLine($"  [{i}] Время: {points[i].Time:F3} мс   Значение: {points[i].Value:F5}");
                    }
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[ОШИБКА]: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
