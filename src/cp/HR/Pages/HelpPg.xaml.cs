using HR.Controls;
using HR.Data.Models;
using HR.Services;
using HR.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// Interaction logic for HelpPg.xaml
    /// </summary>
    public partial class HelpPg : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        public ICommand DeleteItemCommand { get; }
        public ICommand NavigateItemCommand { get; }

        private NavigationService _navigationService;

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

            DeleteItemCommand = new RelayCommand(
                execute: param =>
                {
                    if (param is HR.Data.Models.Employee item)
                    {
                        var result = MessageBox.Show(
                            $"Вы действительно хотите удалить сотрудника \"{item.FullName}\"?",
                            "Подтверждение удаления",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning
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
                    return true;
                }
            );
            NavigateItemCommand = new RelayCommand(
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
                canExecute: param => true
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
            var dg = sender as DataGrid;
            var item = dg?.SelectedItem as HR.Data.Models.Employee;
            if (e.Key == Key.Delete)
            {
                if (item != null && DeleteItemCommand.CanExecute(item))
                {
                    DeleteItemCommand.Execute(item);
                    e.Handled = true;
                }
            }
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (item != null && NavigateItemCommand.CanExecute(item))
                {
                    NavigateItemCommand.Execute(item);
                    e.Handled = true;
                }
            }
            if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (item != null && NavigateItemCommand.CanExecute(item))
                {
                    NavigateItemCommand.Execute(null);
                    e.Handled = true;
                }
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
