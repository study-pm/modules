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
using static HR.Services.AppEventHelper;

namespace HR.Services
{
    internal static class Request
    {
        public static HREntities ctx = new HREntities();
        private static string basePath = "Data/Sources/";
        private static string logsPath = "Logs";
        public static string GetLocalDirPath(string dirName) => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dirName);
        public static string GetLocalFilePath(int uid, string dirName) => Path.Combine(GetLocalDirPath(dirName), $"{uid.ToString()}.json");
        public static async Task<List<ClassGuidance>> GetClassGuidance()
        {
            try
            {
                StatusInformer.ReportProgress("Загрузка классов");
                List<ClassGuidance> roles = await ctx.ClassGuidances.ToListAsync();
                StatusInformer.ReportSuccess("Классы успешно извлечены");
                return roles;
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc.Message);
                StatusInformer.ReportFailure($"Ошибка извлечения классов: {exc.Message}");
                return new List<ClassGuidance>();
            }
        }
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
        public static async Task ClearLog(int uid)
        {
            var (cat, name, op, scope) = (EventCategory.Data, "Deletion", 3, "Журнал пользователя");
            try
            {
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat, Name = name, Op = op, Scope = scope, Type = EventType.Progress, Message = "Удаление данных"
                });
                await JsonHelper.ClearCollectionAsync<AppEventArgs>(GetLocalFilePath(uid, logsPath));
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat, Name = name, Op = op, Scope = scope, Type = EventType.Success,
                    Message = "Данные успешно удалены",
                    Details = "Удалены все данные"
                });
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc.Message, name);
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat, Name = name, Op = op, Scope = scope, Type = EventType.Error,
                    Message = "Ошибка удаления данных", Details = exc.Message
                });
                throw exc;
            }
        }
        public static async Task<int> DeleteLogItems(int uid, List<AppEventArgs> items)
        {
            var (cat, name, scope) = (EventCategory.Data, "Deletion", "Журнал пользователя");
            try
            {
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat, Name = name, Scope = scope, Type = EventType.Progress, Message = "Удаление данных"
                });
                int removedCount = await JsonHelper.RemoveItemsAsync<AppEventArgs>(GetLocalFilePath(uid, logsPath), x => items.Any(del => del.Id == x.Id));
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat, Name = name, Scope = scope, Type = EventType.Success,
                    Message = "Данные успешно удалены", Details = $"Удалено записей: {removedCount}"
                });
                return removedCount;
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc.Message, name);
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat, Name = name, Scope = scope, Type = EventType.Error, Message = "Ошибка удаления данных", Details = exc.Message
                });
                throw exc;
            }
        }
        public static async Task<List<AppEventArgs>> GetLog(int uid)
        {
            var (cat, name, scope) = (EventCategory.Data, "Extraction", "Журнал пользователя");
            try
            {
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat, Name = name, Scope = scope, Type = EventType.Progress, Message = "Загрузка данных"
                });
                List<AppEventArgs> events = await JsonHelper.LoadCollectionAsync<AppEventArgs>(GetLocalFilePath(uid, logsPath));
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat, Name = name, Scope = scope, Type = EventType.Success, Message = "Данные успешно загружены"
                });
                return events;
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc.Message, name);
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat, Name = name, Scope = scope, Type = EventType.Error, Message = "Ошибка загрузки данных", Details = exc.Message
                });
                return new List<AppEventArgs>();
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
        public static async Task<List<Role>> GetRoles()
        {
            try
            {
                StatusInformer.ReportProgress("Загрузка ролей");
                List<Role> roles = await ctx.Roles.ToListAsync();
                StatusInformer.ReportSuccess("Роли успешно извлечены");
                return roles;
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc.Message);
                StatusInformer.ReportFailure($"Ошибка извлечения ролей: {exc.Message}");
                return new List<Role>();
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
        public static async Task<List<Department>> LoadDepartments()
        {
            using (var db = new HREntities())
            {
                return await db.Departments
                    .OrderBy(dep => dep.Id)
                    .ToListAsync()
                    .ConfigureAwait(false);
            }
        }
        public static async Task<List<Employee>> LoadEmployees()
        {
            using (var db = new HREntities())
            {
                return await db.Employees.Include("Staffs.Position").Include("Staffs.Assignments").Include("Staffs.Department")
                    .OrderBy(emp => emp.Surname)
                    .ToListAsync()
                    .ConfigureAwait(false);
            }
        }
        public static async Task<List<Employee>> LoadEmployees(bool isCtx)
        {
            if (isCtx) return await ctx.Employees.OrderBy(emp => emp.Surname).ToListAsync();
            return await ctx.Employees.Include("Staffs.Position").Include("Staffs.Assignments").Include("Staffs.Department")
                    .OrderBy(emp => emp.Surname)
                    .ToListAsync()
                    .ConfigureAwait(false);
        }
        public static async Task<List<Position>> LoadPositions()
        {
            using (var db = new HREntities())
            {
                return await db.Positions
                    .OrderBy(dep => dep.Title)
                    .ToListAsync()
                    .ConfigureAwait(false);
            }
        }
        public static async Task<List<Grade>> LoadGrades()
        {
            using (var db = new HREntities())
            {
                return await db.Grades
                    .OrderBy(g => g.Id)
                    .ToListAsync()
                    .ConfigureAwait(false);
            }
        }
        public static async Task<List<Subject>> LoadSubjects()
        {
            using (var db = new HREntities())
            {
                return await db.Subjects
                    .OrderBy(s => s.Title)
                    .ToListAsync()
                    .ConfigureAwait(false);
            }
        }
        /// <summary>
        /// Asynchronously simulates an operation that completes after a specified delay.
        /// </summary>
        /// <param name="delay">The delay in milliseconds before the task completes.</param>
        /// <param name="isSuccess">Determines whether the task completes successfully or with an exception. Default is true (success).</param>
        /// <returns>A task that completes with <c>true</c> if <paramref name="isSuccess"/> is true; otherwise, the task faults with an exception.</returns>
        public static Task<bool> MockAsync(int delay, bool isSuccess = true)
        {
            var tcs = new TaskCompletionSource<bool>();
            var timer = new System.Timers.Timer(delay);

            timer.Elapsed += (sender, args) =>
            {
                timer.Stop();
                timer.Dispose();
                if (isSuccess) tcs.SetResult(true);
                else tcs.SetException(new Exception("Mock failure"));
            };

            timer.AutoReset = false; // To make it one-time only
            timer.Start();

            return tcs.Task;
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
                Preferences prefs =  await Preferences.LoadAsync(uid);
                if (!prefs.IsLogOn) return prefs;
                if (prefs.LogCategories == null || prefs.LogCategories.Count == 0)
                {
                    prefs.LogCategories = new List<int> { 0, 1, 2, 3 };
                }
                if (prefs.LogTypes == null || prefs.LogTypes.Count == 0)
                {
                    prefs.LogTypes = new List<int> { 1, 2, 3, 4 };
                }
                return prefs;
            }
            catch (Exception exc)
            {
                StatusInformer.ReportFailure($"Ошибка извлечения предпочтений пользователя: ${exc.Message}");
                return null;
            }
        }
    }
}
