using HR.Models;
using HR.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace HR.Services
{
    internal static class Request
    {
        private static string basePath = "Data/Sources/";
        public static async Task<List<Employee>> GetEmployees()
        {
            string path = basePath + "employees.xml";
            string xmlContent;
            try
            {
                if (!File.Exists(path))
                    throw new FileNotFoundException($"Файл не найден: {path}");

                var reader = new StreamReader(path);
                xmlContent = await reader.ReadToEndAsync();
                StatusInformer.ReportSuccess("Данные успешно извлечены");
                return Employee.LoadFromXml(xmlContent);
            }
            catch (FileNotFoundException exc)
            {
                Debug.WriteLine($"Error while reading the file: {exc.Message}");
                StatusInformer.ReportFailure($"Ошибка чтения файла: {exc.Message}");
            }
            catch (XmlException exc)
            {
                Debug.WriteLine($"Error while parsing XML: {exc.Message}");
                StatusInformer.ReportFailure($"Ошибка разбора XML: {exc.Message}");
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"Unknown error: {exc.Message}");
                StatusInformer.ReportFailure($"Ошибка извлечения данных: {exc.Message}");
            }
            // Вернуть пустой список при ошибке
            return new List<Employee>();
        }
    }
}
