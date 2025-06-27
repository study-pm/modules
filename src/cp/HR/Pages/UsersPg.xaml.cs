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
    /// <summary>
    /// Interaction logic for UsersPg.xaml
    /// </summary>
    public partial class UsersPg : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        public ICommand DeleteCmd { get; }
        public ICommand ExportCmd { get; }
        public ICommand FilterCmd { get; }
        public ICommand NavigateCmd { get; }
        public ICommand PrintCmd { get; }
        public ICommand ResetFilterCmd { get; }
        public ICommand ResetSearchCmd { get; }

        private DataGridCellInfo? _lastRightClickedCell;
        private int _lastColumnIndex = -1;

        private ICollectionView CollectionView;
        private ObservableCollection<HR.Data.Models.User> _dataCol;
        public ObservableCollection<HR.Data.Models.User> DataCollection
        {
            get => _dataCol;
            set
            {
                if (_dataCol == value) return;
                _dataCol = value;
                OnPropertyChanged();
            }
        }
        private List<Employee> _employees;
        public List<Employee> Employees
        {
            get => _employees;
            set { _employees = value; OnPropertyChanged(); }
        }
        private Role _selectedRole;
        public Role SelectedRole
        {
            get => _selectedRole;
            set { _selectedRole = value; OnPropertyChanged(); }
        }
        private List<Role> _roles;
        public List<Role> Roles
        {
            get => _roles;
            set
            {
                if (_roles == value) return;
                _roles = value;
                OnPropertyChanged();
            }
        }
        private int? _selectedStatus;

        public int? SelectedStatus
        {
            get => _selectedStatus;
            set { _selectedStatus = value; OnPropertyChanged(); }
        }
        private int _statuses;
        public int Status
        {
            get => _statuses;
            set
            {
                if (_statuses == value) return;
                _statuses = value;
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
                SyncSearchFilters();
                OnPropertyChanged(nameof(IsResetFilter));
                Refresh();
            }
        }
        public int FilteredCount => CollectionView?.Cast<object>().Count() ?? 0;
        public bool IsResetFilter => !string.IsNullOrWhiteSpace(SearchText) || SelectedFilter != null || CollectionFilter != null;
        public bool IsTextSearch => !IsSelectedRole || !IsSelectedStatus;
        public bool IsSelectedRole => SelectedFilter?.Name == "RoleId";
        public bool IsSelectedStatus => SelectedFilter?.Name == "Status";
        public List<SelectionFilter> FilterValues { get; set; } = new List<SelectionFilter> {
            new SelectionFilter { Name = "Login", Title = "Логин" },
            new SelectionFilter { Name = "RoleId", Title = "Роль" },
            new SelectionFilter { Name = "Status", Title = "Статус" },
            new SelectionFilter { Name = "EmployeeId", Title = "Сотрудник" }
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
                OnPropertyChanged(nameof(IsSelectedRole));
                OnPropertyChanged(nameof(IsSelectedStatus));
                // Reset values
                SelectedStatus = null;
                SelectedRole = null;
            }
        }

        private Data.Models.User user = ((App)(Application.Current)).CurrentUser;
        private ObservableCollection<HR.Data.Models.User> _users;
        public ObservableCollection<HR.Data.Models.User> Users
        {
            get => _users;
            set
            {
                _users = value;
                OnPropertyChanged();
            }
        }
        private HR.Data.Models.User _selectedUser;
        public HR.Data.Models.User SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged();
                // Update command state on selected user change
                CommandManager.InvalidateRequerySuggested();
            }
        }
        public UsersPg()
        {
            InitializeComponent();
            DataContext = this;
            Users = new ObservableCollection<HR.Data.Models.User>();

            DeleteCmd = new RelayCommand(
                execute: param =>
                {
                    List<HR.Data.Models.User> itemsToDelete = null;

                    if (param is Data.Models.User singleItem)
                        itemsToDelete = new List<HR.Data.Models.User> { singleItem };
                    else if (param is IEnumerable<Data.Models.User> manyItems)
                        itemsToDelete = manyItems.ToList();
                    else if (param is IList list && list.Count > 0 && list[0] is HR.Data.Models.User)
                        itemsToDelete = list.Cast<HR.Data.Models.User>().ToList();

                    if (itemsToDelete == null || itemsToDelete.Count == 0)
                        return;

                    // Confirmation
                    string msg = itemsToDelete.Count == 1
                        ? $"Вы действительно хотите удалить запись \"{itemsToDelete[0].Login}\"?"
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
                            Scope = "Пользователи",
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
                    if (param == null)
                        return false;

                    Debug.WriteLine($"CanExecute param type: {param.GetType()}");

                    List<Data.Models.User> itemsToDelete = null;
                    if (param is HR.Data.Models.User singleItem)
                        itemsToDelete = new List<Data.Models.User> { singleItem };
                    else if (param is IEnumerable<HR.Data.Models.User> manyItems)
                        itemsToDelete = manyItems.ToList();
                    else if (param is IList list && list.Count > 0 && list[0] is HR.Data.Models.User)
                        itemsToDelete = list.Cast<HR.Data.Models.User>().ToList();

                    if (itemsToDelete == null || itemsToDelete.Count == 0)
                        return false;
                    // Checking if not a current user
                    if (itemsToDelete.Any(item => item.Id == user.Id))
                        return false;

                    return true;
                }
            );
            FilterCmd = new RelayCommand(
                execute: param =>
                {
                    DataGrid dg = param as DataGrid ?? dataGrid;
                    DataGridCellInfo? cellInfo = null;

                    // Search button click
                    if (param == null)
                    {
                        if (SelectedFilter == null)
                        {
                            MessageBox.Show("Выберите критерий поиска!");
                            return;
                        }
                        object searchValue = GetSearchValue();
                        CollectionFilter = new FilterParam(SelectedFilter.Name, searchValue);

                        string valueStr = CollectionFilter.Value?.ToString();
                        if (SelectedFilter != null && string.IsNullOrWhiteSpace(valueStr))
                        {
                            CollectionFilter.Name = SelectedFilter.Name;
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
                            {
                                propertyName = binding.Path.Path;
                                MessageBox.Show($"propertyName {propertyName}");
                            }
                        }
                        else if (cellInfo.Value.Column is DataGridTemplateColumn templateColumn)
                        {
                            // Define property type based on column index
                            MessageBox.Show("Define property type based on column index");
                            int colIndex = dataGrid.Columns.IndexOf(cellInfo.Value.Column);
                            if (colIndex == 1) propertyName = "Status";
                        }

                        if (!string.IsNullOrEmpty(propertyName))
                        {
                            var value = item.GetType().GetProperty(propertyName)?.GetValue(item, null);
                            if (value == null && item is Data.Models.User u && propertyName == "Employee.FullName")
                            {
                                propertyName = "EmployeeId";
                                value = u.EmployeeId;
                            }
                            CollectionFilter = new FilterParam(propertyName, value);
                        }
                    }
                    // Special treatment for Ctrl+F on DataGridTemplateColumn
                    else
                    {
                        var selectedItem = dg.SelectedItem;
                        if (selectedItem != null && _lastColumnIndex > 0)
                        {
                            MessageBox.Show("Special treatment");
                            string propertyName = null;
                            switch (_lastColumnIndex)
                            {
                                case 1: propertyName = "Status"; break;
                            }
                            if (!string.IsNullOrEmpty(propertyName))
                            {
                                var value = selectedItem.GetType().GetProperty(propertyName)?.GetValue(selectedItem, null);
                                CollectionFilter = new FilterParam(propertyName, value);
                            }
                        }
                    }
                },
                _ => !IsProgress
            );
            ResetFilterCmd = new RelayCommand(
                _ => ResetFilter(),
                _ => !IsProgress && IsResetFilter
            );
            ResetSearchCmd = new RelayCommand(
                _ => SearchText = null,
                _ => !string.IsNullOrWhiteSpace(SearchText)
            );
        }

        private async void DeleteItems(List<HR.Data.Models.User> items)
        {
            var (cat, name, op, scope) = (EventCategory.Data, "Delete", 3, "Пользователи");
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
                Request.ctx.Users.RemoveRange(items);
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
                Debug.WriteLine(exc, "UserPg");
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
        // Triggered on Search button
        private object GetSearchValue()
        {
            if (SelectedFilter == null)
                return SearchText;

            switch (SelectedFilter.Name)
            {
                case "EmployeeId":
                    return Employees
                        .Where(emp => !string.IsNullOrEmpty(emp.FullName) &&
                              emp.FullName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0)
                        .Select(emp => emp.Id)
                        .ToList();
                case "RoleId":
                    return SelectedRole?.Id;
                case "Status":
                    return SelectedStatus;
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
        private async Task<bool> SaveData(HR.Data.Models.User item)
        {
            var (cat, name, op, scope) = (EventCategory.Data, "Update", 2, "Пользователи");
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
                    Message = "Сохранение данных"
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
                Debug.WriteLine(exc, "UsersPg");
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat,
                    Name = name,
                    Op = op,
                    Scope = scope,
                    Type = EventType.Error,
                    Message = $"Ошибка сохранения данных",
                    Details = exc.Message
                });
                MessageBox.Show($"Ошибка сохранения данных: {exc.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return true;
            }
            finally
            {
                IsProgress = false;
            }
        }
        private async Task SetData()
        {
            var (cat, name, op, scope) = (EventCategory.Data, "Read", 2, "Пользователи");
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
                var employeesTask = Request.LoadEmployeesLight();
                var usersTask = Request.GetUsers();
                var rolesTask = Request.LoadRoles();

                await Task.WhenAll(employeesTask, usersTask, rolesTask);

                DataCollection = new ObservableCollection<HR.Data.Models.User>(usersTask.Result);
                CollectionView = CollectionViewSource.GetDefaultView(DataCollection);
                Employees = employeesTask.Result;
                Roles = rolesTask.Result;

                CollectionView.Filter = obj => FilterHelper.FilterByValue(obj, CollectionFilter);
                CollectionView.Refresh();
                OnPropertyChanged(nameof(FilteredCount));   // update total count
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
                Debug.WriteLine(exc, "UsersPg");
                MessageBox.Show(exc.GetType().ToString());
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
        private void SyncSearchFilters()
        {
            // Sync SelectedFilter
            if (CollectionFilter == null || FilterValues == null) return;
            var match = FilterValues.FirstOrDefault(f => f.Name == CollectionFilter.Name);
            if (match == null || match == SelectedFilter) return;
            SelectedFilter = match;
            SearchText = CollectionFilter.Value?.ToString();
            if (CollectionFilter.Name == "EmployeeId" && CollectionFilter.Value is int uid)
            {
                SearchText = Employees.FirstOrDefault(x => x.Id == uid).FullName;
            }
            OnPropertyChanged(nameof(SelectedFilter));
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            // Numbering from 1
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var dg = sender as DataGrid;
            var item = dg?.SelectedItem as Data.Models.User;

            if (e.Key == Key.Delete && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (dg?.SelectedItems != null && dg.SelectedItems.Count > 0)
                {
                    // Convert to AppEventArgs (or IList)
                    var items = dg.SelectedItems.Cast<HR.Data.Models.User>().ToList();
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
            var item = e.Row.Item as HR.Data.Models.User;
            if (item == null) return;

            if (e.EditAction == DataGridEditAction.Commit)
            {
                Dispatcher.BeginInvoke(
                    (Action)(async () =>  // Explicit Action delegate type declaration
                    {
                        dataGrid.CommitEdit(DataGridEditingUnit.Row, true);
                        // Get entity state
                        var entry = Services.Request.ctx.Entry(item);
                        // Add if entity is detached
                        if (entry.State == EntityState.Detached)
                        {
                            Services.Request.ctx.Users.Attach(item);
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
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await SetData();
        }
    }
}
