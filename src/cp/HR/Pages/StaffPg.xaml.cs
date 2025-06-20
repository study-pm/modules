using HR.Controls;
using HR.Data.Models;
using HR.Models;
using HR.Services;
using HR.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
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
using static HR.Services.AppEventHelper;

namespace HR.Pages
{

    public class ActivityToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var fallbackBrush = Brushes.Gray;
            if (!(value is Position pos)) return fallbackBrush;

            string resourceKey;

            switch (pos.ActivityId)
            {
                case 0:
                    resourceKey = "ErrorBrush";         // Management
                    break;
                case 1:
                    resourceKey = "CsvBrush";           // Teachers
                    break;
                case 2:
                    resourceKey = "GreenDarkBrush";     // Educators
                    break;
                case 3:
                    resourceKey = "GreenMediumBrush";   // Caregivers
                    break;
                case 4:
                    resourceKey = "cyanDarkBrush";      // Tutors
                    break;
                case 5:
                    resourceKey = "InfoBrush";          // Support
                    break;
                case 6:
                    resourceKey = "OrangeRegularBrush"; // Service
                    break;
                case 7:
                    resourceKey = "AwaitingBrush";      // Other
                    break;
                default:
                    resourceKey = "greyDarkBrush";
                    break;
            }

            if (Application.Current.Resources.Contains(resourceKey))
            {
                return Application.Current.Resources[resourceKey] as Brush ?? fallbackBrush;
            }
            else
            {
                return fallbackBrush;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class DepartmentToPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.WriteLine("DepartmentToPathConverter");
            if (!(value is Department dep)) return null;
                
            switch (dep.Id)
            {
                case 1:
                    return Application.Current.TryFindResource("UserTieSolidPath") as Geometry;
                case 2:
                    return Application.Current.TryFindResource("BookSolidPath") as Geometry;
                case 3:
                    return Application.Current.TryFindResource("SquareRootAltSolidPath") as Geometry;
                case 4:
                    return Application.Current.TryFindResource("BookSolidPath") as Geometry;
                case 5:
                    return Application.Current.TryFindResource("LanguageSolidPath") as Geometry;
                case 6:
                    return Application.Current.TryFindResource("MicroscopeSolidPath") as Geometry;
                case 7:
                    return Application.Current.TryFindResource("GlobeSolidPath") as Geometry; 
                case 8:
                    return Application.Current.TryFindResource("PaletteSolidPath") as Geometry;
                case 9:
                    return Application.Current.TryFindResource("VoleyballSolidPath") as Geometry;
                case 10:
                    return Application.Current.TryFindResource("ClipboardListSolidPath") as Geometry;
                case 11:
                    return Application.Current.TryFindResource("TheaterMasksSolidPath") as Geometry;
                case 12:
                    return Application.Current.TryFindResource("HandsHelpingSolidPath") as Geometry;
                case 13:
                    return Application.Current.TryFindResource("FanSolidPath") as Geometry;
                case 14:
                    return Application.Current.TryFindResource("SwatchbookSolidPath") as Geometry;
                default:
                    return null;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Interaction logic for StaffPg.xaml
    /// </summary>
    public partial class StaffPg : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        private MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

        public ICommand AddCmd { get; }
        public ICommand DeleteCmd { get; }
        public ICommand ExportCmd { get; }
        public ICommand FilterCmd { get; }
        public ICommand NavigateCmd { get; }
        public ICommand PrintCmd { get; }
        public ICommand ResetFilterCmd { get; }
        public ICommand ResetSearchCmd { get; }

        private DataGridCellInfo? _lastRightClickedCell;
        private int _lastColumnIndex = -1;

        private NavigationService _navigationService;

        private ICollectionView CollectionView;
        private ObservableCollection<Employee> _dataCol;
        public ObservableCollection<Employee> DataCollection
        {
            get => _dataCol;
            set
            {
                if (_dataCol == value) return;
                _dataCol = value;
                OnPropertyChanged();
            }
        }

        private bool _isProgress;
        public bool IsProgress
        {
            get => _isProgress;
            set
            {
                if (_isProgress == value) return;
                _isProgress = value;
                OnPropertyChanged();
            }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value) return;
                _searchText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsResetFilter));
            }
        }

        private FilterParam _collectionFilter;
        public FilterParam CollectionFilter
        {
            get => _collectionFilter;
            set
            {
                if (_collectionFilter == value) return;
                _collectionFilter = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsResetFilter));
                Refresh();
            }
        }
        public int FilteredCount => CollectionView?.Cast<object>().Count() ?? 0;
        public bool IsResetFilter => !string.IsNullOrWhiteSpace(SearchText) || SelectedFilter != null || CollectionFilter != null;
        public bool IsTextSearch => !IsSelectedActivity && !IsSelectedDepartment || !IsSelectedPosition || !IsSelectedSubject;
        public bool IsSelectedActivity => SelectedFilter?.Name == "ActivityId";
        public bool IsSelectedDepartment => SelectedFilter?.Name == "DepartmentId";
        public bool IsSelectedPosition => SelectedFilter?.Name == "PositionId";
        public bool IsSelectedSubject => SelectedFilter?.Name == "SubjectId";
        public List<SelectionFilter> FilterValues { get; set; } = new List<SelectionFilter> {
            new SelectionFilter { Name = "FullName", Title = "ФИО" },
            new SelectionFilter { Name = "ActivityId", Title = "Деятельность" },
            new SelectionFilter { Name = "DepartmentId", Title = "Подразделение" },
            new SelectionFilter { Name = "PositionId", Title = "Должность" },
            new SelectionFilter { Name = "SubjectId", Title = "Предмет" },
        };
        private SelectionFilter _selectedFilter;
        public SelectionFilter SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                if (_selectedFilter == value) return;
                _selectedFilter = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsResetFilter));
                OnPropertyChanged(nameof(IsSelectedActivity));
                OnPropertyChanged(nameof(IsSelectedDepartment));
                OnPropertyChanged(nameof(IsSelectedPosition));
                OnPropertyChanged(nameof(IsSelectedSubject));
                OnPropertyChanged(nameof(IsTextSearch));
                // Reset values
                SelectedActivity = null;
                SelectedDepartment = null;
                SelectedPosition = null;
                SelectedSubject = null;
            }
        }
        // ComboBox lists
        private List<Employee> _employees;
        public List<Employee> Employees
        {
            get => _employees;
            set { _employees = value; OnPropertyChanged(); }
        }

        private List<ActivityItem> _activities;
        public List<ActivityItem> Activities
        {
            get => _activities;
            set { _activities = value; OnPropertyChanged(); }
        }
        private ActivityItem _selectedActivity;
        public ActivityItem SelectedActivity
        {
            get => _selectedActivity;
            set { _selectedActivity = value; OnPropertyChanged(); }
        }

        private List<Department> _departments;
        public List<Department> Departments
        {
            get => _departments;
            set { _departments = value; OnPropertyChanged(); }
        }

        private Department _selectedDepartment;
        public Department SelectedDepartment
        {
            get => _selectedDepartment;
            set { _selectedDepartment = value; OnPropertyChanged(); }
        }
        private List<Position> _positions;
        public List<Position> Positions
        {
            get => _positions;
            set { _positions = value; OnPropertyChanged(); }
        }
        private Position _selectedPosition;
        public Position SelectedPosition
        {
            get => _selectedPosition;
            set { _selectedPosition = value; OnPropertyChanged(); }
        }

        private List<Subject> _subjects;
        public List<Subject> Subjects
        {
            get => _subjects;
            set { _subjects = value; OnPropertyChanged(); }
        }
        private Subject _selectedSubject;
        public Subject SelectedSubject
        {
            get => _selectedSubject;
            set { _selectedSubject = value; OnPropertyChanged(); }
        }

        public StaffPg()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += Page_Loaded;

            AddCmd = new RelayCommand(
                _ => MainWindow.frame.Navigate(new EmployeePg()),
                _ => !IsProgress
            );
            DeleteCmd = new RelayCommand(
                execute: param =>
                {
                    // Get list for deletion
                    List<Employee> itemsToDelete = null;

                    if (param is Employee singleItem)
                        itemsToDelete = new List<Employee> { singleItem };
                    else if (param is IEnumerable<Employee> manyItems)
                        itemsToDelete = manyItems.ToList();
                    else if (param is IList list && list.Count > 0 && list[0] is Employee)
                        itemsToDelete = list.Cast<Employee>().ToList();

                    if (itemsToDelete == null || itemsToDelete.Count == 0)
                        return;

                    // Confirmation
                    string msg = itemsToDelete.Count == 1
                        ? $"Вы действительно хотите удалить запись \"{itemsToDelete[0].FullName}\"?"
                        : $"Вы действительно хотите удалить выбранные записи ({itemsToDelete.Count})?";
                    var result = MessageBox.Show(
                        msg,
                        "Подтверждение удаления",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning,
                        MessageBoxResult.No
                    );

                    if (result != MessageBoxResult.Yes)
                    {
                        RaiseAppEvent(new AppEventArgs
                        {
                            Category = EventCategory.Data,
                            Name = "Delete",
                            Op = 3,
                            Scope = "Сотрудники",
                            Type = EventType.Cancel,
                            Message = "Удаление отменено",
                            Details = "Отменено пользователем"
                        });
                        return;
                    }

                    DeleteItems(itemsToDelete);
                },
                canExecute: param =>
                {
                    return !IsProgress;
                }
            );
            FilterCmd = new RelayCommand(
                execute: param =>
                {
                    DataGrid dg = param as DataGrid ?? dataGrid;
                    DataGridCellInfo? cellInfo = null;

                    // MessageBox.Show("FilterCmd");
                    // Search button click
                    if (param == null)
                    {
                        if (SelectedFilter == null)
                        {
                            MessageBox.Show("Выберите критерий поиска!");
                            return;
                        }
                        object searchValue = GetSearchValue();
                        // MessageBox.Show($"searchValue {searchValue}");
                        CollectionFilter = new FilterParam(SelectedFilter.Name, searchValue);
                        SyncMenuFilters();

                        string valueStr = CollectionFilter.Value?.ToString();
                        if (SelectedFilter != null && string.IsNullOrWhiteSpace(valueStr))
                        {
                            CollectionFilter.Name = SelectedFilter.Name;
                            SyncMenuFilters();
                        }
                        return;
                    }

                    // Right mouse button click
                    if (_lastRightClickedCell != null)
                    {
                        cellInfo = _lastRightClickedCell;
                    }
                    // Keyboard shortcut activation
                    else if (dg != null && dg.CurrentCell != null && dg.CurrentCell.Column is DataGridBoundColumn)
                    {
                        cellInfo = dg.CurrentCell;
                    }

                    if (cellInfo != null)
                    {
                        var item = cellInfo.Value.Item;
                        string propertyName = null;
                        if (cellInfo.Value.Column is DataGridBoundColumn boundColumn)
                        {
                            var binding = boundColumn.Binding as Binding;
                            if (binding != null)
                                propertyName = binding.Path.Path;
                        }
                        else if (cellInfo.Value.Column is DataGridTemplateColumn templateColumn)
                        {
                            // Define property type based on column index
                            int colIndex = dataGrid.Columns.IndexOf(cellInfo.Value.Column);
                            if (colIndex == 1) propertyName = "Surname";
                            if (colIndex == 2) propertyName = "Givenname";
                            if (colIndex == 3) propertyName = "Patronymic";
                        }

                        if (!string.IsNullOrEmpty(propertyName))
                        {
                            var value = item.GetType().GetProperty(propertyName)?.GetValue(item, null);
                            CollectionFilter = new FilterParam(propertyName, value);
                            SyncMenuFilters();
                        }
                    }
                    // Special treatment for Ctrl+F on DataGridTemplateColumn
                    else
                    {
                        var selectedItem = dg.SelectedItem;
                        if (selectedItem != null && _lastColumnIndex > 0)
                        {
                            string propertyName = null;
                            switch (_lastColumnIndex)
                            {
                                case 1: propertyName = "Surname"; break;
                                case 2: propertyName = "GivenName"; break;
                                case 3: propertyName = "Patronymic"; break;
                            }
                            if (!string.IsNullOrEmpty(propertyName))
                            {
                                var value = selectedItem.GetType().GetProperty(propertyName)?.GetValue(selectedItem, null);
                                CollectionFilter = new FilterParam(propertyName, value);
                                SyncMenuFilters();
                            }
                        }
                    }
                },
                _ => !IsProgress
            );
            NavigateCmd = new RelayCommand(
                execute: param =>
                {
                    Employee employee = null;

                    if (param is Employee singleItem)
                    {
                        employee = singleItem;
                    }
                    else if (param is IEnumerable<Employee> manyItems)
                    {
                        var items = manyItems.ToList();
                        employee = items.Count > 0 && items[0] is Employee ? items[0] : null;
                    }
                    else if (param is IList list && list.Count > 0 && list[0] is Employee)
                        employee = list[0] as Employee;

                    if (employee == null)
                        return;

                    MainWindow.frame.Navigate(new EmployeePg(employee));
                },
                canExecute: param => !IsProgress
            );
            ResetFilterCmd = new RelayCommand(
                _ => ResetFilter(),
                _ => !IsProgress && IsResetFilter
            );
            ResetSearchCmd = new RelayCommand(
                _ => SearchText = null,
                _ => !string.IsNullOrWhiteSpace(SearchText)
            );

            // @TODO: Read user preferences and set here
            // IsGridView = true;

            /*
            PrintCommand = new RelayCommand(
                execute: _ => ExecutePrint(),
                canExecute: _ => Staff != null && Staff.Any()
            );

            // Привязка команды Copy к обработчику
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, CopyCommand_Executed, CopyCommand_CanExecute));

            // Назначение горячих клавиш Ctrl+C и Ctrl+P
            InputBindings.Add(new KeyBinding(ApplicationCommands.Copy, Key.C, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(new RoutedUICommand("Print", "Print", typeof(StaffPg)), Key.P, ModifierKeys.Control));
            */

            // Subscribe to navigation events for the main frame
            _navigationService = MainWindow.frame.NavigationService;
            if (_navigationService != null)
            {
                _navigationService.Navigated += NavigationService_Navigated;
            }
        }

        private async void DeleteItems(List<Employee> items)
        {
            var (cat, name, op, scope) = (EventCategory.Data, "Delete", 3, "Сотрудники");
            try
            {
                IsProgress = true;
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat,
                    Name = name,
                    Op = op,
                    Scope = scope,
                    Type = EventType.Progress,
                    Message = "Удаление данных"
                });
                Request.ctx.Employees.RemoveRange(items);
                await Request.ctx.SaveChangesAsync();
                // Delete from the collection in memory
                foreach (var item in items)
                    DataCollection.Remove(item);
                Refresh();
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat,
                    Name = name,
                    Op = op,
                    Scope = scope,
                    Type = EventType.Success,
                    Message = "Данные успешно удалены"
                });
                MessageBox.Show($"Данные успешно удалены.", "Успешное удаление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc, "StaffPg");
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat,
                    Name = name,
                    Op = op,
                    Scope = scope,
                    Type = EventType.Error,
                    Message = "Ошибка удаления данных",
                    Details = exc.Message
                });
                MessageBox.Show($"Ошибка удаления данных: {exc.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProgress = false;
            }
        }
        private string GetFilterPropName(string name)
        {
            switch (name)
            {
                case "Сотрудники":
                    return "ActivityId";
                case "Подразделения":
                    return "DepartmentId";
                case "Должности":
                    return "PositionId";
                case "Предметы":
                    return "SubjectId";
                default:
                    return "";
            }
        }
        private object GetSearchValue()
        {
            if (SelectedFilter == null)
                return SearchText;

            switch (SelectedFilter.Name)
            {
                case "ActivityId":
                    return SelectedActivity?.Id;
                case "DepartmentId":
                    return SelectedDepartment?.Id;
                case "PositionId":
                    return SelectedPosition?.Id;
                case "SubjectId":
                    return SelectedSubject?.Id;
                default:
                    // Return search text for other filters
                    return SearchText;
            }
        }
        private void Refresh()
        {
            dataGrid?.Items.Refresh();                  // update numbering
            CollectionView?.Refresh();                  // refresh collection view
            OnPropertyChanged(nameof(FilteredCount));   // update total count
        }
        private void ResetFilter()
        {
            SearchText = null;
            SelectedFilter = null;
            CollectionFilter = null;
        }
        private async Task<bool> SaveData(Employee item)
        {
            var (cat, name, op, scope) = (EventCategory.Data, item.Id == 0 ? "Create" : "Update", item.Id == 0 ? 0 : 2, "Сотрудники");
            try
            {
                IsProgress = true;
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat,
                    Name = name,
                    Op = op,
                    Scope = scope,
                    Type = EventType.Progress,
                    Message = (op == 0 ? "Добавление" : "Сохранение") + "данных"
                });
                await Services.Request.ctx.SaveChangesAsync();
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat,
                    Name = name,
                    Op = op,
                    Scope = scope,
                    Type = EventType.Success,
                    Message = "Данные успешно сохранены"
                });
                Refresh();
                return true;
            }
            catch (Exception exc)
            {
                if (Request.ctx.Entry(item).State == EntityState.Added)
                {
                    Request.ctx.Entry(item).State = EntityState.Detached;
                }
                if (item.Id == 0)
                {
                    // Remove object from collection to stop displaying it in UI
                    DataCollection.Remove(item);
                }
                Debug.WriteLine(exc, "StaffPg");
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat,
                    Name = name,
                    Op = op,
                    Scope = scope,
                    Type = EventType.Error,
                    Message = $"Ошибка {(op == 0 ? "добавления" : "сохранения")} данных",
                    Details = exc.Message
                });
                MessageBox.Show($"Ошибка {(op == 0 ? "добавления" : "сохранения")} данных: {exc.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return true;
            }
            finally
            {
                IsProgress = false;
            }
        }
        private async Task SetData()
        {
            Activities = ActivityHelper.GetAllActivities();
            var (cat, name, op, scope) = (EventCategory.Data, "Read", 2, "Сотрудники");
            try
            {
                IsProgress = true;
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat,
                    Name = name,
                    Op = op,
                    Scope = scope,
                    Type = EventType.Progress,
                    Message = "Загрузка данных"
                });
                // Run tasks in parallel
                var employeesTask = Request.LoadEmployees(true); // Return entities of current context !Improtant for tracking context changes
                var departmentsTask = Request.LoadDepartments();
                var positionsTask = Request.LoadPositions();
                var subjectsTask = Request.LoadSubjects();

                await Task.WhenAll(employeesTask, departmentsTask, positionsTask, subjectsTask);

                // Update UI via Dispatcher
                await Dispatcher.InvokeAsync(() =>
                {
                    DataCollection = new ObservableCollection<Employee>(employeesTask.Result);
                    CollectionView = CollectionViewSource.GetDefaultView(DataCollection);
                    Departments = departmentsTask.Result;
                    Positions = positionsTask.Result;
                    Subjects = subjectsTask.Result;

                    CollectionView.Filter = obj => FilterHelper.FilterByValue(obj, CollectionFilter);
                    CollectionView.Refresh();
                    SyncSearchFilters();
                });
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat,
                    Name = name,
                    Op = op,
                    Scope = scope,
                    Type = EventType.Success,
                    Message = "Данные успешно загружены"
                });
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc, "ClassesPg");
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat,
                    Name = name,
                    Op = op,
                    Scope = scope,
                    Type = EventType.Error,
                    Message = $"Ошибка извлечения данных",
                    Details = exc.Message
                });
                MessageBox.Show($"Ошибка извлечения данных: {exc.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProgress = false;
            }
        }
        private void SyncMenuFilters()
        {
            var items = CollectionView?.Cast<Employee>().ToList();
            List<int> uniqueActivityIds = items?
                .Where(emp => emp.Staffs != null)
                .SelectMany(emp => emp.Staffs)
                .Where(staff => staff.Position != null)
                .Select(staff => staff.Position.ActivityId)
                .Distinct()
                .ToList();

            List<int> uniqueDepartmentIds = items?
                .Where(emp => emp.Staffs != null)
                .SelectMany(emp => emp.Staffs)
                .Select(staff => staff.DepartmentId)
                .Distinct()
                .ToList();

            List<int> uniquePositionIds = items?
                .Where(emp => emp.Staffs != null)
                .SelectMany(emp => emp.Staffs)
                .Select(staff => staff.PositionId)
                .Distinct()
                .ToList();

            List<int> uniqueSubjectIds = items?
                .Where(emp => emp.Staffs != null)
                .SelectMany(emp => emp.Staffs)
                .Where(staff => staff.Assignments != null)
                .SelectMany(staff => staff.Assignments)
                .Select(assignment => assignment.SubjectId)
                .Distinct()
                .ToList();


            if (!(mainWindow?.MenuVM is MenuViewModel) || !(mainWindow.MenuVM.Filters is IEnumerable<MenuFilter>)) return;

            // MessageBox.Show($"SyncMenuFilters(): {CollectionFilter?.Name} {SelectedFilter?.Name}");

            var activityFilter = mainWindow.MenuVM.Filters.FirstOrDefault(f => f.Title == "Сотрудники");
            var departmentFilter = mainWindow.MenuVM.Filters.FirstOrDefault(f => f.Title == "Подразделения");
            var positionFilter = mainWindow.MenuVM.Filters.FirstOrDefault(f => f.Title == "Должности");
            var subjectFilter = mainWindow.MenuVM.Filters.FirstOrDefault(f => f.Title == "Предметы");

            // Sync by Activity
            if (SelectedFilter?.Name == "ActivityId" && activityFilter != null && activityFilter.Values != null && uniqueActivityIds != null)
            {
                activityFilter.IsChecked = true;
                // Синхронизируем состояние IsChecked для каждого значения фильтра
                foreach (var val in activityFilter.Values)
                {
                    val.IsChecked = uniqueActivityIds.Contains(val.Id);
                }
            }

            // Sync by Department
            if (SelectedFilter?.Name == "DepartmentId" && departmentFilter != null && departmentFilter.Values != null && uniqueDepartmentIds != null)
            {
                departmentFilter.IsChecked = true;
                foreach (var val in departmentFilter.Values)
                {
                    val.IsChecked = uniqueDepartmentIds.Contains(val.Id);
                }
            }
            // Sync by Position
            if (SelectedFilter?.Name == "PositionId" && positionFilter != null && positionFilter.Values != null && uniquePositionIds != null)
            {
                positionFilter.IsChecked = true;
                foreach (var val in positionFilter.Values)
                {
                    val.IsChecked = uniquePositionIds.Contains(val.Id);
                }
            }

            // Sync by Subject
            if (SelectedFilter?.Name == "SubjectId" && subjectFilter != null && subjectFilter.Values != null && uniqueSubjectIds != null)
            {
                subjectFilter.IsChecked = true;
                foreach (var val in subjectFilter.Values)
                {
                    val.IsChecked = uniqueSubjectIds.Contains(val.Id);
                }
            }
        }
        private void SyncSearchFilters()
        {
            if (CollectionFilter == null) return;
            switch(CollectionFilter.Name)
            {
                case "Surname":  case "GivenName": case "FirstName":
                    SelectedFilter = FilterValues.FirstOrDefault(f => f.Name == "FullName");
                    SearchText = CollectionFilter.Value.ToString();
                    break;
                case "ActivityId":
                    SelectedFilter = FilterValues.FirstOrDefault(f => f.Name == "ActivityId");
                    if (CollectionFilter.Value is int singleActivityId)
                        SelectedActivity = Activities?.FirstOrDefault(a => a.Id == singleActivityId);
                    else if (CollectionFilter.Value is IEnumerable<int> activityIds && activityIds.Any())
                        SelectedActivity = Activities?.FirstOrDefault(a => a.Id == activityIds.First());
                    else
                        SelectedActivity = null;
                    break;
                case "DepartmentId":
                    SelectedFilter = FilterValues.FirstOrDefault(f => f.Name == "DepartmentId");
                    if (CollectionFilter.Value is int singleDepId)
                        SelectedDepartment = Departments?.FirstOrDefault(a => a.Id == singleDepId);
                    else if (CollectionFilter.Value is IEnumerable<int> depIds && depIds.Any())
                        SelectedDepartment = Departments?.FirstOrDefault(a => a.Id == depIds.First());
                    else
                        SelectedDepartment = null;
                    break;
                case "PositionId":
                    SelectedFilter = FilterValues.FirstOrDefault(f => f.Name == "PositionId");
                    if (CollectionFilter.Value is int singlePosId)
                        SelectedPosition = Positions?.FirstOrDefault(a => a.Id == singlePosId);
                    else if (CollectionFilter.Value is IEnumerable<int> posIds && posIds.Any())
                        SelectedPosition = Positions?.FirstOrDefault(a => a.Id == posIds.First());
                    else
                        SelectedPosition = null;
                    break;
                case "SubjectId":
                    SelectedFilter = FilterValues.FirstOrDefault(f => f.Name == "SubjectId");
                    if (CollectionFilter.Value is int singleSubjectId)
                        SelectedSubject = Subjects?.FirstOrDefault(s => s.Id == singleSubjectId);
                    else if (CollectionFilter.Value is IEnumerable<int> subjectIds && subjectIds.Any())
                        SelectedSubject = Subjects?.FirstOrDefault(s => s.Id == subjectIds.First());
                    else
                        SelectedSubject = null;
                    break;
                default:
                    SearchText = string.Empty;
                    break;
            }
            /*
            if (CollectionFilter.Name == "Title")
            {
                SelectedFilter = FilterValues.FirstOrDefault(f => f.Name == "Title");
                SearchText = CollectionFilter.Value.ToString();
            }
            else if (CollectionFilter.Name == "EmployeeId")
            {
                SelectedFilter = FilterValues.FirstOrDefault(f => f.Name == "EmployeeId");
                if (CollectionFilter.Value is int employeeId)
                {
                    SearchText = Employees.FirstOrDefault(x => x.Id == employeeId)?.FullName ?? string.Empty;
                }
                else if (CollectionFilter.Value is IEnumerable<int> employeeIds)
                {
                    // If value is Id list then may take frist or join the names
                    var firstId = employeeIds.FirstOrDefault();
                    SearchText = Employees.FirstOrDefault(x => x.Id == firstId)?.FullName ?? string.Empty;
                }
                else
                {
                    SearchText = string.Empty;
                }
            }
            */
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            // Numbering from 1
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var dg = sender as DataGrid;
            var item = dg?.SelectedItem as Employee;

            if (e.Key == Key.Delete && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (dg?.SelectedItems != null && dg.SelectedItems.Count > 0)
                {
                    // Convert to AppEventArgs (or IList)
                    var items = dg.SelectedItems.Cast<Employee>().ToList();
                    if (DeleteCmd.CanExecute(items))
                    {
                        DeleteCmd.Execute(items);
                        e.Handled = true;
                    }
                }
            }
            if (e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (item != null && ExportCmd.CanExecute(3))
                {
                    ExportCmd.Execute(3);
                    e.Handled = true;
                }
            }
            if (e.Key == Key.E && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (dg?.SelectedItems != null && dg.SelectedItems.Count > 0)
                {
                    // Convert to AppEventArgs (or IList)
                    var items = dg.SelectedItems.Cast<Employee>().ToList();
                    if (NavigateCmd.CanExecute(items.Last()))
                    {
                        NavigateCmd.Execute(items);
                        e.Handled = true;
                    }
                }
            }
            if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (item != null && FilterCmd.CanExecute(item))
                {
                    _lastRightClickedCell = null; // Invalidate last right clicked cell
                    FilterCmd.Execute(dataGrid);
                    e.Handled = true;
                }
            }
            if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Shift)
            {
                if (ResetFilterCmd.CanExecute(null))
                {
                    ResetFilterCmd.Execute(null);
                    e.Handled = true;
                }
            }
            if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (AddCmd.CanExecute(null))
                {
                    AddCmd.Execute(null);
                    e.Handled = true;
                }
            }
            if (e.Key == Key.P && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (item != null && PrintCmd.CanExecute(null))
                {
                    PrintCmd.Execute(null);
                    e.Handled = true;
                }
            }
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (item != null && ExportCmd.CanExecute(1))
                {
                    ExportCmd.Execute(1);
                    e.Handled = true;
                }
            }
        }
        private void DataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var depObj = (DependencyObject)e.OriginalSource;
            while (depObj != null && !(depObj is DataGridCell))
                depObj = VisualTreeHelper.GetParent(depObj);

            if (depObj is DataGridCell cell)
            {
                var dataGrid = sender as DataGrid;
                if (dataGrid != null)
                {
                    _lastColumnIndex = dataGrid.Columns.IndexOf(cell.Column);
                }
            }
        }
        private void DataGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var depObj = (DependencyObject)e.OriginalSource;
            // Iteration up the tree, regarding the fact that Run is not a Visual
            while (depObj != null && !(depObj is DataGridCell))
            {
                if (depObj is Run)
                    depObj = LogicalTreeHelper.GetParent(depObj);
                else
                    depObj = VisualTreeHelper.GetParent(depObj);
            }

            if (depObj is DataGridCell cell)
            {
                var dataGrid = (DataGrid)sender;
                _lastRightClickedCell = new DataGridCellInfo(cell);
            }
            else
            {
                _lastRightClickedCell = null;
            }
        }
        private void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            var item = e.Row.Item as Employee;
            if (item == null) return;

            if (e.EditAction == DataGridEditAction.Commit)
            {
                Dispatcher.BeginInvoke(
                    (Action)(async () =>  // Explicit Action delegate type declaration
                    {
                        dataGrid.CommitEdit(DataGridEditingUnit.Row, true);

                       

                        if (string.IsNullOrWhiteSpace(item.Surname))
                        {
                            MessageBox.Show("Фамилия не может отсутствовать", "Неверные данные", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            // Return focus and edit state
                            dataGrid.Focus();
                            dataGrid.SelectedItem = item;
                            dataGrid.CurrentCell = new DataGridCellInfo(item, dataGrid.Columns[1]);
                            dataGrid.BeginEdit();
                            return;
                        }
                        if (string.IsNullOrWhiteSpace(item.GivenName))
                        {
                            MessageBox.Show("Имя не может отсутствовать", "Неверные данные", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            // Return focus and edit state
                            dataGrid.Focus();
                            dataGrid.SelectedItem = item;
                            dataGrid.CurrentCell = new DataGridCellInfo(item, dataGrid.Columns[2]);
                            dataGrid.BeginEdit();
                            return;
                        }
                        
                        // Get entity state
                        var entry = Services.Request.ctx.Entry(item);
            
                        // Add if entity is detached
                        if (entry.State == EntityState.Detached)
                        {
                            Services.Request.ctx.Employees.Attach(item);
                            //entry.State = EntityState.Modified; // Explicitely set as modified
                        }

                        // Check state
                        bool isModified = entry.State == EntityState.Modified;

                        if (!isModified) return;

                        await SaveData(item);
                    }),
                    System.Windows.Threading.DispatcherPriority.Background); // Приоритет передается вторым аргументом
            }
        }
        private void DataGridColumnHeader_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (sender is DataGridColumnHeader header && header.ContextMenu != null)
            {
                foreach (MenuItem item in header.ContextMenu.Items)
                {
                    if (int.TryParse(item.Tag?.ToString(), out int columnIndex))
                    {
                        var column = dataGrid.Columns[columnIndex];
                        item.IsChecked = column.Visibility == Visibility.Visible;
                    }
                }
            }
        }
        private void DataGridColumnHeaderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && int.TryParse(menuItem.Tag?.ToString(), out int columnIndex))
            {
                var column = dataGrid.Columns[columnIndex];
                // Toggle visibility based on checkbox state
                column.Visibility = menuItem.IsChecked ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        private void NavigationService_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Content != this) return;
            var navigationParameter = e.ExtraData;

            if (navigationParameter == null) return;
            if (!(navigationParameter is MenuFilter filter)) return;

            string filterName = filter.Name;
            List<int> filterValues = filter.Values.Where(fv => fv.IsChecked).Select(item => item.Id).ToList();
            //MessageBox.Show($"{filter.Title} {GetFilterPropName(filter.Title)} {filterValues.Count.ToString()}");
            CollectionFilter = new FilterParam(GetFilterPropName(filter.Title), filterValues);
            //MessageBox.Show("OK");
            //CollectionView?.Refresh();
            SyncSearchFilters();
        }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await SetData();
        }
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_navigationService != null)
            {
                _navigationService.Navigated -= NavigationService_Navigated;
                _navigationService = null;
            }
        }

















        public ICommand PrintCommand { get; }
        // HashSet consists of Position ids
        private Dictionary<string, HashSet<int>> filterOptions = new Dictionary<string, HashSet<int>>
            {
                { "Management", new HashSet<int> { 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 20 } },
                { "Teachers", new HashSet<int> { 1 } },
                { "Pedagogues", new HashSet<int> { 2, 3, 4, 6, 18, 19 } },
                { "Tutors", new HashSet<int> { 5 } },
            };
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
        
        public bool? IsStaffEmpty => Staff?.Count == 0;
        public bool? IsStaffNotEmpty => Staff?.Count > 0;
        /*
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
        */
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
        
        /*
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
        */
        private void EnlargeImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn)) return;

            // Search for the thumb Image inside the Button
            var image = btn.Content as Image;
            if (image == null) return;

            // Search for the parent Grid
            DependencyObject grid = VisualTreeHelper.GetParent(btn);
            while (grid != null && !(grid is Grid))
            {
                grid = VisualTreeHelper.GetParent(grid);
            }
            if (grid == null) return;

            // Find a Popup inside the Grid
            Popup popup = null;
            int childrenCount = VisualTreeHelper.GetChildrenCount(grid);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(grid, i);
                if (child is Popup p)
                {
                    popup = p;
                    break;
                }
            }
            if (popup == null) return;

            // Search for a full-scale Image inside the Popup
            if (!(popup.Child is Border border)) return;
            if (!(border.Child is ScrollViewer scrollViewer)) return;
            if (!(scrollViewer.Content is Image popupImage)) return;

            // Set image source and open Popup
            popupImage.Source = image.Source;
            popup.IsOpen = true;
        }
        private async Task SetEmployees()
        {
            Staff = new ObservableCollection<Employee>(await Request.GetEmployees());
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
        /*
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
        */

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
        }

        private void UpdateFilteredCount()
        {
            if (staffView == null)
            {
                //FilteredCount = 0;
                return;
            }
            //FilteredCount = staffView.Cast<object>().Count();
        }
    }
}
