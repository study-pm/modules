using HR.Data.Models;
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

namespace HR.Controls
{
    public class Filter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        public Geometry Icon {  get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        private bool isChecked;
        public bool IsChecked
        {
            get => isChecked;
            set
            {
                if (isChecked == value) return;
                isChecked = value;
                if (!value) AllChecked = false;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AllChecked));
            }
        }
        private ObservableCollection<FilterValue> values = new ObservableCollection<FilterValue>();
        public ObservableCollection<FilterValue> Values
        {
            get => values;
            set
            {
                if (values == value) return;
                // Unsubscribe for collection changes
                if (values != null)
                {
                    values.CollectionChanged -= Values_CollectionChanged;
                    foreach (var v in values)
                        v.PropertyChanged -= Value_PropertyChanged;
                }

                values = value;

                // Subscribe for collection changes
                if (values != null)
                {
                    values.CollectionChanged += Values_CollectionChanged;
                    foreach (var v in values)
                        v.PropertyChanged += Value_PropertyChanged;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(AllChecked));
            }
        }
        private void Values_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(AllChecked));
        }

        private void Value_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FilterValue.IsChecked))
            {
                OnPropertyChanged(nameof(AllChecked));
            }
        }
        public bool AllChecked
        {
            get => Values.All(x => x.IsChecked);
            set
            {
                foreach (var v in Values)
                    v.IsChecked = value;
                OnPropertyChanged();
            }
        }
        public Filter()
        {
            Values = new ObservableCollection<FilterValue>();
        }

    }
    public class FilterValue : INotifyPropertyChanged
    {
        private bool _isChecked;
        public int Id { get; set; }
        public string Title { get; set; }
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChecked)));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
    /// <summary>
    /// Interaction logic for AsideLeft.xaml
    /// </summary>
    public partial class AsideLeft : NavCtl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        public static readonly DependencyProperty InProgressProp =
            DependencyProperty.Register(nameof(IsInProgress), typeof(bool), typeof(AsideLeft), new PropertyMetadata(false));

        public bool IsInProgress
        {
            get { return (bool)GetValue(InProgressProp); }
            set { SetValue(InProgressProp, value); }
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
            }
        };
        public RelayCommand NavigateCommand { get; }
        public AsideLeft()
        {
            InitializeComponent();

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
        private async void Control_Loaded(object sender, RoutedEventArgs e)
        {
            await SetFilterData();
        }
        private void RadioButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var radioButton = sender as RadioButton;
            if (radioButton == null)
                return;

            // If the RadioButton is already checked, uncheck it and mark event as handled
            if (radioButton.IsChecked == true)
            {
                radioButton.IsChecked = false;

                // If your ViewModel binding is TwoWay, this will update IsChecked in VM as well
                e.Handled = true; // prevent default toggle behavior
            }
        }
    }
}
