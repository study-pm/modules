using HR.Data.Models;
using HR.Models;
using HR.Services;
using HR.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
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
    /// Interaction logic for AuthPg.xaml
    /// </summary>
    public partial class AuthPg : Page, INotifyPropertyChanged
    {
        public static readonly DependencyProperty LoginErrProp =
            DependencyProperty.Register(nameof(IsLoginErr), typeof(bool), typeof(AuthPg), new PropertyMetadata(false));
        public bool IsLoginErr
        {
            get { return (bool)GetValue(LoginErrProp); }
            set { SetValue(LoginErrProp, value); }
        }
        public static readonly DependencyProperty InProgressProp =
            DependencyProperty.Register(nameof(IsInProgress), typeof(bool), typeof(AuthPg), new PropertyMetadata(false));
        public bool IsInProgress
        {
            get { return (bool)GetValue(InProgressProp); }
            set { SetValue(InProgressProp, value); }
        }
        public static readonly DependencyProperty LoginProp =
            DependencyProperty.Register(nameof(LoginProp), typeof(bool), typeof(AuthPg), new PropertyMetadata(false));
        public string Login { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private bool _isPwdOn;
        public bool IsPwdOn
        {
            get => _isPwdOn;
            set
            {
                if (_isPwdOn == value) return;
                _isPwdOn = value;
                if (!_isPwdOn)
                {
                    // Update PasswordBox value when toggling its visibility on
                    PasswordPwb.Password = Password;
                }
                OnPropertyChanged(nameof(IsPwdOn));
            }
        }
        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                if (_password == value) return;
                _password = value;
                OnPropertyChanged(nameof(Password));
                ValidatePassword();
            }
        }
        public AuthPg()
        {
            InitializeComponent();
            DataContext = this;
        }
        private async Task<HR.Data.Models.User> GetUser(string login, string password)
        {
            try
            {
                // Check for user with provided login existence
                HR.Data.Models.User user = await Request.ctx.Users.FirstOrDefaultAsync(x => x.Login == login);
                if (user == null)
                {
                    StatusInformer.ReportWarning("Попытка входа в систему. Несоответствие учетного имени");
                    MessageBox.Show("Пользователь не обнаружен. Проверьте правильность введения учетного имени.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return null;
                }
                // Check for user password match
                Debug.WriteLine("0x" + BitConverter.ToString(user.PasswordHash).Replace("-", ""));
                var hash = Crypto.HashPassword(password, user.Salt);
                Debug.WriteLine("0x" + BitConverter.ToString(hash).Replace("-", ""));
                // Correct comparison of byte arrays by content
                if (!hash.SequenceEqual(user.PasswordHash))
                {
                    StatusInformer.ReportWarning("Попытка входа в систему. Несоответствие пароля");
                    MessageBox.Show("Пароль не подходит. Проверьте правильность введения пароля.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return null;
                }
                StatusInformer.ReportSuccess("Успешный вход в систему");
                return user;
            }
            catch (Exception exc)
            {
                StatusInformer.ReportFailure($"Неизвестная ошибка: {exc.Message}");
                MessageBox.Show(exc.Message, "Неизвестная ошибка. Попробуйте войти позже.", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
        private void ValidatePassword()
        {
            var rule = new NotEmptyValidationRule();
            var result = rule.Validate(Password, CultureInfo.CurrentCulture);

            var bindingExpression = PasswordPwb.GetBindingExpression(PasswordBox.TagProperty);
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
        }
        public void ResetFields()
        {
            // 1. Очистка свойств
            Login = string.Empty;
            Password = string.Empty;
            OnPropertyChanged(nameof(Login));
            OnPropertyChanged(nameof(Password));

            // 2. Очистка TextBox
            LoginTxb.Text = string.Empty;

            // 3. Очистка PasswordBox
            PasswordPwb.Password = string.Empty;

            // 4. Сброс ошибок валидации для TextBox
            var loginBinding = LoginTxb.GetBindingExpression(TextBox.TextProperty);
            if (loginBinding != null)
            {
                Validation.ClearInvalid(loginBinding);
                loginBinding.UpdateSource();
            }

            // 5. Сброс ошибок валидации для PasswordBox (вы используете TagProperty для binding)
            var passwordBinding = PasswordPwb.GetBindingExpression(PasswordBox.TagProperty);
            if (passwordBinding != null)
            {
                Validation.ClearInvalid(passwordBinding);
                passwordBinding.UpdateSource();
            }

            // Errors reset
            HR.Utilities.ValidationHelper.SetShowErrors(LoginTxb, true);
            HR.Utilities.ValidationHelper.SetShowErrors(PasswordPwb, true);

            // Touched reset
            HR.Utilities.ValidationHelper.SetTouched(LoginTxb, false);
            HR.Utilities.ValidationHelper.SetTouched(PasswordPwb, false);
        }
        private void PasswordPwb_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var pb = (PasswordBox)sender;
            // Например, если DataContext - ваша ViewModel:
            if (DataContext is AuthPg vm)
                vm.Password = pb.Password;
            // the same as: Password = PassPwb.Password;
        }
        private async void SignInBtn_Click(object sender, RoutedEventArgs e)
        {
            // 0. Синхронизируем Password свойство с PasswordBox
            Password = PasswordPwb.Password;

            // 1. Принудительно показываем ошибки (эмулируем "Touched" и "ShowErrors")
            HR.Utilities.ValidationHelper.SetShowErrors(LoginTxb, true);
            HR.Utilities.ValidationHelper.SetShowErrors(PasswordPwb, true);
            HR.Utilities.ValidationHelper.SetTouched(LoginTxb, true);
            HR.Utilities.ValidationHelper.SetTouched(PasswordPwb, true);

            // 2. Принудительно обновляем биндинги (если вдруг не обновились)
            ValidationHelper.ForceValidate(LoginTxb, TextBox.TextProperty);
            // ValidationHelper.ForceValidate(PasswordPwb, PasswordBox.TagProperty); -- это не делаем, т.к. Password валидируется отдельно
            // ЯВНО вызываем ValidatePassword, чтобы ошибка обновилась
            ValidatePassword();

            // 3. Проверяем наличие ошибок
            bool hasLoginError = Validation.GetHasError(LoginTxb);
            bool hasPasswordError = Validation.GetHasError(PasswordPwb);

            // Принудительно обновляем UI в случае ошибок
            if (hasLoginError)
            {
                LoginTxb.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Render);
                HR.Utilities.ValidationHelper.SetErrorTextBlockVisibility(LoginTxb, Visibility.Visible);
            }
            if (hasPasswordError)
            {
                PasswordPwb.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Render);
                HR.Utilities.ValidationHelper.SetErrorTextBlockVisibility(PasswordPwb, Visibility.Visible);
            }

            if (hasLoginError || hasPasswordError)
            {
                ValidationMessage.Text = "Проверьте правильность введенных значений";
                ValidationPopup.IsOpen = true;
                return;
            }

            // 4. Если ошибок нет - выполняем вход
            IsInProgress = true;
            StatusInformer.ReportProgress("Проверка данных пользователя");
            App app = Application.Current as App;
            app.CurrentUser = await GetUser(Login, Password);
            if (app.CurrentUser == null)
            {
                IsInProgress = false;
                return;
            }
            // Read and apply user preferences
            Preferences preferences = await Services.Request.GetPreferences(app.CurrentUser.Id);
            if (preferences.IsStayLoggedIn) await Services.Request.SaveUidToFileAsync(app.CurrentUser.Id, Data.Models.User.uidFilePath);
            IsInProgress = false;
            // @TODO: Go to Startup page
            if (app.IsAuth == true) MainWindow.frame.Navigate(new PreferencesPg());
        }
        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            ResetFields();
        }

        private void TogglePwdBtn_Click(object sender, RoutedEventArgs e) => IsPwdOn = !IsPwdOn;
    }
}
