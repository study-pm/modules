using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using HR.Data.Models;
using HR.Pages;
using HR.Services;
using HR.Utilities;
using static HR.Services.AppEventHelper;

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
            // Culture setup
            var culture = new CultureInfo("ru-RU");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(culture.IetfLanguageTag)));
        }

        private async Task<string> GetStartUri(int uid)
        {
            string uri = string.Empty;
            var (cat, name, op, scope) = (EventCategory.Service, "Read", 1, "Приложение");
            try
            {
                uri = await Request.LoadLastState(uid);
                if (string.IsNullOrEmpty(uri))
                    throw new Exception("URI is null or empty");
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat,
                    Name = name,
                    Op = op,
                    Scope = scope,
                    Type = EventType.Success,
                    Message = "Возобновление состояния",
                    Details = "Последнее состояние предыдущего сеанса успешно возобновлено"
                });
                return uri;
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc, "App: restore previous state");
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat,
                    Name = name,
                    Op = op,
                    Scope = scope,
                    Type = EventType.Error,
                    Message = "Ошибка возобновления состояния",
                    Details = exc.Message
                });
                return null;
            }
        }
        public async void LogOut()
        {
            CurrentUser = null;
            Preferences = null;
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.mainFrame.Navigate(new AuthPg());

            var (cat, name, op, scope) = (EventCategory.Auth, "Logout", 3, "Приложение");
            try
            {
                await Request.DeleteUidFileAsync(Data.Models.User.uidFilePath);
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat, Name = name, Op = op, Scope = scope, Type = EventType.Info,
                    Message = "Завершение сеанса", Details = "Гостевой режим"
                });
                EventLogger?.Dispose();
                EventLogger = null;
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc.Message, name);
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat, Name = name, Op = op, Scope = scope, Type = EventType.Error,
                    Message = "Ошибка завершениея сеанса", Details = exc.Message
                });
            }
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
                int uid = int.Parse(File.ReadAllText(Data.Models.User.uidFilePath));
                // Check for user with provided login existence
                return await Request.ctx.Users.FirstAsync(x => x.Id == uid);
            }
            catch (FormatException exc)
            {
                Debug.WriteLine($"Invalid user file data format: {exc.Message}");
                return null;
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"File data points to missing or invalid user: {exc.Message}");
                return null;
            }
        }
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            RaiseAppEvent(new AppEventArgs
            {
                Category = EventCategory.Auth,
                Name = "Shutdown",
                Op = 1,
                Scope = "Приложение",
                Type = EventType.Info,
                Message = "Завершение работы",
                Details = "Остановка приложения"
            });
        }
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            var (cat, name, op, scope) = (EventCategory.Auth, "Startup", 0, "Приложение");
            try
            {
                //var splash = new SplashWindow(); // Custom splash window
                // splash.Show(); // Shows splash window
                var splash = new SplashScreen("Static/Img/graduation-cap-solid.png");
                splash.Show(false); // show splash screen without auto closing (true to auto-close)

                CurrentUser = await GetCurrentUser();
                // Handle preferences
                if (IsAuth)
                {
                    Preferences = await Request.GetPreferences(user.Id);
                    if (Preferences.IsLogOn) EventLogger = new Logger(CurrentUser.Id, Preferences.LogCategories, Preferences.LogTypes);
                }
                var mainWindow = new MainWindow();
                // this.MainWindow = mainWindow; // Assign as main window
                mainWindow.Show();
                // Close splash screen with no delay
                splash.Close(TimeSpan.FromMilliseconds(0));

                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat, Name = name, Op = op, Scope = scope, Type = EventType.Info,
                    Message = IsAuth ? "Возобновление сеанса" : "Новый сеанс",
                    Details = IsAuth ? "Пользовательский режим" : "Гостевой режим"
                });

                if (IsAuth && !string.IsNullOrWhiteSpace(Preferences.StartPage))
                {
                    string uri = Preferences.StartPage;
                    if (uri == "resume")
                        uri = await GetStartUri(CurrentUser.Id);
                    if (!string.IsNullOrWhiteSpace(uri) && UserAppState.IsPageAllowed(uri))
                        mainWindow.mainFrame.Navigate(new Uri(uri, UriKind.Relative));
                }
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc.Message, name);
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat, Name = name, Op = op, Scope = scope, Type = EventType.Error,
                    Message = "Ошибка загрузки данных", Details = exc.Message
                });

                Environment.Exit(1);
            }
        }
    }
}
