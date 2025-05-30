using HR.Data.Models;
using HR.Models;
using HR.Pages;
using HR.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace HR.Services
{
    internal static class Request
    {
        public static HREntities ctx = new HREntities();
        private static string basePath = "Data/Sources/";
        public static async Task<List<EmployeeX>> GetEmployeesFromXML()
        {
            string path = basePath + "employees.xml";
            string xmlContent;
            try
            {
                StatusInformer.ReportProgress("Загрузка данных");
                if (!File.Exists(path))
                    throw new FileNotFoundException($"Файл не найден: {path}");

                var reader = new StreamReader(path);
                xmlContent = await reader.ReadToEndAsync();
                StatusInformer.ReportSuccess("Данные успешно извлечены");
                return EmployeeX.LoadFromXml(xmlContent);
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
            return new List<EmployeeX>();
        }
        public static async Task<List<Employee>> GetEmployees()
        {
            try
            {
                StatusInformer.ReportProgress("Загрузка данных о сотрудниках");
                List<Employee> employees = await ctx.Employees
                    .Include(e => e.Developments)
                    .Include(e => e.Educations)
                    .Include(e => e.Retrainings)
                    .Include(e => e.Staffs)
                    .ToListAsync();
                StatusInformer.ReportSuccess("Данные сотрудников успешно извлечены");
                return employees;
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc.Message);
                StatusInformer.ReportFailure($"Ошибка извлечения данных о сотрудниках: {exc.Message}");
                return new List<Employee>();  // Возврат значения при ошибке
            }
        }
        public static async Task<List<Employee>> GetEmployeesUnregistered()
        {
            try
            {
                StatusInformer.ReportProgress("Загрузка данных о сотрудниках");
                List<Employee> employees = await ctx.Employees.Where(e => !e.Users.Any())
                    .ToListAsync();
                StatusInformer.ReportSuccess("Данные сотрудников успешно извлечены");
                return employees;
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc.Message);
                StatusInformer.ReportFailure($"Ошибка извлечения данных о сотрудниках: {exc.Message}");
                return new List<Employee>();  // Возврат значения при ошибке
            }
        }
        public static async Task<List<HR.Data.Models.User>> GetUsers(bool isNewCtx = false)
        {
            var context = isNewCtx ? new HREntities() : ctx;
            try
            {
                StatusInformer.ReportProgress("Загрузка данных о пользователях");
                List<Data.Models.User> users = await context.Users.ToListAsync();
                StatusInformer.ReportSuccess("Данные пользователей успешно извлечены");
                return users;
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc.Message);
                StatusInformer.ReportFailure($"Ошибка извлечения данных о пользователях: {exc.Message}");
                return new List<Data.Models.User>();  // Возврат значения при ошибке
            }
        }
        internal static async Task DeleteUidFileAsync(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    // Удаляем файл асинхронно через Task.Run, так как File.Delete синхронный
                    await Task.Run(() => File.Delete(filePath));
                    StatusInformer.ReportSuccess("Файл пользователя успешно удалён");
                }
            }
            catch (Exception ex)
            {
                StatusInformer.ReportFailure($"Ошибка удаления файла пользователя: {ex.Message}");
            }
        }
        internal static async Task SaveUidToFileAsync(int userId, string uidFilePath)
        {
            try
            {
                string path = uidFilePath;
                // Записать uid в файл асинхронно
                using (var writer = new StreamWriter(path, false, Encoding.UTF8))
                {
                    await writer.WriteAsync(userId.ToString());
                }
                StatusInformer.ReportSuccess("Файл пользователя успешно сохранен");
            }
            catch (Exception ex)
            {
                StatusInformer.ReportFailure($"Ошибка сохранения файла пользователя: {ex.Message}");
            }
        }
        public static async Task<Preferences> GetPreferences(int uid)
        {
            try
            {
                StatusInformer.ReportProgress("Извлечение предпочтений пользователя");
                return await Preferences.LoadAsync(uid);
            }
            catch (Exception exc)
            {
                StatusInformer.ReportFailure($"Ошибка извлечения предпочтений пользователя: ${exc.Message}");
                return null;
            }
        }
    }
}
