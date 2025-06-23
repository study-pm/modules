using HR.Data.Models;
using HR.Services;
using HR.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
    /// Interaction logic for RegisterPg.xaml
    /// </summary>
    public partial class RegisterPg : Page, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        public static readonly DependencyProperty InProgressProp =
            DependencyProperty.Register(nameof(IsInProgress), typeof(bool), typeof(RegisterPg), new PropertyMetadata(false));

        public bool IsInProgress
        {
            get { return (bool)GetValue(InProgressProp); }
            set { SetValue(InProgressProp, value); }
        }

        private ObservableCollection<Employee> employees;
        public ObservableCollection<Employee> Employees
        {
            get => employees;
            set
            {
                if (employees == value) return;
                employees = value;
                OnPropertyChanged();
            }
        }

        private int? _employeeId;
        public int? EmployeeId
        {
            get => _employeeId;
            set
            {
                if (_employeeId != value)
                {
                    _employeeId = value;
                    OnPropertyChanged();
                }
            }
        }
        // --- Add for async validation ---
        private readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();
        public bool HasErrors => _errors.Count > 0;
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) return null;
            return _errors.ContainsKey(propertyName) ? _errors[propertyName] : null;
        }
        private void AddError(string propertyName, string error)
        {
            if (!_errors.ContainsKey(propertyName))
                _errors[propertyName] = new List<string>();
            if (!_errors[propertyName].Contains(error))
                _errors[propertyName].Add(error);
        }
        private void ClearErrors(string propertyName)
        {
            if (_errors.ContainsKey(propertyName))
                _errors.Remove(propertyName);
        }
        protected virtual void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
        // --- End async validation fields ---
        private string _login;
        public string Login
        {
            get => _login;
            set
            {
                if (_login != value)
                {
                    _login = value;
                    OnPropertyChanged();
                    _ = ValidateLoginAsync(); // ← Start async validation on change
                }
            }
        }
        private string _password1;
        public string Password1
        {
            get => _password1;
            set
            {
                if (_password1 == value) return;
                _password1 = value;
                OnPropertyChanged();
                ValidatePassword(Password1, Pw1Pwb);
            }
        }
        private bool _isPwd1On;
        public bool IsPwd1On
        {
            get => _isPwd1On;
            set
            {
                if (_isPwd1On == value) return;
                _isPwd1On = value;
                if (!_isPwd1On)
                {
                    // Update PasswordBox value when toggling its visibility on
                    Pw1Pwb.Password = Password1;
                }
                OnPropertyChanged(nameof(IsPwd1On));
            }
        }
        private string _password2;
        public string Password2
        {
            get => _password2;
            set
            {
                if (_password2 == value) return;
                _password2 = value;
                OnPropertyChanged();
                ValidatePassword(Password2, Pw2Pwb);
            }
        }
        private bool _isPwd2On;
        public bool IsPwd2On
        {
            get => _isPwd2On;
            set
            {
                if (_isPwd2On == value) return;
                _isPwd2On = value;
                if (!_isPwd2On)
                {
                    // Update PasswordBox value when toggling its visibility on
                    Pw2Pwb.Password = Password2;
                }
                OnPropertyChanged(nameof(IsPwd2On));
            }
        }
        public RegisterPg()
        {
            InitializeComponent();
            Employees = new ObservableCollection<Employee>(); // Инициализация пустой коллекции
            this.DataContext = this;
        }
        private async Task<bool> CheckLoginUniqueAsync(string login)
        {
            var user = await Services.Request.ctx.Users.FirstOrDefaultAsync(x => x.Login == login);
            return user == null;
        }
        private bool CheckPasswordMatch() => Password1 == Password2;
        public void ClearAllErrors()
        {
            ClearErrors(nameof(Login));
            ClearErrors(nameof(Password1));
            ClearErrors(nameof(Password2));
            ClearErrors(nameof(EmployeeId));
            // Raise ErrorsChanged for all properties cleared
            OnErrorsChanged(nameof(Login));
            OnErrorsChanged(nameof(Password1));
            OnErrorsChanged(nameof(Password2));
            OnErrorsChanged(nameof(EmployeeId));
        }
        private async Task<bool> Register()
        {
            byte[] salt = Crypto.GenerateSalt();
            byte[] hash = Crypto.HashPassword(Password1, salt);
            Debug.WriteLine("0x" + BitConverter.ToString(salt).Replace("-", ""));
            string hexString = BitConverter.ToString(hash).Replace("-", "");
            string sqlValue = "0x" + hexString;
            Debug.WriteLine(sqlValue);
            await Request.MockAsync(2000);
            throw new NotImplementedException("Register logic here");
        }
        private void Reset()
        {
            // Clear validation errors in ViewModel
            ClearAllErrors();

            // Clear data
            EmployeeId = null;
            Login = string.Empty;
            Password1 = string.Empty;
            Password2 = string.Empty;
            OnPropertyChanged(nameof(EmployeeId));
            OnPropertyChanged(nameof(Login));
            OnPropertyChanged(nameof(Password1));
            OnPropertyChanged(nameof(Password2));

            // Clear PasswordBoxes
            Pw1Pwb.Password = string.Empty;
            Pw2Pwb.Password = string.Empty;

            // Get controls list
            List<Control> controls = new List<Control> { EmployeeCmb, LoginTxb, Pw1Pwb, Pw2Pwb };

            // Clear control elements
            ValidationHelper.ResetInvalid(EmployeeCmb, TextBox.TextProperty);
            ValidationHelper.ResetInvalid(LoginTxb, TextBox.TextProperty);
            ValidationHelper.ResetInvalid(Pw1Pwb, PasswordBox.TagProperty);
            ValidationHelper.ResetInvalid(Pw2Pwb, PasswordBox.TagProperty);

            // Reset errors display
            ValidationHelper.ForceErrorsDisplay(controls, false);
        }
        private bool Validate()
        {
            // Sync Password property with PasswordBox
            Password1 = Pw1Pwb.Password;
            Password2 = Pw2Pwb.Password;

            // Get controls list
            List<Control> controls = new List<Control> { EmployeeCmb, LoginTxb, Pw1Pwb, Pw2Pwb };

            // Force errors display
            ValidationHelper.ForceErrorsDisplay(controls);

            // Force validate
            ValidationHelper.ForceValidate(EmployeeCmb, ComboBox.SelectedValueProperty);
            ValidationHelper.ForceValidate(LoginTxb, TextBox.TextProperty);
            // Force validate for password boxes
            ValidatePassword(Password1, Pw1Pwb);
            ValidatePassword(Password2, Pw2Pwb);

            // Get validity state
            Dictionary<Control, bool> validity = ValidationHelper.GetValidityState(controls);

            // Force update UI if error
            foreach (var state in validity)
                if (!state.Value) ValidationHelper.ForceUpdateUI(state.Key);

            // Display validation warning popup
            if (validity.Values.Contains(false))
            {
                ValidationMessage.Text = "Проверьте правильность введенных значений";
                ValidationPopup.IsOpen = true;
                return false;
            }
            return true;
        }
        private CancellationTokenSource _loginValidationCts;
        private async Task ValidateLoginAsync()
        {
            // Cancel any previous pending validation
            _loginValidationCts?.Cancel();
            _loginValidationCts = new CancellationTokenSource();
            var token = _loginValidationCts.Token;

            try
            {
                // Wait debounce delay
                await Task.Delay(250, token);

                ClearErrors(nameof(Login));

                bool isUnique = await CheckLoginUniqueAsync(Login);

                if (!isUnique)
                {
                    AddError(nameof(Login), "Этот логин уже занят");
                }

                OnErrorsChanged(nameof(Login));
            }
            catch (TaskCanceledException)
            {
                // Validation was cancelled due to new input, ignore
            }
        }
        private void ValidatePassword(string password, PasswordBox pwdBx)
        {
            var rules = new List<ValidationRule> {
                new PasswordLengthValidationRule { MinLength = 8 },
                new StrongPasswordValidationRule()
            };

            ValidationResult result = new ValidationResult(true, null);
            foreach (var rule in rules)
            {
                result = rule.Validate(password, CultureInfo.CurrentCulture);

                var bindingExpression = pwdBx.GetBindingExpression(PasswordBox.TagProperty);
                if (result.IsValid)
                {
                    Validation.ClearInvalid(bindingExpression);
                }
                else
                {
                    var validationError = new ValidationError(rule, bindingExpression)
                    {
                        ErrorContent = result.ErrorContent
                    };
                    Validation.MarkInvalid(bindingExpression, validationError);
                }
                if (!result.IsValid) break;
            }
        }
        private void Pw1Pwb_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var pb = (PasswordBox)sender;
            Password1 = pb.Password;
        }
        private void Pw2Pwb_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Password2 = ((PasswordBox)sender).Password;
        }
        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Reset();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void SignOnBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IsInProgress = true;
                if (!Validate()) return;
                if (!CheckPasswordMatch())
                {
                    MessageBox.Show("Пароли должны совпадать!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                StatusInformer.ReportProgress("Загрузка данных");
                bool isSuccess = await Register();
                if (isSuccess == false) StatusInformer.ReportFailure("Невозможно зарегистрировать пользователя");
                else StatusInformer.ReportSuccess("Успешная регистрация пользователя");
            }
            catch (Exception exc)
            {
                StatusInformer.ReportFailure("Невозможно зарегистрировать пользователя");
                MessageBox.Show(exc.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsInProgress = false;
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            IsInProgress = true;
            Employees = new ObservableCollection<Employee>((await Services.Request.GetEmployeesUnregistered()).OrderBy(emp => emp.Surname));
            IsInProgress = false;
        }
        private void TogglePwd1Btn_Click(object sender, RoutedEventArgs e)
        {
            IsPwd1On = !IsPwd1On;
            if (IsPwd1On)
            {
                Pw1Txt.Focus();
                ValidationHelper.SetShowErrors(Pw1Txt, true);
                ValidationHelper.SetTouched(Pw1Txt, true);
                ValidationHelper.ForceValidate(Pw1Txt, TextBox.TextProperty);
            }
            else
            {
                Pw1Pwb.Focus();
                ValidationHelper.SetShowErrors(Pw1Pwb, true);
                ValidationHelper.SetTouched(Pw1Pwb, true);
                ValidatePassword(Password1, Pw1Pwb);
            }
        }
        private void TogglePwd2Btn_Click(object sender, RoutedEventArgs e)
        {
            IsPwd2On = !IsPwd2On;
            if (IsPwd2On)
            {
                Pw2Txt.Focus();
                ValidationHelper.SetShowErrors(Pw2Txt, true);
                ValidationHelper.SetTouched(Pw2Txt, true);
                ValidationHelper.ForceValidate(Pw2Txt, TextBox.TextProperty);
            }
            else
            {
                Pw2Pwb.Focus();
                ValidationHelper.SetShowErrors(Pw2Pwb, true);
                ValidationHelper.SetTouched(Pw2Pwb, true);
                ValidatePassword(Password2, Pw2Pwb);
            }
        }
    }
}
