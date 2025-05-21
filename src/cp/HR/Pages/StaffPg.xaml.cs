using HR.Data.Models;
using HR.Models;
using HR.Services;
using HR.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HR.Pages
{
    /// <summary>
    /// Interaction logic for StaffPg.xaml
    /// </summary>
    public partial class StaffPg : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        public ICommand PrintCommand { get; }
        // HashSet consists of Position ids
        private Dictionary<string, HashSet<int>> filterOptions = new Dictionary<string, HashSet<int>>
            {
                { "Management", new HashSet<int> { 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 20 } },
                { "Teachers", new HashSet<int> { 1 } },
                { "Pedagogues", new HashSet<int> { 2, 3, 4, 6, 18, 19 } },
                { "Tutors", new HashSet<int> { 5 } },
            };
        private NavigationService _navigationService;
        private ObservableCollection<Employee> staff;
        public ObservableCollection<Employee> Staff
        {
            get => staff;
            set
            {
                if (staff == value) return;
                staff = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsStaffEmpty));
                OnPropertyChanged(nameof(IsStaffNotEmpty));
            }
        }
        private ICollectionView staffView;
        public IEnumerable<string> FilterOptions => filterOptions.Keys;
        private int filteredCount;
        public int FilteredCount
        {
            get => filteredCount;
            set
            {
                if (filteredCount == value) return;
                filteredCount = value;
                OnPropertyChanged();
            }
        }
        public bool? IsStaffEmpty => Staff?.Count == 0;
        public bool? IsStaffNotEmpty => Staff?.Count > 0;
        private bool isGridView;
        public bool IsGridView
        {
            get => isGridView;
            set
            {
                isGridView = value;
                OnPropertyChanged();
                if (EmployeesDataGrid != null) ResetSorting();
            }
        }
        private string searchText;
        public string SearchText
        {
            get => searchText;
            set
            {
                if (searchText == value) return;
                searchText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSearchEmpty));
            }
        }
        public bool IsSearchEmpty
        {
            get => string.IsNullOrEmpty(SearchText);
        }
        private string selectedDepartmentFilter;
        public string SelectedDepartmentFilter
        {
            get => selectedDepartmentFilter;
            set
            {
                if (selectedDepartmentFilter == value) return;
                selectedDepartmentFilter = value;
                OnPropertyChanged();
                ApplyFilter(); // Auto applied on setting value
            }
        }
        public StaffPg()
        {
            InitializeComponent();
            DataContext = this;
            // @TODO: Read user preferences and set here
            IsGridView = true;

            PrintCommand = new RelayCommand(
                execute: _ => ExecutePrint(),
                canExecute: _ => Staff != null && Staff.Any()
            );

            // Привязка команды Copy к обработчику
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, CopyCommand_Executed, CopyCommand_CanExecute));

            // Назначение горячих клавиш Ctrl+C и Ctrl+P
            InputBindings.Add(new KeyBinding(ApplicationCommands.Copy, Key.C, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(new RoutedUICommand("Print", "Print", typeof(StaffPg)), Key.P, ModifierKeys.Control));

            // Subscribe to navigation events for the main frame
            _navigationService = MainWindow.frame.NavigationService;
            if (_navigationService != null)
            {
                _navigationService.Navigated += NavigationService_Navigated;
            }
        }
        private void ExecutePrint()
        {
            if (Staff != null)
            {
                ItemsControlHelper.PrintStaff(Staff);
            }
        }
        private void CopyCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = EmployeesDataGrid.SelectedItems.Count > 0;
        }

        private void CopyCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DataGridCommandsHelper.CopySelectedRowsToClipboard(EmployeesDataGrid);
        }

        private void PrintMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DataGridCommandsHelper.PrintDataGrid(EmployeesDataGrid);
        }
        private void CopyStaff_Click(object sender, RoutedEventArgs e)
        {
            ItemsControlHelper.CopyStaffToClipboard(Staff);
        }

        private void PrintStaff_Click(object sender, RoutedEventArgs e)
        {
            ItemsControlHelper.PrintStaff(Staff);
        }
        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var image = sender as Image;
            if (image == null) return;

            // Получаем родительский Grid, в котором находится Image и Popup
            var grid = VisualTreeHelper.GetParent(image);
            while (grid != null && !(grid is Grid))
            {
                grid = VisualTreeHelper.GetParent(grid);
            }
            if (grid == null) return;

            // Находим Popup внутри этого Grid
            Popup popup = null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(grid); i++)
            {
                var child = VisualTreeHelper.GetChild(grid, i);
                if (child is Popup p)
                {
                    popup = p;
                    break;
                }
            }
            if (popup == null) return;

            // Находим Image внутри Popup (тот, что показывает увеличенное изображение)
            var border = popup.Child as Border;
            if (border == null) return;

            var scrollViewer = border.Child as ScrollViewer;
            if (scrollViewer == null) return;

            var popupImage = scrollViewer.Content as Image;
            if (popupImage == null) return;

            // Устанавливаем источник изображения и открываем Popup
            popupImage.Source = image.Source;
            popup.IsOpen = true;
        }
        private async Task SetEmployees()
        {
            Staff = new ObservableCollection<Employee>(await Request.GetEmployees());
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Set employees collection
            await SetEmployees();
            // Get default view for the staff
            staffView = CollectionViewSource.GetDefaultView(Staff);
            if (staffView == null) return;
            // Set filter conditions (predicate)
            staffView.Filter = FilterStaff;
            // Update filtered items count
            UpdateFilteredCount();
        }
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_navigationService != null)
            {
                _navigationService.Navigated -= NavigationService_Navigated;
                _navigationService = null;
            }
        }
        private void NavigationService_Navigated(object sender, NavigationEventArgs e)
        {
            // Check if the current page
            if (e.Content != this) return;
            var navigationParameter = e.ExtraData;

            // Set and apply sorting criteria depending on the route param
            if (navigationParameter is string filterParam && filterOptions.ContainsKey(filterParam))
                SelectedDepartmentFilter = filterParam;
            else
                SelectedDepartmentFilter = null;
        }
        private void ApplyFilter()
        {
            staffView?.Refresh();
            UpdateFilteredCount();
        }
        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchText = string.Empty;
            ApplyFilter();
        }
        private bool FilterStaff(object obj)
        {
            if (!(obj is Employee emp)) return false;
            // Фильтрация по подразделению
            if (!string.IsNullOrEmpty(SelectedDepartmentFilter) && filterOptions.ContainsKey(SelectedDepartmentFilter))
            {
                var allowedDepartments = filterOptions[SelectedDepartmentFilter];
                // Проверяем, есть ли среди Staffs хотя бы один с нужным DepartmentId
                bool inDepartment = emp.Staffs != null && emp.Staffs.Any(staff => allowedDepartments.Contains(staff.PositionId));
                if (!inDepartment) return false;
            }

            // Фильтрация по поисковому тексту
            if (string.IsNullOrWhiteSpace(SearchText)) return true;

            var lowerSearch = SearchText.ToLower();

            // Предполагается, что у Employee есть свойства LastName, FirstName, MiddleName
            return (emp.Surname?.ToLower().Contains(lowerSearch) == true) ||
                   (emp.GivenName?.ToLower().Contains(lowerSearch) == true) ||
                   (emp.Patronymic?.ToLower().Contains(lowerSearch) == true);
        }
        private void ResetSorting()
        {
            if (EmployeesDataGrid == null || EmployeesDataGrid.Columns.Count == 0)
                return;
            // Убираем визуальные индикаторы сортировки
            foreach (var column in EmployeesDataGrid.Columns)
                column.SortDirection = null;

            // Получаем CollectionView для коллекции Staff
            var collectionView = CollectionViewSource.GetDefaultView(Staff);
            // Если используется ICollectionView для сортировки, очищаются сортировки в нем
            if (collectionView != null)
            {
                collectionView.SortDescriptions.Clear();
                collectionView.Refresh();
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
        }

        private void UpdateFilteredCount()
        {
            if (staffView == null)
            {
                FilteredCount = 0;
                return;
            }
            FilteredCount = staffView.Cast<object>().Count();
        }
    }
}
