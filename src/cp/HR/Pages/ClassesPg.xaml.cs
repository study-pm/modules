using HR.Controls;
using HR.Data.Models;
using HR.Models;
using HR.Services;
using HR.Utilities;
using Microsoft.Win32;
using PdfSharp.Pdf.Filters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static HR.Services.AppEventHelper;

namespace HR.Pages
{
    /// <summary>
    /// Interaction logic for ClassesPg.xaml
    /// </summary>
    public partial class ClassesPg : Page, INotifyPropertyChanged
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
        private ObservableCollection<ClassGuidance> _dataCol;
        public ObservableCollection<ClassGuidance> DataCollection
        {
            get => _dataCol;
            set
            {
                if (_dataCol == value) return;
                _dataCol = value;
                OnPropertyChanged();
                Refresh();
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
                SyncMenuFilters();
                SyncSearchFilters();
            }
        }
        public int FilteredCount => CollectionView?.Cast<object>().Count() ?? 0;
        public bool IsResetFilter => !string.IsNullOrWhiteSpace(SearchText) || SelectedFilter != null || CollectionFilter != null;
        public bool IsTextSearch => !IsSelectedGrade;
        public bool IsSelectedGrade => SelectedFilter?.Name == "GradeId";
        public List<SelectionFilter> FilterValues { get; set; } = new List<SelectionFilter> {
            new SelectionFilter { Name = "Title", Title = "Класс" },
            new SelectionFilter { Name = "GradeId", Title = "Параллель" },
            new SelectionFilter { Name = "EmployeeId", Title = "Руководитель" },
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
                OnPropertyChanged(nameof(IsSelectedGrade));
                OnPropertyChanged(nameof(IsTextSearch));
            }
        }
        // ComboBox lists
        private List<Employee> _employees;
        public List<Employee> Employees
        {
            get => _employees;
            set { _employees = value; OnPropertyChanged(); }
        }

        private List<Grade> _grades;
        public List<Grade> Grades
        {
            get => _grades;
            set { _grades = value; OnPropertyChanged(); }
        }

        private Employee _selectedEmployee;
        public Employee SelectedEmployee
        {
            get => _selectedEmployee;
            set { _selectedEmployee = value; OnPropertyChanged(); }
        }

        private Grade _selectedGrade;
        public Grade SelectedGrade
        {
            get => _selectedGrade;
            set { _selectedGrade = value; OnPropertyChanged(); }
        }
        public ClassesPg()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += Page_Loaded;

            AddCmd = new RelayCommand(
                _ =>
                {
                    ResetFilter();
                    var newItem = new ClassGuidance();
                    DataCollection.Add(newItem); // Add to ObservableCollection, bound to DataGrid
                    dataGrid.Focus();
                    dataGrid.SelectedItem = newItem;
                    dataGrid.CurrentCell = new DataGridCellInfo(newItem, dataGrid.Columns[1]);
                    dataGrid.ScrollIntoView(newItem);

                    if (dataGrid.ItemContainerGenerator.ContainerFromItem(newItem) is DataGridRow row)
                    {
                        var cell = VisualHelper.GetCell(dataGrid, row, 1);
                        cell?.Focus();
                    }

                    dataGrid.BeginEdit();
                },
                _ => !IsProgress
            );
            DeleteCmd = new RelayCommand(
                execute: param =>
                {
                    // Get list for deletion
                    List<ClassGuidance> itemsToDelete = null;

                    if (param is ClassGuidance singleItem)
                        itemsToDelete = new List<ClassGuidance> { singleItem };
                    else if (param is IEnumerable<ClassGuidance> manyItems)
                        itemsToDelete = manyItems.ToList();
                    else if (param is IList list && list.Count > 0 && list[0] is ClassGuidance)
                        itemsToDelete = list.Cast<ClassGuidance>().ToList();

                    if (itemsToDelete == null || itemsToDelete.Count == 0)
                        return;

                    // Confirmation
                    string msg = itemsToDelete.Count == 1
                        ? $"Вы действительно хотите удалить запись \"{itemsToDelete[0].Title}\"?"
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
                            Category = EventCategory.Data, Name = "Delete", Op = 3, Scope = "Классное руководство", Type = EventType.Cancel,
                            Message = "Удаление отменено", Details = "Отменено пользователем"
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
            ExportCmd = new RelayCommand(
                execute: param =>
                {
                    int index = 1;
                    if (param is int i) index = i;
                    else if (param is string s && int.TryParse(s, out int parsed)) index = parsed;

                    var sfd = new SaveFileDialog
                    {
                        FileName = "class_guidance_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"),
                        DefaultExt = ".csv",
                        Filter = "Таблица CSV (разделители - запятые) (*.csv)|*.csv|Таблица Excel (*.xlsx)|*.xlsx|Документ PDF (*.pdf)|*.pdf",
                        FilterIndex = index
                    };
                    bool? result = sfd.ShowDialog();
                    if (result != true)
                    {
                        RaiseAppEvent(new AppEventArgs
                        {
                            Category = EventCategory.Service,
                            Name = "Export",
                            Op = 4,
                            Scope = "Классное руководство",
                            Type = EventType.Cancel,
                            Message = "Экспорт данных отменен",
                            Details = "Отменено пользователем"
                        });
                        return;
                    }

                    string filePath = sfd.FileName;
                    index = sfd.FilterIndex;
                    switch (index)
                    {
                        case 1:
                        default:
                            ExportCsv(filePath);
                            break;
                        case 2:
                            ExportExcel(filePath);
                            break;
                        case 3:
                            ExportPdf(filePath);
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
                            if (colIndex == 2) propertyName = "EmployeeId";
                        }

                        if (!string.IsNullOrEmpty(propertyName))
                        {
                            var value = item.GetType().GetProperty(propertyName)?.GetValue(item, null);
                            CollectionFilter = new FilterParam(propertyName, value);
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
                                case 1: propertyName = "Title"; break;
                                case 2: propertyName = "EmployeeId"; break;
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
            NavigateCmd = new RelayCommand(
                execute: param =>
                {
                    if (param is Employee employee)
                    {
                        MainWindow.frame.Navigate(new EmployeePg(employee));
                    }
                    else if (param is NavigationData navData)
                    {
                        MainWindow.frame.Navigate(new Uri(navData.Uri, UriKind.Relative), navData.Parameter);
                    }
                    else
                    {
                        MessageBox.Show("Ошибка: сотрудник не выбран или не найден.");
                    }
                },
                canExecute: param => !IsProgress && (param is Employee || param is NavigationData)
            );
            ResetFilterCmd = new RelayCommand(
                _ => ResetFilter(),
                _ => !IsProgress && IsResetFilter
            );
            ResetSearchCmd = new RelayCommand(
                _ => SearchText = null,
                _ => !string.IsNullOrWhiteSpace(SearchText)
            );

            // Subscribe to navigation events for the main frame
            _navigationService = MainWindow.frame.NavigationService;
            if (_navigationService != null)
            {
                _navigationService.Navigated += NavigationService_Navigated;
            }
        }

        private void AddItem()
        {
            MessageBox.Show("AddItem()");
        }
        private async void DeleteItems(List<ClassGuidance> items)
        {
            var (cat, name, op, scope) = (EventCategory.Data, "Delete", 3, "Классное руководство");
            try
            {
                IsProgress = true;
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat, Name = name, Op = op, Scope = scope, Type = EventType.Progress, Message = "Удаление данных"
                });
                Request.ctx.ClassGuidances.RemoveRange(items);
                await Request.ctx.SaveChangesAsync();
                // Delete from the collection in memory
                foreach (var item in items)
                    DataCollection.Remove(item);
                Refresh();
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat, Name = name, Op = op, Scope = scope, Type = EventType.Success,
                    Message = "Данные успешно удалены"
                });
                MessageBox.Show($"Данные успешно удалены.", "Успешное удаление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc, "ClassesPg");
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat, Name = name, Op = op, Scope = scope, Type = EventType.Error,
                    Message = "Ошибка удаления данных", Details = exc.Message
                });
                MessageBox.Show($"Ошибка удаления данных: {exc.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProgress = false;
            }
        }
        private object GetSearchValue()
        {
            if (SelectedFilter == null)
                return SearchText;

            switch (SelectedFilter.Name)
            {
                case "EmployeeId":
                    // Return selected employee or their id
                    if (string.IsNullOrWhiteSpace(SearchText))
                    {
                        // If text string is empty then return null to unfilter by an employee
                        return null;
                    }

                    // Search employees by FullName ignoring case
                    var matchingIds = Employees?
                        .Where(e => !string.IsNullOrEmpty(e.FullName) &&
                                    e.FullName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0)
                        .Select(e => e.Id)
                        .ToList();

                    return matchingIds != null && matchingIds.Count > 0 ? matchingIds : null;

                case "GradeId":
                    // Return selected class or its Id
                    return SelectedGrade != null ? (int?)SelectedGrade.Id : null;

                default:
                    // Return search text for other filters
                    return SearchText;
            }
        }
        private async void ExportCsv(string filePath)
        {
            var (cat, name, op, scope) = (EventCategory.Service, "Export", 4, "Классное руководство");
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
                    Message = "Экспорт данных"
                });
                var skipList = new List<string> { "Id", "EmployeeId", "Grade", "GradeId" };

                //  Get hidden columns names (using SortMemberPath if any, Header otherwise)
                var hiddenColumns = dataGrid.Columns
                    .Where(c => c.Visibility != Visibility.Visible)
                    .Select(c => !string.IsNullOrEmpty(c.SortMemberPath) ? c.SortMemberPath : c.Header?.ToString())
                    .Where(n => !string.IsNullOrEmpty(n))
                    .ToList();

                // Add hidden columns avoiding duplication
                foreach (var colName in hiddenColumns)
                {
                    if (!skipList.Contains(colName, StringComparer.OrdinalIgnoreCase))
                        skipList.Add(colName);
                }

                // Now skipList contains all columns to skip
                var skip = skipList.ToArray();

                var converters = new Dictionary<string, Func<object, string>>
                {
                    ["Employee"] = val => val == null ? "" : ((Employee)val).FullName
                };

                var headers = new Dictionary<string, string>
                {
                    ["Title"] = "Класс",
                    ["Employee"] = "Классный руководитель"
                };

                string csvStr = CsvHelper.ExportCollectionViewToCsv(CollectionView, filePath, skip, converters, headers);

                await CsvHelper.SaveCsvAsync(filePath, csvStr);

                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat,
                    Name = name,
                    Op = op,
                    Scope = scope,
                    Type = EventType.Success,
                    Message = "Данные успешно экспортированы",
                    Details = "Экспорт в CSV"
                });
                MessageBox.Show($"Классное руководство сохранено в файле CSV.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc, "ClassesPg: CSV data export");
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat,
                    Name = name,
                    Op = op,
                    Scope = scope,
                    Type = EventType.Error,
                    Message = "Ошибка экспорта данных",
                    Details = exc.Message
                });
                MessageBox.Show($"Ошибка сохранения файла CSV: {exc.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProgress = false;
            }
        }
        private void ExportExcel(string filePath)
        {
            MessageBox.Show("Export Excel");
        }
        private void ExportPdf(string filePath)
        {
            var (cat, name, op, scope) = (EventCategory.Service, "Export", 4, "Классное руководство");
            try
            {
                var skip = new HashSet<string>(new[] { "Id", "Employee", "Grade", "GradeId" }, StringComparer.OrdinalIgnoreCase);

                var hiddenColumns = dataGrid.Columns
                    .Where(c => c.Visibility != Visibility.Visible)
                    .Select(c => c.SortMemberPath ?? c.Header?.ToString())
                    .Where(n => !string.IsNullOrEmpty(n))
                    .ToList();

                foreach (var colName in hiddenColumns)
                {
                    skip.Add(colName);
                }

                var headers = new Dictionary<string, string>
                {
                    ["Title"] = "Класс",
                    ["EmployeeId"] = "Классный руководитель"
                };
                var converters = new Dictionary<string, Func<object, object, string>>
                {
                    ["EmployeeId"] = (val, rowObj) =>
                    {
                        if (rowObj is ClassGuidance cg && cg.Employee != null)
                            return cg.Employee.FullName;
                        return "";
                    }
                };
                var columnWidths = new Dictionary<string, double>
                {
                    { "GradeId", 2 },
                    { "Title", 1.5 }
                };
                var document = PdfHelper.CreateDocumentFromCollectionView(
                    CollectionView,
                    documentTitle: $"Классное руководство на {DateTime.Now}",
                    skipProperties: skip,
                    customHeaders: headers,
                    customConverters: converters,
                    columnWidths: columnWidths,
                    addRowNumbers: true,
                    isLandscape: false
                    );

                PdfHelper.SaveDocumentToPdf(document, filePath);

                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat,
                    Name = name,
                    Op = op,
                    Scope = scope,
                    Type = EventType.Success,
                    Message = "Данные успешно экспортированы",
                    Details = "Экспорт в PDF"
                });
                MessageBox.Show($"Журнал событий сохранен в файле PDF.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc, "LogPg: PDF data export");
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat,
                    Name = name,
                    Op = op,
                    Scope = scope,
                    Type = EventType.Error,
                    Message = "Ошибка экспорта данных",
                    Details = exc.Message
                });
                MessageBox.Show($"Ошибка сохранения файла PDF: {exc.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
        private async Task<bool> SaveData(ClassGuidance item)
        {
            var (cat, name, op, scope) = (EventCategory.Data, item.Id == 0 ? "Create" : "Update", item.Id == 0 ? 0 : 2, "Классное руководство");
            try
            {
                IsProgress = true;
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat, Name = name, Op = op, Scope = scope, Type = EventType.Progress,
                    Message = (op == 0 ? "Добавление" : "Сохранение") + "данных"
                });
                await Services.Request.ctx.SaveChangesAsync();
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat, Name = name, Op = op, Scope = scope, Type = EventType.Success, Message = "Данные успешно сохранены"
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
                Debug.WriteLine(exc, "ClassesPg");
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat, Name = name, Op = op, Scope = scope, Type = EventType.Error,
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
            var (cat, name, op, scope) = (EventCategory.Data, "Read", 2, "Классное руководство");
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
                var classGuidanceTask = Request.GetClassGuidance();
                var employeesTask = Request.LoadEmployees();
                var gradesTask = Request.LoadGrades();

                await Task.WhenAll(classGuidanceTask, employeesTask, gradesTask);

                // Update UI via Dispatcher
                await Dispatcher.InvokeAsync(() =>
                {
                    DataCollection = new ObservableCollection<ClassGuidance>(classGuidanceTask.Result);
                    CollectionView = CollectionViewSource.GetDefaultView(DataCollection);
                    Employees = employeesTask.Result;
                    Grades = gradesTask.Result;

                    CollectionView.Filter = obj => FilterHelper.FilterByValue(obj, CollectionFilter);
                    CollectionView.Refresh();
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
            var items = CollectionView?.Cast<ClassGuidance>().ToList();
            List<int> uniqueIds = items?.Select(cg => cg.GradeId).Distinct().ToList();

            if (!(mainWindow?.MenuVM is MenuViewModel) || !(mainWindow.MenuVM.Filters is IEnumerable<MenuFilter>)) return;

            var classesFilter = mainWindow.MenuVM.Filters.FirstOrDefault(f => f.Title == "Классы");

            if (classesFilter == null || classesFilter.Values == null || uniqueIds == null) return;

            foreach (var val in classesFilter.Values)
                val.IsChecked = uniqueIds.Contains(val.Id);
        }
        private void SyncSearchFilters()
        {
            if (CollectionFilter == null) return;
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
            else if (CollectionFilter.Name == "GradeId")
            {
                MessageBox.Show("GradeId");
                SelectedFilter = FilterValues.FirstOrDefault(f => f.Name == "GradeId");
                if (CollectionFilter.Value is int singleGradeId)
                    SelectedGrade = Grades?.FirstOrDefault(a => a.Id == singleGradeId);
                else if (CollectionFilter.Value is IEnumerable<int> gradeIds && gradeIds.Any())
                    SelectedGrade = Grades?.FirstOrDefault(a => a.Id == gradeIds.First());
                else
                    SelectedGrade = null;
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
            var item = dg?.SelectedItem as ClassGuidance;

            if (e.Key == Key.Delete && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (dg?.SelectedItems != null && dg.SelectedItems.Count > 0)
                {
                    // Convert to AppEventArgs (or IList)
                    var items = dg.SelectedItems.Cast<ClassGuidance>().ToList();
                    if (DeleteCmd.CanExecute(items))
                    {
                        DeleteCmd.Execute(items);
                        e.Handled = true;
                    }
                }
            }
            if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Shift)
            {
                if (item != null && ResetFilterCmd.CanExecute(null))
                {
                    ResetFilterCmd.Execute(null);
                    e.Handled = true;
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
            // Iterating tree ascending, regarding the fact that Run is not a Visual
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
            var item = e.Row.Item as ClassGuidance;
            if (item == null) return;

            if (e.EditAction == DataGridEditAction.Commit)
            {
                Dispatcher.BeginInvoke(
                    (Action)(async () =>  // Explicit Action delegate type declaration
                    {
                        if (string.IsNullOrWhiteSpace(item.Title))
                        {
                            MessageBox.Show("Название не может быть пустым", "Неверные данные", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            // Возвращаем фокус и редактирование
                            dataGrid.Focus();
                            dataGrid.SelectedItem = item;
                            dataGrid.CurrentCell = new DataGridCellInfo(item, dataGrid.Columns[1]); // e.g. first (second) column
                            dataGrid.BeginEdit();
                            return;
                        }
                        // Добавляем новый объект в контекст, если он новый
                        if (item.Id == 0) // или другой признак нового объекта
                        {
                            bool isParsed = int.TryParse(item.Title.Split('-')[0], out int parsed);
                            if (!isParsed)
                            {
                                MessageBox.Show("Неверный формат названия", "Неверные данные", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                // Возвращаем фокус и редактирование
                                dataGrid.Focus();
                                dataGrid.SelectedItem = item;
                                dataGrid.CurrentCell = new DataGridCellInfo(item, dataGrid.Columns[1]);
                                dataGrid.BeginEdit();
                                return;
                            }
                            item.GradeId = parsed;
                            Request.ctx.ClassGuidances.Add(item);
                        }
                        else
                        {
                            var entry = Services.Request.ctx.Entry(item);
                            bool isModified = entry.State == EntityState.Modified;

                            if (!isModified) return;
                        }

                        await SaveData(item);
                    }),
                    System.Windows.Threading.DispatcherPriority.Background); // Приоритет передается вторым аргументом
            }
            if (e.EditAction == DataGridEditAction.Cancel)
            {
                var result = MessageBox.Show(
                        $"Отменить {(item.Id == 0 ? "добавление" : "редактирование")} элемента?",
                        "Подтверждение отмены",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning,
                        MessageBoxResult.No
                    );

                if (result != MessageBoxResult.Yes)
                {
                    // Пользователь отказался отменять — отменяем отмену редактирования
                    e.Cancel = true;

                    // Возвращаем фокус и редактирование на текущую ячейку
                    dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        dataGrid.SelectedItem = item;
                        dataGrid.CurrentCell = new DataGridCellInfo(item, dataGrid.Columns[0]); // например, первая колонка
                        dataGrid.Focus();

                        if (dataGrid.ItemContainerGenerator.ContainerFromItem(item) is DataGridRow row)
                        {
                            var cell = VisualHelper.GetCell(dataGrid, row, 1);
                            cell?.Focus();
                        }
                        dataGrid.BeginEdit();

                    }), System.Windows.Threading.DispatcherPriority.Input);
                    return;
                }
                // Пользователь подтвердил отмену — логируем и удаляем, если новый элемент
                RaiseAppEvent(new AppEventArgs
                {
                    Category = EventCategory.Data,
                    Name = item.Id == 0 ? "Create" : "Update",
                    Op = 3,
                    Scope = "Классное руководство",
                    Type = EventType.Cancel,
                    Message = "Операция отменена",
                    Details = item.Id == 0 ? "Добавление" : "Редактирование" + "отменено пользователем"
                });
                // Если это новый элемент (например, Id == 0), удаляем его из коллекции
                if (item.Id == 0)
                {
                    DataCollection.Remove(item);
                }
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

            List<int> classes = filter.Values.Where(fv => fv.IsChecked).Select(item => item.Id).ToList();
            CollectionFilter = new FilterParam("GradeId", classes);
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
    }
}
