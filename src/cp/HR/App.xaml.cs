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

        private Task<int?> GetCurrentUser()
        {
            // solution directory
            string solutionRoot = FindSolutionRoot();
            // user local files
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            // current working folder
            string currentDir = Environment.CurrentDirectory;
            // current execution folder
            string exeDir = AppDomain.CurrentDomain.BaseDirectory;
            if (string.IsNullOrEmpty(solutionRoot))
            {
                // Return null to prevent throwing exception
                return Task.FromResult<int?>(null);
            }
            string uidFilePath = Path.Combine(solutionRoot, "Local", "user.uid");
            uidFilePath = Path.Combine(exeDir, "local", "user.uid");

            bool fileExists = File.Exists(uidFilePath);

            if (!fileExists)
            {
                // Return null if uid file doesn't exist
                return Task.FromResult<int?>(null);
            }

            string uidContent = File.ReadAllText(uidFilePath);
            if (!int.TryParse(uidContent, out int userId))
            {
                // Return null if invalid format
                return Task.FromResult<int?>(null);
            }

            // Mock checking user existences from DB
            var tcs = new TaskCompletionSource<int?>();
            int mockDelay = 3000;
            var timer = new System.Timers.Timer(mockDelay);

            timer.Elapsed += (sender, args) =>
            {
                timer.Stop();
                timer.Dispose();
                tcs.SetResult(userId);
            };

            timer.AutoReset = false; // To make it one-time only
            timer.Start();

            return tcs.Task;
        }
        public static string FindSolutionRoot()
        {
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo directory = new DirectoryInfo(currentDir);

            while (directory != null)
            {
                if (Directory.GetFiles(directory.FullName, "*.sln").Length > 0)
                {
                    return directory.FullName;
                }
                directory = directory.Parent;
            }
            return null;
        }
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                //var splash = new SplashWindow(); // Custom splash window
                var splash = new SplashScreen("Img/graduation-cap-solid.png");
                // splash.Show(true); // true auto close after loading
                splash.Show(false); // show splash screen without auto closing

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
