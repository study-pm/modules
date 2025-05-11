using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using HR.Models;
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
        private User user;
        public User CurrentUser {
            get => user;
            set {
                if (value == user) return;
                user = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsAuth));
            }
        }
        public bool IsAuth => CurrentUser != null;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        private async Task<User> GetCurrentUser()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string uidFilePath = Path.Combine(basePath, "local", "user.uid");

            bool fileExists = File.Exists(uidFilePath);

            if (!fileExists)
            {
                // Return null if uid file doesn't exist
                return null;
            }

            string uidContent = File.ReadAllText(uidFilePath);
            if (!int.TryParse(uidContent, out int userId))
            {
                // Return null if invalid format
                return null;
            }

            // Mock checking user existences from DB
            await Utils.MockAsync(3000);
            return new User { Id = 1, Login = "Current user" };
        }
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                //var splash = new SplashWindow(); // Custom splash window
                // splash.Show(); // Shows splash window
                var splash = new SplashScreen("Img/graduation-cap-solid.png");
                splash.Show(false); // show splash screen without auto closing (true to auto-close)

                CurrentUser = await GetCurrentUser();

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
