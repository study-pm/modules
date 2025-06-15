using HR.Data.Models;
using HR.Services;
using HR.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static HR.Services.AppEventHelper;

namespace HR.Pages
{
    public class EventCategoryToPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            EventCategory status;

            if (value is EventCategory ec)
                status = ec;
            else if (value is byte b && Enum.IsDefined(typeof(EventCategory), b))
                status = (EventCategory)b;
            else
                return null;

            switch (status)
            {
                case EventCategory.Auth:
                    return Application.Current.TryFindResource("KeySolidPath") as Geometry;
                case EventCategory.Data:
                    return Application.Current.TryFindResource("DatabaseSolidPath") as Geometry;
                case EventCategory.Navigation:
                    return Application.Current.TryFindResource("LocationArrowSolidPath") as Geometry;
                case EventCategory.Service:
                    return Application.Current.TryFindResource("ToolsSolidPath") as Geometry;
                default:
                    return null;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class EventCategoryToTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AppEventHelper.EventCategory eventCategory)
            {
                return eventCategory.ToTitle();
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class EventTypeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            EventType status;

            if (value is EventType et)
                status = et;
            else if (value is byte b && Enum.IsDefined(typeof(EventType), b))
                status = (EventType)b;
            else
                return Brushes.Gray;

            string resourceKey;

            switch (status)
            {
                case EventType.Progress:
                    resourceKey = "AwaitingBrush";
                    break;
                case EventType.Success:
                    resourceKey = "SuccessBrush";
                    break;
                case EventType.Error:
                    resourceKey = "ErrorBrush";
                    break;
                case EventType.Info:
                    resourceKey = "InfoBrush";
                    break;
                case EventType.Warning:
                    resourceKey = "WarningBrush";
                    break;
                default:
                    resourceKey = "greyDarkBrush";
                    break;
            }

            if (Application.Current.Resources.Contains(resourceKey))
            {
                return Application.Current.Resources[resourceKey] as Brush ?? Brushes.Gray;
            }
            else
            {
                return Brushes.Gray;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class EventOperationToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int op)) return Brushes.Gray;
            string resourceKey;

            switch (op)
            {
                case 0:
                    resourceKey = "InfoBrush";
                    break;
                case 1:
                    resourceKey = "WarningBrush";
                    break;
                case 2:
                    resourceKey = "SuccessBrush";
                    break;
                case 3:
                    resourceKey = "ErrorBrush";
                    break;
                default:
                    resourceKey = "greyDarkBrush";
                    break;
            }

