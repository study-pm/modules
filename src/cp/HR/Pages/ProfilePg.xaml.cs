using HR.Data.Models;
using HR.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
    public class UserViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        internal Data.Models.User dm;
        public string EmployeeName => dm.Employee?.FullName ?? "—";
        private string _login;
        public string Login
        {
            get => _login;
            set
            {
                if (_login == value) return;
                _login = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsChanged));
                OnPropertyChanged(nameof(IsEnabled));
            }
        }
        private Role _role;
        public Role Role
        {
            get => _role;
            set
            {
                if (_role == value) return;
                _role = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsChanged));
                OnPropertyChanged(nameof(IsEnabled));
            }
        }
        public ObservableCollection<Role> Roles { get; set; }
        public bool IsChanged => dm.Role != Role;
        public bool IsEnabled => IsChanged && !IsInProgress;
        private bool _isInProgress;
        public bool IsInProgress
        {
            get => _isInProgress;
            set
            {
                if (_isInProgress == value) return;
                _isInProgress = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEnabled));
            }
        }
        private bool _isRoleChange;
        public bool IsRoleChange
        {
            get => _isRoleChange;
            set
            {
                if (_isRoleChange == value) return;
                _isRoleChange = value;
                OnPropertyChanged();
            }
        }
        public UserViewModel() { }
        public UserViewModel(Data.Models.User dataModel)
        {
            dm = dataModel;
            Role = dm.Role;
        }
        public async Task InitializeAsync()
        {
            IsInProgress = true;
            Roles = new ObservableCollection<Role>(await Services.Request.GetRoles());
            OnPropertyChanged(nameof(Roles));
            IsInProgress = false;
        }
        public void Reset()
        {
            Login = dm.Login;
            Role = dm.Role;
        }
        public void Set()
        {
            dm.Login = Login;
            dm.Role = Role;
            OnPropertyChanged(nameof(IsChanged));
            OnPropertyChanged(nameof(IsEnabled));
        }
    }
    /// <summary>
    /// Interaction logic for ProfilePg.xaml
    /// </summary>
    public partial class ProfilePg : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        private NavigationService _navigationService;

        private Data.Models.User user = ((App)(Application.Current)).CurrentUser;
        private UserViewModel _vm;
        public UserViewModel vm
        {
            get => _vm;
            set
            {
                _vm = value;
                OnPropertyChanged();
            }
        }
        private bool isPwdChange;
        public bool IsPwdChange
        {
            get => isPwdChange;
            set
            {
                if (value == isPwdChange) return;
                isPwdChange = value;
                OnPropertyChanged();
            }
        }
        private bool isRoleEdit;
        public bool IsRoleEdit
        {
            get => isRoleEdit;
            set
            {
                if (value == isRoleEdit) return;
                isRoleEdit = value;
                OnPropertyChanged();
            }
        }
        public ProfilePg()
        {
            InitializeComponent();
            Unloaded += Page_Unloaded;

            vm = new UserViewModel(user);
            DataContext = vm;
        }
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
        private void RoleChangeBtn_Click(object sender, RoutedEventArgs e) => vm.IsRoleChange = true;

        private void RoleCancelIconBtn_Click(object sender, RoutedEventArgs e)
        {
            vm.IsRoleChange = false;
        }

        private void RoleSubmitIconBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Заявка на изменение роли отправлена администратору", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            vm.IsRoleChange = false;
        }

        private void ChangePwdLink_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NavigationService_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Content == this) return; // Skip if navigation to current page

            if (!vm.IsChanged) return;
            var result = MessageBox.Show("Форма содержит несохраненные изменения. Вы действительно хотите уйти без сохранения данных?", "Несохраненные изменения", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
                e.Cancel = true; // Cancel navigation
        }
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_navigationService != null)
            {
                _navigationService.Navigating -= NavigationService_Navigating;
                _navigationService = null;
            }
        }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            vm = new UserViewModel(user);
            await vm.InitializeAsync();
            DataContext = vm;
            vm.Reset();
        }
    }
}
