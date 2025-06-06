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
    /// Interaction logic for UsersPg.xaml
    /// </summary>
    public partial class UsersPg : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        public ICommand DeleteUserCommand { get; }

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

            DeleteUserCommand = new RelayCommand(
                execute: param =>
                {
                    if (param is HR.Data.Models.User userToDelete)
                    {
                        var result = MessageBox.Show(
                            $"Вы действительно хотите удалить пользователя \"{userToDelete.Login}\"?",
                            "Подтверждение удаления",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning
                        );

                        if (result == MessageBoxResult.Yes)
                        {
                            Users.Remove(userToDelete);
                            // @TODO: Delete from DB
                            // await Services.Request.DeleteUser(userToDelete.Id);
                        }
                    }
                },
                canExecute: param =>
                {
                    if (param is HR.Data.Models.User userToDelete)
                    {
                        // Checking if not a current user
                        return userToDelete.Id != user.Id;
                    }
                    return false;
                }
            );
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var usersFromService = await Services.Request.GetUsers();
            Users.Clear();
            foreach (var user in usersFromService)
                Users.Add(user);
        }
        private void UsersDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            // Numbering from 1
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
        private void UsersDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                var dg = sender as DataGrid;
                var userToDelete = dg?.SelectedItem as HR.Data.Models.User;
                if (userToDelete != null && DeleteUserCommand.CanExecute(userToDelete))
                {
                    DeleteUserCommand.Execute(userToDelete);
                    e.Handled = true;
                }
            }
        }
    }
}