            if (Application.Current.Resources.Contains(resourceKey))
            {
                return Application.Current.Resources[resourceKey] as Brush ?? Brushes.Gray;
            }
            else
            {
                return Brushes.Gray;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class EventTypeToPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            EventType status;

            if (value is EventType et)
                status = et;
            else if (value is byte b && Enum.IsDefined(typeof(EventType), b))
                status = (EventType)b;
            else
                return null;

            switch (status)
            {
                case EventType.Progress:
                    return Application.Current.TryFindResource("ClockSolidPath") as Geometry;
                case EventType.Success:
                    return Application.Current.TryFindResource("CheckCircleSolidPath") as Geometry;
                case EventType.Error:
                    return Application.Current.TryFindResource("ExclamationCircleSolidPath") as Geometry;
                case EventType.Info:
                    return Application.Current.TryFindResource("InfoCircleSolidPath") as Geometry;
                case EventType.Warning:
                    return Application.Current.TryFindResource("ExclamationTriangleSolidPath") as Geometry;
                default:
                    return null;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class EventTypeToTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AppEventHelper.EventType eventType)
            {
                return eventType.ToTitle(); // Вызов вашего метода расширения
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class SelectionFilter
    {
        public int id{ get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
    }
    public class EnumFilterInfo : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public Type EnumType { get; set; }
        public List<SelectionFilter> Values { get; set; }

        private SelectionFilter _selected;
        public SelectionFilter Selected
        {
            get => _selected;
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Selected)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
    /// <summary>
    /// Interaction logic for LogPg.xaml
    /// </summary>
    public partial class LogPg : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        public ICommand ClearCmd { get; }
        public ICommand DeleteCmd { get; }
        public ICommand ExportCmd { get; }
        public ICommand FilterCmd { get; }
        public ICommand ResetFilterCmd { get; }
        public ICommand ResetSearchCmd { get; }

        private DataGridCellInfo? _lastRightClickedCell;
        private int _lastColumnIndex = -1;

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
        private DateTime? _dateFrom;
        public DateTime? DateFrom
        {
            get => _dateFrom;
            set { _dateFrom = value; OnPropertyChanged(); }
        }

        private DateTime? _dateTo;
        public DateTime? DateTo
        {
            get => _dateTo;
            set { _dateTo = value; OnPropertyChanged(); }
        }
        public List<EnumFilterInfo> EnumFilters { get; set; } = new List<EnumFilterInfo>
        {
            new EnumFilterInfo
            {
                Name = "Type",
                EnumType = typeof(EventType),
                Values = Enum.GetValues(typeof(EventType))
                    .Cast<EventType>()
                    .Select(e => new SelectionFilter
                    {
                        id = (int)e,
                        Name = e.ToString(),
                        Title = e.ToTitle()
                    })
                    .ToList()
            },
            new EnumFilterInfo
            {
                Name = "Category",
                EnumType = typeof(EventCategory),
                Values = Enum.GetValues(typeof(EventCategory))
                    .Cast<EventCategory>()
                    .Select(e => new SelectionFilter
                    {
                        id = (int)e,
                        Name = e.ToString(),
                        Title = e.ToTitle()
                    })
                    .ToList()
            }
        };

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

        public bool IsResetFilter => !string.IsNullOrWhiteSpace(SearchText) || SelectedFilter != null;

        private ICollectionView CollectionView;
        private ObservableCollection<AppEventArgs> _dataCol;
        public ObservableCollection<AppEventArgs> DataCollection
        {
            get => _dataCol;
            set
            {
                if (_dataCol == value) return;
                _dataCol = value;
                OnPropertyChanged();
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
                // Sync SelectedFilter
                if (CollectionFilter == null || FilterValues == null) return;
                var match = FilterValues.FirstOrDefault(f => f.Name == CollectionFilter.Name);
                if (match == null || match == SelectedFilter) return;
                SelectedFilter = match;
                SearchText = CollectionFilter.Value?.ToString();
                OnPropertyChanged(nameof(SelectedFilter));

                if (SelectedFilter.Name == "Timestamp")
                {
                    // Если пришёл диапазон (Tuple<DateTime, DateTime>)
                    if (CollectionFilter.Value is Tuple<DateTime, DateTime> range)
                    {
                        DateFrom = range.Item1;
                        DateTo = range.Item2;
                    }
                    // Если пришла одна дата
                    else if (CollectionFilter.Value is DateTime dt)
                    {
                        DateFrom = dt;
                        DateTo = null;
                    }
                    else
                    {
                        DateFrom = null;
                        DateTo = null;
                    }
                }

                else if (EnumFilters == null) return;
                // Universal sync for all EnumFilterInfo
                foreach (var enumFilter in EnumFilters)
                {
                    if (enumFilter.Name == SelectedFilter.Name)
                    {
                        enumFilter.Selected = FindEnumSelection(enumFilter.Values, CollectionFilter.Value);
                    }
                    else
                    {
                        enumFilter.Selected = null;
                    }
                }
            }
        }
        public int FilteredCount => CollectionView?.Cast<object>().Count() ?? 0;
        public List<SelectionFilter> FilterValues { get; set; } = new List<SelectionFilter> {
            new SelectionFilter { Name = "Scope", Title = "Контекст" },
            new SelectionFilter { Name = "Message", Title = "Событие" },
            new SelectionFilter { Name = "Details", Title = "Подробности" },
            new SelectionFilter { Name = "Type", Title = "Тип" },
            new SelectionFilter { Name = "Category", Title = "Категория" },
            new SelectionFilter { Name = "Timestamp", Title = "Дата" },
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
                OnPropertyChanged(nameof(IsSelectedCategory));
                OnPropertyChanged(nameof(IsSelectedTimestamp));
                OnPropertyChanged(nameof(IsSelectedType));
                OnPropertyChanged(nameof(IsTextSearch));

                // Reset all selected values in enum-filters on filter change
                foreach (var ef in EnumFilters)
                    if (ef.Name != value?.Name)
                        ef.Selected = null;
            }
        }
        public bool IsSelectedTimestamp => SelectedFilter?.Name == "Timestamp";
        public bool IsSelectedType => SelectedFilter?.Name == "Type";
        public bool IsSelectedCategory => SelectedFilter?.Name == "Category";
        public bool IsTextSearch => !IsSelectedCategory && !IsSelectedTimestamp && !IsSelectedType;

        public LogPg()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += Page_Loaded;


            ClearCmd = new RelayCommand(
                _ =>
                {
                    var result = MessageBox.Show(
                        "Вы действительно хотите очистить лог (все данные при этом будут удалены без возможности восстановления)?",
                        "Подтверждение удаления",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning,
                        MessageBoxResult.No
                    );

                    if (result != MessageBoxResult.Yes)
                        return;

                    ClearLog();
                },
                _ => !IsProgress && DataCollection?.Count > 0
            );
            DeleteCmd = new RelayCommand(
                execute: param =>
                {
                    // Get list for deletion
                    List<AppEventArgs> itemsToDelete = null;

                    if (param is AppEventArgs singleItem)
                        itemsToDelete = new List<AppEventArgs> { singleItem };
                    else if (param is IEnumerable<AppEventArgs> manyItems)
                        itemsToDelete = manyItems.ToList();
                    else if (param is IList list && list.Count > 0 && list[0] is AppEventArgs)
                        itemsToDelete = list.Cast<AppEventArgs>().ToList();

                    if (itemsToDelete == null || itemsToDelete.Count == 0)
                        return;

                    // Confirmation
                    string msg = itemsToDelete.Count == 1
                        ? $"Вы действительно хотите удалить запись \"{itemsToDelete[0].Message}\"?"
                        : $"Вы действительно хотите удалить выбранные записи ({itemsToDelete.Count})?";
                    var result = MessageBox.Show(
                        msg,
                        "Подтверждение удаления",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning,
                        MessageBoxResult.No
                    );

                    if (result != MessageBoxResult.Yes)
                        return;

                    DeleteItems(itemsToDelete);
                },
                canExecute: param =>
                {
                    return !IsProgress;
                }
            );
            ExportCmd = new RelayCommand(
                execute: param =>
                {
                    switch (param)
                    {
                        case "CSV":
                        default:
                            ExportCSV();
                            break;
                        case "PDF":
                            ExportPDF();
                            break;
                    }
                },
                _ => !IsProgress
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
                        if (SelectedFilter != null && !string.IsNullOrWhiteSpace(valueStr))
                        {
                            CollectionFilter.Name = SelectedFilter.Name;
                            FilterByCellValue();
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
                            if (colIndex == 1) propertyName = "Type";
                            else if (colIndex == 6) propertyName = "Category";
                        }

                        if (!string.IsNullOrEmpty(propertyName))
                        {
                            var value = item.GetType().GetProperty(propertyName)?.GetValue(item, null);
                            CollectionFilter = new FilterParam(propertyName, value);
                            FilterByCellValue();
                        }
                    }
                    else
                    {
                        var selectedItem = dg.SelectedItem;
                        if (selectedItem != null && _lastColumnIndex > 0)
                        {
                            string propertyName = null;
                            switch (_lastColumnIndex)
                            {
                                case 1: propertyName = "Type"; break;
                                case 2: propertyName = "Timestamp"; break;
                                case 3: propertyName = "Scope"; break;
                                case 4: propertyName = "Message"; break;
                                case 5: propertyName = "Details"; break;
                                case 6: propertyName = "Category"; break;
                            }
                            if (!string.IsNullOrEmpty(propertyName))
                            {
                                var value = selectedItem.GetType().GetProperty(propertyName)?.GetValue(selectedItem, null);
                                CollectionFilter = new FilterParam(propertyName, value);
                                FilterByCellValue();
                            }
                        }
                    }
                },
                _ => !IsProgress
            );
            ResetFilterCmd = new RelayCommand(
                execute: param => ResetFilter(),
                _ => !IsProgress && IsResetFilter
            );
            ResetSearchCmd = new RelayCommand(
                _ => SearchText = null,
                _ => !string.IsNullOrWhiteSpace(SearchText)
            );
        }

        private async void ClearLog()
        {
            try
            {
                IsProgress = true;
                await Request.ClearLog(App.Current.CurrentUser.Id);
                // Remove collection in memory
                DataCollection = null;
                // Clear the collection in memory
                // DataCollection.Clear(); // update numbering
                // Refresh();              // update total count
                MessageBox.Show($"Данные успешно удалены.", "Успешное удаление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception exc)
            {
                MessageBox.Show($"Ошибка удаления данных: {exc.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProgress = false;
            }
        }
        private async void DeleteItems(List<AppEventArgs> items)
        {
            try
            {
                IsProgress = true;
                int removedCount = await Request.DeleteLogItems(App.Current.CurrentUser.Id, items);
                // Delete from the collection in memory
                foreach (var item in items)
                    DataCollection.Remove(item);
                Refresh();
                MessageBox.Show($"Данные успешно удалены.", "Успешное удаление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception exc) {
                MessageBox.Show($"Ошибка удаления данных: {exc.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProgress = false;
            }
        }
        private void ExportCSV()
        {
            MessageBox.Show("Export CSV");
        }
        private void ExportPDF()
        {
            MessageBox.Show("Export PDF");
        }
        private void FilterByCellValue()
        {
            if (CollectionView == null)
                CollectionView = CollectionViewSource.GetDefaultView(DataCollection);
            
            CollectionView.Filter = obj =>
            {
                var item = obj as AppEventArgs;
                if (item == null) return false;
                var prop = item.GetType().GetProperty(CollectionFilter.Name);
                if (prop == null) return false;
                var val = prop.GetValue(item, null);
                var filterVal = CollectionFilter.Value;
                if (val == null || filterVal == null) return false;

                if (CollectionFilter.Name == "Timestamp" && val is DateTime itemDate)
                {
                    // Date range
                    if (filterVal is Tuple<DateTime, DateTime> range)
                    {
                        var from = range.Item1.Date;
                        var to = range.Item2.Date;
                        return itemDate.Date >= from && itemDate.Date <= to;
                    }
                    // Single date
                    if (filterVal is DateTime dt)
                    {
                        return itemDate.Date == dt.Date;
                    }
                    // Datestring
                    if (filterVal is string s && DateTime.TryParse(s, out var parsed))
                    {
                        return itemDate.Date == parsed.Date;
                    }
                }

                // Strings: search substring case-insensitive
                if (val is string s1)
                {
                    // Convert filter object value to string
                    string filterStr = CollectionFilter.Value?.ToString() ?? string.Empty;
                    return s1.IndexOf(filterStr, StringComparison.OrdinalIgnoreCase) >= 0;
                }

                // Enum and other types: direct comparison
                return val.Equals(filterVal); // return Equals(val, CollectionFilter.Value);
            };
            Refresh();
        }
        private SelectionFilter FindEnumSelection(List<SelectionFilter> values, object value)
        {
            if (value == null) return null;
            string name = value is Enum ? value.ToString() : value as string;
            return values.FirstOrDefault(v => v.Name == name);
        }
        private object GetSearchValue()
        {
            var enumFilter = EnumFilters.FirstOrDefault(f => f.Name == SelectedFilter?.Name);
            if (enumFilter != null && enumFilter.Selected != null)
            {
                return Enum.Parse(enumFilter.EnumType, enumFilter.Selected.Name);
            }
            // Single date/date range filter
            if (SelectedFilter?.Name == "Timestamp")
            {
                if (DateFrom.HasValue && DateTo.HasValue)
                {
                    var from = DateFrom.Value.Date;
                    var to = DateTo.Value.Date;
                    // Swap dates if order is invalid
                    if (from > to) (to, from) = (from, to);
                    return new Tuple<DateTime, DateTime>(from, to);
                }
                if (DateFrom.HasValue)
                    return DateFrom.Value.Date;
                if (DateTo.HasValue)
                    return DateTo.Value.Date;
                return null;
            }
            return SearchText;
        }
        private void Refresh()
        {
            dataGrid.Items.Refresh();                   // update numbering
            CollectionView.Refresh();                   // refresh collection view
            OnPropertyChanged(nameof(FilteredCount));   // update total count
        }
        private void ResetFilter()
        {
            SearchText = null;
            SelectedFilter = null;
            DateFrom = null;
            DateTo = null;
            CollectionView.Filter = null;
            Refresh();
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
        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            // Numbering from 1
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var dg = sender as DataGrid;
            var item = dg?.SelectedItem as AppEventArgs;

            if (e.Key == Key.Delete && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (ClearCmd.CanExecute(null))
                {
                    ClearCmd.Execute(null);
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Delete)
            {
                if (dg?.SelectedItems != null && dg.SelectedItems.Count > 0)
                {
                    // Convert to AppEventArgs (or IList)
                    var items = dg.SelectedItems.Cast<AppEventArgs>().ToList();
                    if (DeleteCmd.CanExecute(items))
                    {
                        DeleteCmd.Execute(items);
                        e.Handled = true;
                    }
                }
            }
            if (e.Key == Key.Escape)
            {
                if (item != null && ResetFilterCmd.CanExecute(item))
                {
                    ResetFilterCmd.Execute(item);
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
            if (e.Key == Key.P && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (item != null && ExportCmd.CanExecute(null))
                {
                    ExportCmd.Execute("PDF");
                    e.Handled = true;
                }
            }
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (item != null && ExportCmd.CanExecute(null))
                {
                    ExportCmd.Execute("CSV");
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
            // Обход вверх по дереву, учитывая, что Run не является Visual
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

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            IsProgress = true;
            DataCollection = new ObservableCollection<AppEventArgs>(await Request.GetLog(App.Current.CurrentUser.Id));
            CollectionView = CollectionViewSource.GetDefaultView(DataCollection);
            Refresh();
            IsProgress = false;
        }
    }
}
