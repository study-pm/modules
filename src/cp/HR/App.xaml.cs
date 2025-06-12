using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using HR.Data.Models;
using HR.Pages;
using HR.Services;
using HR.Utilities;

namespace HR
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        public static new App Current => (App)Application.Current;
        public ICommand LogOutCommand { get; }
        private Data.Models.User user;
        public HR.Data.Models.User CurrentUser {
            get => user;
            set {
                if (value == user) return;
                user = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsAuth));
            }
        }
        public Preferences Preferences { get; set; }
        public bool IsAuth => CurrentUser != null;
        public Logger EventLogger { get; set; }
        public App()
        {
            LogOutCommand = new RelayCommand(_ => LogOut());
        }
        public async void LogOut()
        {
            CurrentUser = null;
            Preferences = null;
            EventLogger = null;
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.mainFrame.Navigate(new AuthPg());
            await Request.DeleteUidFileAsync(Data.Models.User.uidFilePath);
            StatusInformer.ReportInfo("Гостевой режим");
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        private async Task<Data.Models.User> GetCurrentUser()
        {
            bool fileExists = File.Exists(HR.Data.Models.User.uidFilePath);

            if (!fileExists)
            {
                // Return null if uid file doesn't exist
                return null;
            }
            try
            {
                StatusInformer.ReportProgress("Проверка файла пользователя");
                int uid = int.Parse(File.ReadAllText(Data.Models.User.uidFilePath));
                // Check for user with provided login existence
                return await Request.ctx.Users.FirstAsync(x => x.Id == uid);
            }
            catch (FormatException exc)
            {
                StatusInformer.ReportFailure($"Неверный формат данных файла пользователя: {exc.Message}");
                return null;
            }
            catch (Exception exc)
            {
                StatusInformer.ReportFailure($"Отсутствует пользователь из файла пользователя: {exc.Message}");
                return null;
            }
        }
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                //var splash = new SplashWindow(); // Custom splash window
                // splash.Show(); // Shows splash window
                var splash = new SplashScreen("Static/Img/graduation-cap-solid.png");
                splash.Show(false); // show splash screen without auto closing (true to auto-close)

                CurrentUser = await GetCurrentUser();
                if (IsAuth)
                {
                    Preferences = await Request.GetPreferences(user.Id);
                    List<AppEventHelper.EventCategory> categories = new List<AppEventHelper.EventCategory> {
                        AppEventHelper.EventCategory.Auth,
                        AppEventHelper.EventCategory.Data,
                        AppEventHelper.EventCategory.Navigation,
                        AppEventHelper.EventCategory.Service
                    };
                    List<AppEventHelper.EventType> types = new List<AppEventHelper.EventType> {
                        AppEventHelper.EventType.Fatal,
                        AppEventHelper.EventType.Error,
                        AppEventHelper.EventType.Warning,
                        AppEventHelper.EventType.Success
                    };
                    EventLogger = new Logger(CurrentUser.Id, categories, types);
                }
                var mainWindow = new MainWindow();
                // this.MainWindow = mainWindow; // Assign as main window
                mainWindow.Show();

                splash.Close(TimeSpan.FromMilliseconds(200));
                if (IsAuth) StatusInformer.ReportSuccess("Пользовательский режим");
                else StatusInformer.ReportInfo("Гостевой режим");
                // StatusInformer.ReportProgress("Загрузка данных...");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                Environment.Exit(1);
            }
        }
    }
}
