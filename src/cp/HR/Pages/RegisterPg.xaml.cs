using HR.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
    /// Interaction logic for RegisterPg.xaml
    /// </summary>
    public partial class RegisterPg : Page, INotifyPropertyChanged
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
        public static readonly DependencyProperty LoginProp =
            DependencyProperty.Register(nameof(LoginProp), typeof(bool), typeof(RegisterPg), new PropertyMetadata(false));
        public string Login { get; set; }

        public static readonly DependencyProperty UserIdProp =
            DependencyProperty.Register(nameof(UserIdProp), typeof(bool), typeof(RegisterPg), new PropertyMetadata(false));
        public int? UserId { get; set; }
        private string _password1;
        public string Password1
        {
            get => _password1;
            set
            {
                _password1 = value;
                ValidatePassword(Password1, Pw1Pwb);
            }
        }
        private string _password2;
        public string Password2
        {
            get => _password2;
            set
            {
                _password2 = value;
                ValidatePassword(Password2, Pw2Pwb);
            }
        }
        public RegisterPg()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        private async Task<bool> Register()
        {
            await Utils.MockAsync(2000);
            throw new NotImplementedException("Register logic here");
        }
        private void Reset()
        {
            // Clear data
            UserId = null;
            Login = string.Empty;
            Password1 = string.Empty;
            Password2 = string.Empty;
            OnPropertyChanged(nameof(UserId));
            OnPropertyChanged(nameof(Login));
            OnPropertyChanged(nameof(Password1));
            OnPropertyChanged(nameof(Password2));

            // Get controls list
            List<Control> controls = new List<Control> { UserIdTxb, LoginTxb, Pw1Pwb, Pw2Pwb };

            // Clear control elements
            ValidationHelper.ResetInvalid(UserIdTxb, TextBox.TextProperty);
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
            Password2 = Pw1Pwb.Password;

            // Get controls list
            List<Control> controls = new List<Control> { UserIdTxb, LoginTxb, Pw1Pwb, Pw2Pwb };

            // Force errors display
            ValidationHelper.ForceErrorsDisplay(controls);

            // Force validate
            ValidationHelper.ForceValidate(UserIdTxb, TextBox.TextProperty);
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
        private void ValidatePassword(string password, PasswordBox pwdBx)
        {
            var rule = new PasswordLengthValidationRule { MinLength = 8 };
            var result = rule.Validate(password, CultureInfo.CurrentCulture);

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
        }
        private void Pw1Pwb_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var pb = (PasswordBox)sender;
            if (DataContext is RegisterPg vm) vm.Password1 = pb.Password;
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
    }
}
