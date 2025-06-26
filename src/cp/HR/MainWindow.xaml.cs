using HR.Controls;
using HR.Data.Models;
using HR.Pages;
using HR.Services;
using HR.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static HR.Services.AppEventHelper;

namespace HR
{
    public class MenuViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        private bool _isInProgress;
        public bool IsInProgress
        {
            get => _isInProgress;
            set
            {
                if (_isInProgress == value) return;
                _isInProgress = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<MenuFilter> Filters { get; } = new ObservableCollection<MenuFilter>()
        {
            new MenuFilter()
            {
                Icon = (Geometry)Application.Current.FindResource("UserFriendsSolidPath"),
                Name = "Виды деятельности",
                Title = "Сотрудники",
                Page = "StaffPg",
                PageUri = "Pages/StaffPg.xaml",
                Values = new ObservableCollection<FilterValue>(
                    ActivityHelper.GetAllActivities().Select(a => new FilterValue
                    {
                        Id = a.Id,
                        Title = a.Title
                    })
                )
            },
            new MenuFilter()
            {
                Icon = (Geometry)Application.Current.FindResource("SitemapSolidPath"),
                Name = "Структурные подразделения",
                Title = "Подразделения",
                Page = "StaffPg",
                PageUri = "Pages/StaffPg.xaml",
                Values = new ObservableCollection<FilterValue>() // initialize empty collection
            },
            new MenuFilter()
            {
                Icon = (Geometry)Application.Current.FindResource("ProjectDiagramSolidPath"),
                Name = "Должности",
                Title = "Должности",
                Page = "StaffPg",
                PageUri = "Pages/StaffPg.xaml",
                Values = new ObservableCollection<FilterValue>() // initialize empty collection
            },
            new MenuFilter()
            {
                Icon = (Geometry)Application.Current.FindResource("BookOpenSolidPath"),
                Name = "Учебные предметы",
                Title = "Предметы",
                Page = "StaffPg",
                PageUri = "Pages/StaffPg.xaml",
                Values = new ObservableCollection<FilterValue>() // initialize empty collection
            },
            new MenuFilter()
            {
                Icon = (Geometry)Application.Current.FindResource("UsersSolidPath"),
                Name = "Классное руководство",
                Title = "Классы",
                Page = "ClassesPg",
                PageUri = "Pages/ClassesPg.xaml",
                Values = new ObservableCollection<FilterValue>() // initialize empty collection
            },
            new MenuFilter()
            {
                Icon = (Geometry)Application.Current.FindResource("QuestionCircleSolidPath"),
                Name = "Справочная информация",
                Title = "Справка",
                Page = "HelpPg",
                PageUri = "Pages/HelpPg.xaml",
                IsDisabledOnChecked = true,
                Values = new ObservableCollection<FilterValue>() // initialize empty collection
            }
        };

        public MenuViewModel()
        {
            foreach (MenuFilter filter in Filters)
                filter.PropertyChanged += Filter_PropertyChanged;
        }

        public async Task SetFilterData()
        {
            IsInProgress = true;
            // Start both tasks without awaiting immediately
            var getDptTask = Task.Run(async () =>
            {
                using (var ctx = new HR.Data.Models.HREntities())
                {
                    return await ctx.Departments.Select(x => new FilterValue { Id = x.Id, Title = x.Title }).ToListAsync();
                }
            });
            var getPosTask = Task.Run(async () =>
            {
                using (var ctx = new HR.Data.Models.HREntities())
                {
                    return await ctx.Positions.Select(x => new FilterValue { Id = x.Id, Title = x.Title }).OrderBy(x => x.Title).ToListAsync();
                }
            });
            var getSubjTask = Task.Run(async () =>
            {
                using (var ctx = new HR.Data.Models.HREntities())
                {
                    return await ctx.Subjects.Select(x => new FilterValue { Id = x.Id, Title = x.Title }).OrderBy(x => x.Title).ToListAsync();
                }
            });
            var getClassTask = Task.Run(async () =>
            {
                using (var ctx = new HR.Data.Models.HREntities())
                {
                    return await ctx.Grades.Select(x => new FilterValue { Id = x.Id, Title = x.Title }).ToListAsync();
                }
            });

            // Await both tasks in parallel
            await Task.WhenAll(getDptTask, getPosTask, getSubjTask, getClassTask);

            // Process results
            //var departments = departmentsTask.Result;
            //var subjects = subjectsTask.Result;

            IsInProgress = false;

            Filters[1].Values = new ObservableCollection<FilterValue>(getDptTask.Result);
            Filters[2].Values = new ObservableCollection<FilterValue>(getPosTask.Result);
            Filters[3].Values = new ObservableCollection<FilterValue>(getSubjTask.Result);
            Filters[4].Values = new ObservableCollection<FilterValue>(getClassTask.Result);
        }

        private void Filter_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Frame frame { get; set; }
        public MenuViewModel MenuVM { get; } = new MenuViewModel();
        private const double MediumWidth = 1080;
        private const double NarrowWidth = 720;
        private const double MinimalWidth = 70;
        private const double NormalWidth = 250;
        private const double NormalMargin = 30;
        public MainWindow()
        {
            InitializeComponent();
            MainWindow.frame = mainFrame;
            // Handle window resize and elements visibility
            this.SizeChanged += MainWindow_SizeChanged;
            UpdateVisibility(this.ActualWidth);
            // Handle closing main window
            this.Closing += MainWindow_Closing;
            // Implement navigation command bindings
            CommandBinding goToPageBinding = new CommandBinding(NavigationCommands.GoToPage, GoToPage_Executed, GoToPage_CanExecute);
            this.CommandBindings.Add(goToPageBinding);
            this.DataContext = this;
        }
        private void GoToPage_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;

            if (!(e.Parameter is string uri) || string.IsNullOrEmpty(uri))
                // Можно добавить логику проверки, например, что параметр не пустой
                return;

            try
            {
                var resourceUri = new Uri($"pack://application:,,,/{uri}", UriKind.Absolute);
                var streamInfo = Application.GetResourceStream(resourceUri);

                if (streamInfo == null) return;

                e.CanExecute = true;
            }
            catch
            {
                // Ошибка при формировании URI или получении ресурса - команда недоступна
                return;
            }
        }

        private void GoToPage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is string uri)
            {
                mainFrame.Navigate(new Uri(uri, UriKind.Relative));
            }
        }
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateVisibility(e.NewSize.Width);
        }
        private void SetRightSide(bool IsVisible)
        {
            RightAside.Visibility = IsVisible ? Visibility.Visible : Visibility.Collapsed;
            RightSplitter.Visibility = IsVisible ? Visibility.Visible : Visibility.Collapsed;
            MainLayoutContainer.ColumnDefinitions[4].Width = IsVisible ? new GridLength(NormalWidth, GridUnitType.Pixel) : new GridLength(0);
            MainLayoutContainer.ColumnDefinitions[4].MinWidth = IsVisible ? MinimalWidth : 0;
        }
        private void SetLeftSide(bool IsVisible)
        {
            LeftAside.Visibility = IsVisible ? Visibility.Visible : Visibility.Collapsed;
            LeftSplitter.Visibility = IsVisible ? Visibility.Visible : Visibility.Collapsed;
            MainLayoutContainer.ColumnDefinitions[0].Width = IsVisible ? new GridLength(NormalWidth, GridUnitType.Pixel) : new GridLength(0);
            MainLayoutContainer.ColumnDefinitions[0].MinWidth = IsVisible ? MinimalWidth : 0;
        }
        public void UpdateVisibility(double width)
        {
            if (width < NarrowWidth)
            {
                SetLeftSide(false);
                SetRightSide(false);
                TopHeader.SetNarrowMode(false);
            }
            else if (width < MediumWidth)
            {
                SetLeftSide(App.Current.Preferences.IsLeftAsideOff ? false : true);
                SetRightSide(false);
                TopHeader.SetNarrowMode(false);
            }
            else
            {
                SetLeftSide(App.Current.Preferences.IsLeftAsideOff ? false: true);
                SetRightSide(App.Current.Preferences.IsRightAsideOff ? false : true);
                TopHeader.SetNarrowMode(false);
            }
        }
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!App.Current.IsAuth || App.Current.Preferences.StartPage != "resume") return;

            string lastUri = null;
            if (mainFrame.CurrentSource != null)
            {
                lastUri = mainFrame.CurrentSource.ToString();
            }
            else if (mainFrame.Content != null)
            {
                var page = mainFrame.Content as Page;
                lastUri = page?.GetType().FullName;
            }

            if (string.IsNullOrEmpty(lastUri)) return;

            var (cat, name, op, scope) = (EventCategory.Service, "Shutdown", 0, "Приложение");
            try
            {
                bool res = Request.SaveLastState(App.Current.CurrentUser.Id, lastUri);
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat,
                    Name = name,
                    Op = op,
                    Scope = scope,
                    Type = EventType.Success,
                    Message = "Состояние успешно сохранено",
                    Details = $"Последнее состояние: {lastUri}"
                });
            }
            catch(Exception exc)
            {
                Debug.WriteLine(exc, "MainWindow: preserve state");
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat,
                    Name = name,
                    Op = op,
                    Scope = scope,
                    Type = EventType.Error,
                    Message = "Ошибка сохранения состояния",
                    Details = exc.Message
                });
            }
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await MenuVM.SetFilterData();
        }
    }
}
