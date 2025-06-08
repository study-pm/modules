using HR.Controls;
using HR.Data.Models;
using HR.Models;
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
    /// Interaction logic for ClassesPg.xaml
    /// </summary>
    public partial class ClassesPg : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        public ICommand DeleteItemCommand { get; }

        private NavigationService _navigationService;
        public FilterValue FilterParam { get; set; }

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
        public bool? IsListEmpty => Classes?.Count == 0;
        public bool? IsListNotEmpty => Classes?.Count > 0;
        private ObservableCollection<ClassGuidance> _classes;
        public ObservableCollection<ClassGuidance> Classes
        {
            get => _classes;
            set
            {
                _classes = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsListEmpty));
                OnPropertyChanged(nameof(IsListNotEmpty));
            }
        }
        private HR.Data.Models.ClassGuidance _selected;
        public HR.Data.Models.ClassGuidance Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                OnPropertyChanged();
                // Update command state on selected user change
                CommandManager.InvalidateRequerySuggested();
            }
        }
        public ClassesPg()
        {
            InitializeComponent();
            DataContext = this;
            Classes = new ObservableCollection<HR.Data.Models.ClassGuidance>();

            Loaded += Page_Loaded;

            DeleteItemCommand = new RelayCommand(
                execute: param =>
                {
                    if (param is HR.Data.Models.ClassGuidance toDelete)
                    {
                        var result = MessageBox.Show(
                            $"Вы действительно хотите удалить класс \"{toDelete.Title}\"?",
                            "Подтверждение удаления",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning
                        );

                        if (result == MessageBoxResult.Yes)
                        {
                            Classes.Remove(toDelete);
                            dataGrid.Items.Refresh(); // refresh auto numbering
                            // @TODO: Delete from DB
                            // await Services.Request.DeleteUser(userToDelete.Id);
                        }
                    }
                },
                canExecute: param =>
                {
                    return true;
                }
            );

            // Subscribe to navigation events for the main frame
            _navigationService = MainWindow.frame.NavigationService;
            if (_navigationService != null)
            {
                _navigationService.Navigated += NavigationService_Navigated;
            }
        }
        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            // Numbering from 1
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                var dg = sender as DataGrid;
                var toDelete = dg?.SelectedItem as HR.Data.Models.ClassGuidance;
                if (toDelete != null && DeleteItemCommand.CanExecute(toDelete))
                {
                    DeleteItemCommand.Execute(toDelete);
                    e.Handled = true;
                }
            }
        }
        private void NavigationService_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Content != this) return;
            var navigationParameter = e.ExtraData;

            // If there is a single FilterValue — use it
            if (navigationParameter is HR.Utilities.FilterValue filterValue)
            {
                HandleFilterValue(filterValue);
            }
            // If there is a collection of FilterValues — use it
            if (navigationParameter is IEnumerable<FilterValue> filterValues)
            {
                HandleFilterValues(filterValues);
            }
            // If there is NavigationData — retrieve Parameter
            else if (navigationParameter is NavigationData navData)
            {
                // If Parameter is a single FilterValue
                if (navData.Parameter is HR.Utilities.FilterValue filter)
                {
                    HandleFilterValue(filter);
                }
                // If Parameter is a collection of FilterValues
                else if (navData.Parameter is IEnumerable<HR.Utilities.FilterValue> filters)
                {
                    HandleFilterValues(filters);
                }
                else
                {
                    // Handling other type params, if necessary
                    MessageBox.Show("NavigationData.Parameter has unexpected type: " + navData.Parameter?.GetType().Name);
                }
            }
            else if (navigationParameter != null)
            {
                MessageBox.Show("ELSE: " + navigationParameter.GetType().Name);
            }
        }
        private void HandleFilterValue(Utilities.FilterValue filter)
        {
            // Param single value processing logic
        }

        private void HandleFilterValues(IEnumerable<FilterValue> filters)
        {
            // Param collection value processing logic
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var fromService = await Services.Request.GetClassGuidance();
            Classes = new ObservableCollection<ClassGuidance>(fromService);
            // Alternative way: without creating a new instance
            /*
            Classes.Clear();
            foreach (var item in fromService)
                Classes.Add(item);
            */
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
