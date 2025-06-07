using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows;
using HR.Utilities;
using PdfSharp.Pdf.Filters;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Data.Entity;
using HR.Pages;

namespace HR.Controls
{
    public class NavigationData : DependencyObject
    {
        public static readonly DependencyProperty ParameterProperty =
            DependencyProperty.Register("Parameter", typeof(object), typeof(NavigationData));

        public object Parameter
        {
            get => GetValue(ParameterProperty);
            set => SetValue(ParameterProperty, value);
        }

        // Остальные свойства
        public string Uri { get; set; }
        public string Name { get; set; }
    }
    public abstract class NavCtl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register(nameof(CurrentPage), typeof(string), typeof(NavCtl), new PropertyMetadata(null));

        public static readonly DependencyProperty InProgressProp =
            DependencyProperty.Register(nameof(IsInProgress), typeof(bool), typeof(NavCtl), new PropertyMetadata(false));

        public bool IsInProgress
        {
            get { return (bool)GetValue(InProgressProp); }
            set { SetValue(InProgressProp, value); }
        }
        public string CurrentPage
        {
            get => (string)GetValue(CurrentPageProperty);
            set => SetValue(CurrentPageProperty, value);
        }
        public static readonly DependencyProperty PageParamProperty =
            DependencyProperty.Register(nameof(PageParam), typeof(string), typeof(NavCtl), new PropertyMetadata(null));

        public string PageParam
        {
            get => (string)GetValue(PageParamProperty);
            set => SetValue(PageParamProperty, value);
        }
        public ObservableCollection<Filter> Filters { get; } = new ObservableCollection<Filter>()
        {
            new Filter()
            {
                Icon = (Geometry)Application.Current.FindResource("UsersSolidPath"),
                Name = "Виды деятельности",
                Title = "Сотрудники",
                Values = new ObservableCollection<FilterValue>
                {
                    new FilterValue { Id = 1, Title = "Администрация" },
                    new FilterValue { Id = 2, Title = "Учителя" },
                    new FilterValue { Id = 3, Title = "Преподаватели" },
                    new FilterValue { Id = 4, Title = "Воспитатели" },
                }
            },
            new Filter()
            {
                Icon = (Geometry)Application.Current.FindResource("SitemapSolidPath"),
                Name = "Структурные подразделения",
                Title = "Подразделения",
                Values = new ObservableCollection<FilterValue>() // initialize empty collection
            },
            new Filter()
            {
                Icon = (Geometry)Application.Current.FindResource("BookOpenSolidPath"),
                Name = "Учебные предметы",
                Title = "Предметы",
                Values = new ObservableCollection<FilterValue>() // initialize empty collection
            },
            new Filter()
            {
                Icon = (Geometry)Application.Current.FindResource("BookOpenSolidPath"),
                Name = "Классное руководство",
                Title = "Классы",
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
        public RelayCommand NavigateCommand { get; }
        protected NavCtl()
        {
            Loaded += Page_Loaded;
            this.DataContext = this;

            foreach (Filter filter in Filters)
                filter.PropertyChanged += Filter_PropertyChanged;

            NavigateCommand = new RelayCommand(
                execute: param =>
                {
                    if (param is NavigationData navData)
                    {
                        MainWindow.frame.Navigate(new Uri(navData.Uri, UriKind.Relative), navData.Parameter);
                    }
                },
                canExecute: param => param is NavigationData
            );
        }
        private async Task SetFilterData()
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
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (MainWindow.frame == null) return;
            MainWindow.frame.Navigated += Nav_Navigated;
        }

        private void Nav_Navigated(object sender, NavigationEventArgs e)
        {
            // Имя типа страницы
            CurrentPage = e.Content.GetType().Name;
            PageParam = e.ExtraData as string;
            if (e.Content is ClassesPg classesPage)
            {
                classesPage.FilterParam = e.ExtraData as FilterValue; // добавить свойство FilterParam в ClassesPg
            }
        }
    }
}
