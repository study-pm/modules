using HR.Controls;
using HR.Data.Models;
using HR.Services;
using HR.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
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

namespace HR.Pages
{
    /// <summary>
    /// Interaction logic for HelpPg.xaml
    /// </summary>
    public partial class HelpPg : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        public ICommand DeleteCmd { get; }
        public ICommand ExportCmd { get; }
        public ICommand FilterCmd { get; }
        public ICommand NavigateCmd { get; }

        private NavigationService _navigationService;

        private DataGridCellInfo? _lastRightClickedCell;

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

        private ObservableCollection<Employee> _staff;
        public ObservableCollection<Employee> Staff
        {
            get => _staff;
            set
            {
                if (_staff == value) return;
                _staff = value;
                OnPropertyChanged();
            }
        }

        public HelpPg()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += Page_Loaded;

            DeleteCmd = new RelayCommand(
                execute: param =>
                {
                    if (param is HR.Data.Models.Employee item)
                    {
                        var result = MessageBox.Show(
                            $"Вы действительно хотите удалить сотрудника \"{item.FullName}\"?",
                            "Подтверждение удаления",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning,
                            MessageBoxResult.No
                        );

                        if (result == MessageBoxResult.Yes)
                        {
                            Staff.Remove(item);
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
                    switch(param)
                    {
                        case "CSV": default:
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
                    DataGrid dg = param as DataGrid ?? dataGrid; // dataGrid — это ссылка на твой DataGrid (например, через поле)
                    DataGridCellInfo? cellInfo = null;

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
                        var column = cellInfo.Value.Column as DataGridBoundColumn;
                        if (column != null)
                        {
                            var binding = column.Binding as Binding;
                            if (binding != null)
                            {
                                string propertyName = binding.Path.Path;
                                var value = item.GetType().GetProperty(propertyName)?.GetValue(item, null);
                                FilterByCellValue(new FilterParam(propertyName, value));
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
                        MainWindow.frame.Navigate(new EmployeePg());
                    }
                },
                canExecute: param => !IsProgress
            );

            // Subscribe to navigation events for the main frame
            _navigationService = MainWindow.frame.NavigationService;
            if (_navigationService != null)
            {
                _navigationService.Navigated += NavigationService_Navigated;
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
        private void FilterByCellValue(FilterParam param)
        {
            MessageBox.Show($"Filter by {param.Name} with value {param.Value}");
        }

        private void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                Dispatcher.BeginInvoke(
                    (Action)(async () =>  // Явное указание типа делегата Action
                    {
                        var item = e.Row.Item as Employee;
                        if (item == null) return;

                        var entry = Services.Request.ctx.Entry(item);
                        bool isModified = entry.State == EntityState.Modified;

                        if (!isModified) return;

                        try
                        {
                            IsProgress = true;
                            StatusInformer.ReportProgress("Сохранение данных");
                            await Services.Request.ctx.SaveChangesAsync();
                            StatusInformer.ReportSuccess("Данные успешно сохранены");
                        }
                        catch (Exception exc)
                        {
                            StatusInformer.ReportFailure($"Ошибка при сохранении данных: {exc}");
                            MessageBox.Show($"Ошибка при сохранении данных: {exc}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        finally
                        {
                            IsProgress = false;
                        }
                    }),
                    System.Windows.Threading.DispatcherPriority.Background); // Приоритет передается вторым аргументом
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
            var item = dg?.SelectedItem as HR.Data.Models.Employee;
            if (e.Key == Key.Delete && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (item != null && DeleteCmd.CanExecute(item))
                {
                    DeleteCmd.Execute(item);
                    e.Handled = true;
                }
            }
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (item != null && NavigateCmd.CanExecute(item))
                {
                    NavigateCmd.Execute(item);
                    e.Handled = true;
                }
            }
            if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (item != null && NavigateCmd.CanExecute(item))
                {
                    _lastRightClickedCell = null; // Invalidate last right clicked cell
                    FilterCmd.Execute(dataGrid);
                    e.Handled = true;
                }
            }
            if (e.Key == Key.Insert && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (item != null && NavigateCmd.CanExecute(item))
                {
                    NavigateCmd.Execute(null);
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
        private void NavigationService_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Content != this) return;
            var navigationParameter = e.ExtraData;

            // Handle filter values here
        }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Staff = new ObservableCollection<Employee>(await Request.GetEmployees());
            // Alternative way: without creating a new instance
            /*
            Classes.Clear();
            foreach (var item in fromService)
                Classes.Add(item);
            */
        }
    }
}
