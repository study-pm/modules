using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Data.Models
{
    [Serializable]
    public class UserAppState
    {
        public static readonly string basePath = "Cache";
        public static readonly string cacheFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, basePath);
        public string LastState { get; set; }
        public static List<StartupPage> Pages { get; set; } = new List<StartupPage>
        {
            new StartupPage { Id = 1, Name = "SettingsPg", Title = "Безопасность", Uri = "Pages/SettingsPg.xaml" },
            new StartupPage { Id = 2, Name = "HomePg", Title = "Главная", Uri = "Pages/HomePg.xaml" },
            new StartupPage { Id = 3, Name = "LogPg", Title = "Журнал", Uri = "Pages/LogPg.xaml" },
            new StartupPage { Id = 4, Name = "PreferencesPg", Title = "Настройки", Uri = "Pages/PreferencesPg.xaml" },
            new StartupPage { Id = 5, Name = "ClassesPg", Title = "Классы", Uri = "Pages/ClassesPg.xaml" },
            new StartupPage { Id = 6, Name = "ProfilePg", Title = "Профиль", Uri = "Pages/ProfilePg.xaml" },
            new StartupPage { Id = 7, Name = "HelpPg", Title = "Помощь", Uri = "Pages/HelpPg.xaml" },
            new StartupPage { Id = 8, Name = "StaffPg", Title = "Сотрудники", Uri = "Pages/StaffPg.xaml" },
        };
        public static bool IsPageAllowed(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
                return false;
            return Pages.Any(p => string.Equals(p.Uri, uri, StringComparison.OrdinalIgnoreCase));
        }
    }
}
