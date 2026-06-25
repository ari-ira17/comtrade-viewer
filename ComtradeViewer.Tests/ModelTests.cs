using System;
using System.IO;
using System.Linq;
using Xunit;

using ComtradeViewer.Model.Services;

namespace ComtradeViewer.Tests
{
    public class ComtradeParserTests : IDisposable
    {
        private readonly string _cfgPath;
        private readonly string _datPath;

        public ComtradeParserTests()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            _cfgPath = Path.GetTempFileName();
            _datPath = Path.GetTempFileName();
        }

        [Fact]
        public void Parse_ValidComtradeData_CorrectlyCalculatesFirstRow()
        {

            string cfgContent = @" 2, 2
14,6A,8D
1,Ia,,,A,0.000054,0.000000,0.000000,-214,214,100,5,S
2,Ua,,,V,0.001412,0.000000,0.000000,-99998,99998,10000,100,S
3,Ib,,,A,0.000054,0.000000,0.000000,-188,188,100,5,S
4,Ub,,,V,0.000050,0.000000,0.000000,-7596,7596,10000,100,S
5,Ic,,,A,0.000054,0.000000,0.000000,-214,214,100,5,S
6,Uc,,,V,0.000050,0.000000,0.000000,-8545,8545,10000,100,S
1,TC1,0
2,TC2,0
3,TC3,0
4,TC4,0
5,TC5,0
6,TC6,0
7,TC7,0
8,TC8,0
50
1
8000,1
11/30/18,15:41:51.726000
11/30/18,15:41:59.959000
ASCII";

            string datContent = "0000000001,0000000000,-00195,-14182,-00192,-14175,-00196,-14181,0,0,0,0,0,0,0,0";

            File.WriteAllText(_cfgPath, cfgContent);
            File.WriteAllText(_datPath, datContent);

            var parser = new ComtradeParser();

            var result = parser.Parse(_cfgPath, _datPath);

            Assert.NotNull(result);
            Assert.True(result.ContainsKey("Ia"));
            Assert.True(result.ContainsKey("Ua"));

            // Проверка канала Ia (Ток фазы А)
            var firstIaPoint = result["Ia"].First();
            Assert.Equal(0.0, firstIaPoint.Time, precision: 5);
            // Ожидаем: -195 * 0.000054 + 0 = -0.01053
            Assert.Equal(-0.01053, firstIaPoint.Value, precision: 5);

            // Проверка канала Ua (Напряжение фазы А)
            var firstUaPoint = result["Ua"].First();
            Assert.Equal(0.0, firstUaPoint.Time, precision: 5);
            // Ожидаем: -14182 * 0.001412 + 0 = -20.024984
            Assert.Equal(-20.02498, firstUaPoint.Value, precision: 5);
        }

        public void Dispose()
        {
            // Выполняется автоматически после Assert'ов.
            // Удаляем временные файлы, чтобы не засорять диск.
            if (File.Exists(_cfgPath)) File.Delete(_cfgPath);
            if (File.Exists(_datPath)) File.Delete(_datPath);
        }
    }
}
