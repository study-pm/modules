using HR.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
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
    public partial class AuthPg : Page
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

        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                ValidatePassword();
            }
        }
        public AuthPg()
        {
            InitializeComponent();
            DataContext = this;
        }
        private async Task<int?> SignIn()
        {
            // Mock checking user existences from DB
            await Utils.MockAsync(3000);
            return 1;
        }
        private void ValidatePassword()
        {
            var rule = new PasswordLengthValidationRule { MinLength = 8 };
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

            // Сброс "Touched"
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
            IsInProgress = true;
            int? uid = await SignIn();
            IsInProgress = false;
        }
        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            ResetFields();
        }

    }
}
