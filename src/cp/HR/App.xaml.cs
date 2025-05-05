using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using HR.Utilities;

namespace HR
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        private int? uid;
        public int? UserId
        {
            get => uid;
            set
            {
                uid = value;
                OnPropertyChanged(nameof(IsAuth));
            }
        }
        public bool IsAuth => uid != null;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        private async Task<int?> GetCurrentUser()
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
            return 1;
        }
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                //var splash = new SplashWindow(); // Custom splash window
                // splash.Show(); // Shows splash window
                var splash = new SplashScreen("Img/graduation-cap-solid.png");
                splash.Show(false); // show splash screen without auto closing (true to auto-close)

                UserId = await GetCurrentUser();

                var mainWindow = new MainWindow();
                // this.MainWindow = mainWindow; // Assign as main window
                mainWindow.Show();

                splash.Close(TimeSpan.FromMilliseconds(200));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                Environment.Exit(1);
            }
        }
    }
}
