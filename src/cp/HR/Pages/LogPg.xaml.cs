using HR.Data.Models;
using HR.Services;
using HR.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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
        public string Name { get; set; }
        public string Title { get; set; }
    }
    /// <summary>
    /// Interaction logic for LogPg.xaml
    /// </summary>
    public partial class LogPg : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

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
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value) return;
                _searchText = value;
                OnPropertyChanged();
            }
        }

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
                // Синхронизируем SelectedFilter
                if (CollectionFilter != null && FilterValues != null)
                {
                    var match = FilterValues.FirstOrDefault(f => f.Name == CollectionFilter.Name);
                    if (match != null && match != SelectedFilter)
                    {
                        _selectedFilter = match;
                        SearchText = CollectionFilter.Value?.ToString();
                        OnPropertyChanged(nameof(SelectedFilter));
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
            new SelectionFilter { Name = "Date", Title = "Дата" },
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
            }
        }

        public LogPg()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += Page_Loaded;

            DeleteCmd = new RelayCommand(
                execute: param =>
                {
                    if (param is AppEventArgs item)
                    {
                        var result = MessageBox.Show(
                            $"Вы действительно хотите удалить запись \"{item.Message}\"?",
                            "Подтверждение удаления",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning,
                            MessageBoxResult.No
                        );

                        if (result == MessageBoxResult.Yes)
                        {
                            DataCollection.Remove(item);
                            dataGrid.Items.Refresh(); // refresh auto numbering
                            // @TODO: Delete from DB
                            // await Services.Request.DeleteUser(userToDelete.Id);
                        }
                    }
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

                    // 0. Если param == null, значит вызов с кнопки "Найти"
                    if (param == null)
                    {
                        if (SelectedFilter == null)
                        {
                            MessageBox.Show("Выберите критерий поиска!");
                            return;
                        }
                        CollectionFilter = new FilterParam(SelectedFilter.Name, SearchText);
                        // Проверяем выбранный фильтр и значение
                        string valueStr = CollectionFilter.Value?.ToString();
                        if (SelectedFilter != null && !string.IsNullOrWhiteSpace(valueStr))
                        {
                            // Обновляем имя фильтра в CollectionFilter
                            CollectionFilter.Name = SelectedFilter.Name;
                            // Вызываем фильтрацию
                            FilterByCellValue();
                        }
                        return;
                    }

                    // 1. Если был клик правой кнопкой мыши — используем _lastRightClickedCell
                    if (_lastRightClickedCell != null)
                    {
                        cellInfo = _lastRightClickedCell;
                    }
                    // 2. Если команда вызвана через горячую клавишу — используем CurrentCell
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
                _ => !IsProgress && (!string.IsNullOrWhiteSpace(SearchText) || CollectionView?.Filter != null)
            );
            ResetSearchCmd = new RelayCommand(
                _ => SearchText = null,
                _ => !string.IsNullOrWhiteSpace(SearchText)
            );
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

                // Для строк — сравниваем как строки (без учёта регистра)
                /*
                if (val is string s1 && CollectionFilter.Value is string s2)
                    return string.Equals(s1, s2, StringComparison.OrdinalIgnoreCase);
                */
                // Для строк — ищем подстроку без учёта регистра
                if (val is string s1)
                {
                    // Приводим фильтр к строке, если это не строка (например, object)
                    string filterStr = CollectionFilter.Value?.ToString() ?? string.Empty;
                    return s1.IndexOf(filterStr, StringComparison.OrdinalIgnoreCase) >= 0;
                }

                // Для других типов — сравниваем через Equals
                return Equals(val, CollectionFilter.Value);
            };
            CollectionView.Refresh();
            OnPropertyChanged(nameof(FilteredCount));
        }
        private void ResetFilter()
        {
            SearchText = null;
            SelectedFilter = null;
            CollectionView.Filter = null;
            CollectionView.Refresh();
            OnPropertyChanged(nameof(FilteredCount));
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
                if (item != null && DeleteCmd.CanExecute(item))
                {
                    DeleteCmd.Execute(item);
                    e.Handled = true;
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
            OnPropertyChanged(nameof(FilteredCount));
            IsProgress = false;
        }
    }
}
