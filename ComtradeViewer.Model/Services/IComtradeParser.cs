using System.Collections.Generic;
using ComtradeViewer.Model.Models;

namespace ComtradeViewer.Model.Services
{
    public interface IComtradeParser
    {
        /// <summary>
        /// Парсит COMTRADE файлы конфигурации и данных
        /// </summary>
        /// <param name="cfgPath">Путь к файлу конфигурации (.cfg)</param>
        /// <param name="datPath">Путь к файлу данных (.dat)</param>
        /// <returns>Результат парсинга с данными и информацией о каналах</returns>
        ComtradeParseResult Parse(string cfgPath, string datPath);
    }
}
