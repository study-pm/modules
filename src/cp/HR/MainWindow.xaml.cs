using HR.Controls;
using HR.Pages;
using HR.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
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
                Icon = (Geometry)Application.Current.FindResource("UsersSolidPath"),
                Name = "Виды деятельности",
                Title = "Сотрудники",
                PageUri = "Pages/StaffPg.xaml",
                Values = new ObservableCollection<FilterValue>
                {
                    new FilterValue { Id = 1, Title = "Администрация" },
                    new FilterValue { Id = 2, Title = "Учителя" },
                    new FilterValue { Id = 3, Title = "Преподаватели" },
                    new FilterValue { Id = 4, Title = "Воспитатели" },
                }
            },
            new MenuFilter()
            {
                Icon = (Geometry)Application.Current.FindResource("SitemapSolidPath"),
                Name = "Структурные подразделения",
                Title = "Подразделения",
                PageUri = "Pages/StaffPg.xaml",
                Values = new ObservableCollection<FilterValue>() // initialize empty collection
            },
            new MenuFilter()
            {
                Icon = (Geometry)Application.Current.FindResource("BookOpenSolidPath"),
                Name = "Учебные предметы",
                Title = "Предметы",
                PageUri = "Pages/StaffPg.xaml",
                Values = new ObservableCollection<FilterValue>() // initialize empty collection
            },
            new MenuFilter()
            {
                Icon = (Geometry)Application.Current.FindResource("BookOpenSolidPath"),
                Name = "Классное руководство",
                Title = "Классы",
                PageUri = "Pages/ClassesPg.xaml",
                Values = new ObservableCollection<FilterValue>
                {
                    new FilterValue { Id = 1, Title = "1 параллель" },
                    new FilterValue { Id = 2, Title = "2 параллель" },
                    new FilterValue { Id = 3, Title = "3 пареллель" },
                    new FilterValue { Id = 4, Title = "4 параллель" },
                    new FilterValue { Id = 5, Title = "5 параллель" },
                    new FilterValue { Id = 6, Title = "6 параллель" },
                    new FilterValue { Id = 7, Title = "7 параллель" },
                    new FilterValue { Id = 8, Title = "8 параллель" },
                    new FilterValue { Id = 9, Title = "9 параллель" },
                    new FilterValue { Id = 10, Title = "10 параллель" },
                    new FilterValue { Id = 11, Title = "11 параллель" }
                }
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
            var getSubjTask = Task.Run(async () =>
            {
                using (var ctx = new HR.Data.Models.HREntities())
                {
                    return await ctx.Subjects.Select(x => new FilterValue { Id = x.Id, Title = x.Title }).OrderBy(x => x.Title).ToListAsync();
                }
            });

            // Await both tasks in parallel
            await Task.WhenAll(getDptTask, getSubjTask);

            // Process results
            //var departments = departmentsTask.Result;
            //var subjects = subjectsTask.Result;

            IsInProgress = false;

            Filters[1].Values = new ObservableCollection<FilterValue>(getDptTask.Result);
            Filters[2].Values = new ObservableCollection<FilterValue>(getSubjTask.Result);
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
        private void UpdateVisibility(double width)
        {
            if (width < NarrowWidth)
            {
                SetLeftSide(false);
                SetRightSide(false);
                TopHeader.SetNarrowMode(true);
            }
            else if (width < MediumWidth)
            {
                SetLeftSide(false);
                SetRightSide(true);
                TopHeader.SetNarrowMode(false);
            }
            else
            {
                SetLeftSide(true);
                SetRightSide(true);
                TopHeader.SetNarrowMode(false);
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await MenuVM.SetFilterData();
        }
    }
}
