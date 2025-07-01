using HR.Data.Models;
using HR.Models;
using HR.Services;
using HR.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
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
using static HR.Services.AppEventHelper;

namespace HR.Pages
{
    public class UserViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        internal Data.Models.User dm;

        public string EmployeeName => dm.Employee?.FullName ?? "—";
        private Employee _employee;
        public Employee Employee
        {
            get => _employee;
            set
            {
                if (_employee == value) return;
                _employee = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsChanged));
                OnPropertyChanged(nameof(IsEnabled));
            }
        }
        public ObservableCollection<Employee> Employees { get; set; }

        public bool HasImage => Image != null;

        private string _image;
        public string Image
        {
            get => _image;
            set
            {
                if (_image == value) return;
                _image = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsChanged));
                OnPropertyChanged(nameof(IsEnabled));
                OnPropertyChanged(nameof(HasImage));
            }
        }

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

        public bool IsChanged => dm.Login != Login;
        public bool IsEnabled => IsChanged && !IsProgress;
        private bool _isProgress;
        public bool IsProgress
        {
            get => _isProgress;
            set
            {
                if (_isProgress == value) return;
                _isProgress = value;
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
            Employee = dm.Employee;
            Image = dm.Image;
            Login = dm.Login;
            Role = dm.Role;
        }
        public async Task InitializeAsync()
        {
            IsProgress = true;
            Employees = new ObservableCollection<Employee>(await Services.Request.GetEmployees());
            Roles = new ObservableCollection<Role>(await Services.Request.GetRoles());
            OnPropertyChanged(nameof(Roles));
            IsProgress = false;
        }
        public void Reset()
        {
            Employee = dm.Employee;
            Image = dm.Image;
            Login = dm.Login;
            Role = dm.Role;
        }
        public void Set()
        {
            dm.Image = Image;
            dm.Login = Login;
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

        public RelayCommand AddImgCmd { get; }
        public RelayCommand ResetCmd { get; }
        public RelayCommand SubmitCmd { get; }

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

            AddImgCmd = new RelayCommand(
                _ =>
                {
                    ChangeImage();
                    Save();
                },
                _ => !vm.IsProgress
            );
            ResetCmd = new RelayCommand(
                _ => {
                    vm.Reset();
                },
                _ => vm.IsEnabled
            );
            SubmitCmd = new RelayCommand(
                _ => Save(),
                _ => vm.IsEnabled
            );
        }
        private void ChangeImage()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Выберите изображение";
            ofd.Filter = "Изображения (*.jpeg;*.png;*.gif;*jpg)|*.jpeg;*.png;*.gif;*jpg";
            if (ofd.ShowDialog() != true)
                return;
            string imgPath = ofd.FileName;
            string imgFullName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(imgPath).ToLower();
            ImgHelper.SetPreviewImage(imgPath, UserImg);
            vm.Image = ImgHelper.SaveImg(imgPath, 40);
        }
        private void EnlargeImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn)) return;

            ImgHelper.OpenImgPopup(btn);
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
        private async void Save()
        {
            var (cat, name, op, scope) = (EventCategory.Data, "Update", 2, "Профиль пользователя");
            try
            {
                vm.IsProgress = true;
                vm.Set();
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat,
                    Name = name,
                    Op = op,
                    Scope = scope,
                    Type = EventType.Progress,
                    Message = "Обновление данных"
                });
                await Request.ctx.SaveChangesAsync();
                App.Current.OnPropertyChanged(nameof(App.CurrentUser)); // Notify app about changes
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat,
                    Name = name,
                    Op = op,
                    Scope = scope,
                    Type = EventType.Success,
                    Message = "Данные успешно обновлены"
                });
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc, "ProfilePg");
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat,
                    Name = name,
                    Op = op,
                    Scope = scope,
                    Type = EventType.Error,
                    Message = "Ошибка обновления данных",
                    Details = exc.Message
                });
                MessageBox.Show($"Ошибка обновления данных: {exc.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                vm.IsProgress = false;
            }
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
