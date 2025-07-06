using HR.Data.Models;
using HR.Services;
using HR.Utilities;
using PdfSharp.Xps.XpsModel;
using QRCoder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Windows.Xps.Packaging;
using PdfSharp.Xps;
using System.Diagnostics;
using OtpNet;
using System.Windows.Media.Animation;
using static HR.Services.AppEventHelper;
using System.Globalization;
using System.Data.Entity.Validation;
using HR.Models;
using Microsoft.Win32;

namespace HR.Pages
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        internal HR.Data.Models.User dm;
        private string _code;
        public string Code
        {
            get => _code;
            set
            {
                if (_code == value) return;
                _code = value;
                OnPropertyChanged();
            }
        }
        public bool IsChanged => dm?.Is2faOn != Is2FA;
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
        private bool _is2fa;
        public bool Is2FA
        {
            get => _is2fa;
            set
            {
                if (_is2fa == value) return;
                _is2fa = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsChanged));
                OnPropertyChanged(nameof(IsEnabled));
            }
        }
        public bool IsSecret => Secret != null;
        private string secret;
        public string Secret
        {
            get => secret;
            set
            {
                if (secret == value) return;
                secret = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSecret));
            }
        }

        public SettingsViewModel(HR.Data.Models.User dataModel)
        {
            dm = dataModel;
            Reset();
        }
        public void Reset()
        {
            Is2FA = dm.Is2faOn;
            Secret = dm.Secret;
        }
        public void Set()
        {
            dm.Is2faOn = Is2FA;
            dm.Secret = Secret;
            OnPropertyChanged(nameof(IsChanged));
            OnPropertyChanged(nameof(IsEnabled));
        }
    }
    /// <summary>
    /// Interaction logic for SettingsPg.xaml
    /// </summary>
    public partial class SettingsPg : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        private const double MinColumnWidth = 350;

        public RelayCommand ResetCmd { get; }
        public RelayCommand SubmitCmd { get; }

        private Data.Models.User user = ((App)(Application.Current)).CurrentUser;

        private byte[] secretBytes;
        private string secret;
        public string Secret
        {
            get => secret;
            set
            {
                if (secret == value) return;
                secret = value;
                OnPropertyChanged();
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
                OnPropertyChanged();
                CheckPassword();
            }
        }
        private bool _isPwdValid;
        public bool IsPwdValid
        {
            get => _isPwdValid;
            set
            {
                if (_isPwdValid == value) return;
                _isPwdValid = value;
                OnPropertyChanged(nameof(IsPwdValid));
            }
        }
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
                    PwdPwb.Password = Password;
                }
                OnPropertyChanged(nameof(IsPwdOn));
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

        private SettingsViewModel _vm;
        public SettingsViewModel vm
        {
            get => _vm;
            set
            {
                _vm = value;
                OnPropertyChanged();
            }
        }
        private readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        protected virtual void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
        public SettingsPg()
        {
            InitializeComponent();
            this.SizeChanged += SettingsPg_SizeChanged;
            Loaded += SettingsPg_Loaded;
            vm = new SettingsViewModel(user);
            DataContext = this;

            ResetCmd = new RelayCommand(
                _ => Reset(),
                _ => !vm.IsProgress && IsPwdValid
            );
            SubmitCmd = new RelayCommand(
                _ =>
                {
                    if (!Validate()) return;
                    if (!CheckPasswordMatch())
                    {
                        MessageBox.Show("Пароли должны совпадать!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var (hash, salt) = Cypher(Password1);
                    user.PasswordHash = hash;
                    user.Salt = salt;

                    SavePassword();
                },
                _ => !vm.IsProgress && IsPwdValid
            );
        }
        private void AdjustGridLayout(double availableWidth)
        {
            MainGrid.RowDefinitions.Clear();
            MainGrid.ColumnDefinitions.Clear();

            if (availableWidth >= MinColumnWidth * 2)
            {
                // Две колонки, одна строка
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                Grid.SetRow(FirstSection, 0);
                Grid.SetColumn(FirstSection, 0);

                Grid.SetRow(SecondSection, 0);
                Grid.SetColumn(SecondSection, 1);
            }
            else
            {
                // Одна колонка, две строки
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                Grid.SetRow(FirstSection, 0);
                Grid.SetColumn(FirstSection, 0);

                Grid.SetRow(SecondSection, 1);
                Grid.SetColumn(SecondSection, 0);
            }
        }
        private void CheckPassword()
        {
            var hash = Crypto.HashPassword(Password, user.Salt);
            Debug.WriteLine("0x" + BitConverter.ToString(hash).Replace("-", ""));
            var bindingExpression = PwdPwb.GetBindingExpression(PasswordBox.TagProperty);
            if (bindingExpression == null) return;

            if (hash.SequenceEqual(user.PasswordHash))
            {
                Validation.ClearInvalid(bindingExpression);
                ValidationHelper.SetShowErrors(PwdPwb, false);
                IsPwdValid = true;
            }
            else
            {
                var rule = new PasswordLengthValidationRule();
                var validationError = new ValidationError(rule, bindingExpression)
                {
                    ErrorContent = "Неверный пароль"
                };
                Validation.MarkInvalid(bindingExpression, validationError);

                ValidationHelper.SetShowErrors(PwdPwb, true);
                ValidationHelper.SetTouched(PwdPwb, true);
                IsPwdValid = false;
            }
        }
        private bool CheckPasswordMatch() => Password1 == Password2;
        private (byte[] hash, byte[] salt) Cypher(string password)
        {
            byte[] salt = Crypto.GenerateSalt();
            byte[] hash = Crypto.HashPassword(password, salt);
            string hexString = BitConverter.ToString(hash).Replace("-", "");
            string sqlValue = "0x" + hexString;
            return (hash, salt);
        }
        public void ClearAllErrors()
        {
            ClearErrors(nameof(Password));
            ClearErrors(nameof(Password1));
            ClearErrors(nameof(Password2));
            // Raise ErrorsChanged for all properties cleared
            OnErrorsChanged(nameof(Password));
            OnErrorsChanged(nameof(Password1));
            OnErrorsChanged(nameof(Password2));
        }
        private void ClearErrors(string propertyName)
        {
            if (_errors.ContainsKey(propertyName))
                _errors.Remove(propertyName);
        }
        private async Task<string> GetEncryptedSecret(string plainText)
        {
            var (cat, name, op, scope) = (EventCategory.Data, "Auth", 1, "Пользователь");
            try
            {
                vm.IsProgress = true;
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat,
                    Name = name,
                    Op = op,
                    Scope = scope,
                    Type = EventType.Progress,
                    Message = "Шифрование данных"
                });
                string basePath = Fs.GetFullRootPath(Crypto.keysPath);
                var (key, iv) = await Fs.LoadKeysParallelAsync(basePath, "aes_key.bin", "aes_iv.bin");
                string encrypted = Crypto.Encrypt(plainText, key, iv);
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat,
                    Name = name,
                    Op = op,
                    Scope = scope,
                    Type = EventType.Success,
                    Message = "Данные успешно зашифрованы"
                });
                return encrypted;
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc, "SettingsPg");
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat,
                    Name = name,
                    Op = op,
                    Scope = scope,
                    Type = EventType.Error,
                    Message = "Ошибка шифрования данных",
                    Details = exc.Message
                });
                MessageBox.Show($"Ошибка шифрования данных: {exc.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return "";
            }
            finally
            {
                vm.IsProgress = false;
            }
        }
        private void Reset()
        {
            // Clear validation errors in ViewModel
            ClearAllErrors();

            // Clear data
            Password = string.Empty;
            Password1 = string.Empty;
            Password2 = string.Empty;
            OnPropertyChanged(nameof(Password));
            OnPropertyChanged(nameof(Password1));
            OnPropertyChanged(nameof(Password2));

            // Clear PasswordBoxes
            PwdPwb.Password = string.Empty;
            Pw1Pwb.Password = string.Empty;
            Pw2Pwb.Password = string.Empty;

            // Get controls list
            List<Control> controls = new List<Control> { PwdPwb, Pw1Pwb, Pw2Pwb };

            // Clear control elements
            ValidationHelper.ResetInvalid(PwdPwb, PasswordBox.TagProperty);
            ValidationHelper.ResetInvalid(Pw1Pwb, PasswordBox.TagProperty);
            ValidationHelper.ResetInvalid(Pw2Pwb, PasswordBox.TagProperty);

            // Reset errors display
            ValidationHelper.ForceErrorsDisplay(controls, false);
        }
        private async Task<bool> Save()
        {
            var (cat, name, op, scope) = (EventCategory.Auth, "Update", 2, "Пользователь");
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
                return true;
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc, "SettingsPg");
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
                return false;
            }
            finally
            {
                vm.IsProgress = false;
            }
        }
        private async void SavePassword()
        {
            bool res = await Save();
            if (res)
            {
                Reset();
                MessageBox.Show("Новый пароль успешно сохранен. Для входа в приложение используйте новый пароль.", "Смена пароля", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void SetQrCode(byte[] qrCodeBytes)
        {
            imgQrCode.Source = Fs.LoadImage(qrCodeBytes);
            imgQrCode.Visibility = Visibility.Visible;
        }
        private bool Validate()
        {
            // Sync Password property with PasswordBox
            Password1 = Pw1Pwb.Password;
            Password2 = Pw2Pwb.Password;

            // Get controls list
            List<Control> controls = new List<Control> { Pw1Pwb, Pw2Pwb };

            // Force errors display
            ValidationHelper.ForceErrorsDisplay(controls);

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

        private void GetQrBtn_Click(object sender, RoutedEventArgs e)
        {
            // Generate secret
            (secretBytes, Secret) = Crypto.GenerateSecret();
            // Get and display QR code in Image
            SetQrCode(Utils.GetQrCode(secretBytes, user.Login));
        }
        private void SettingsPg_Loaded(object sender, RoutedEventArgs e)
        {
            AdjustGridLayout(this.ActualWidth);
        }

        private void SettingsPg_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdjustGridLayout(e.NewSize.Width);
        }

        private async void CodeTxb_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txb = (TextBox)sender;
            if (Validation.GetHasError(txb)) return;

            try
            {
                var totp = new Totp(secretBytes);
                bool isValid = totp.VerifyTotp(vm.Code, out long timeStepMatched, new VerificationWindow(2, 2));
                if (isValid)
                {
                    string encryptedSecret = await GetEncryptedSecret(Secret);
                    if (string.IsNullOrWhiteSpace(encryptedSecret)) return;

                    vm.Secret = encryptedSecret;
                    vm.Is2FA = true;
                    bool res = await Save();
                    if (!res) return;

                    StatusInformer.ReportSuccess($"Успешное подключение 2FA");
                    MessageBox.Show("Двухфакторная аутентификация успешно подключена!\n\n" +
                        "Теперь ваша учетная запись защищена дополнительным уровнем безопасности.\n\n" +
                        "• При каждом входе в систему после ввода пароля вам потребуется ввести шестизначный код из приложения Authenticator на вашем смартфоне.\n" +
                        "• Код обновляется каждые 30 секунд, так что вводите актуальный.\n\n" +
                        "Важные советы:\n" +
                        "• Сохраните резервные коды в надёжном месте для восстановления доступа при потере телефона.\n" +
                        "• Не теряйте телефон и при смене перенесите аккаунт в новое приложение.\n" +
                        "• Никому не сообщайте свои коды из Authenticator.",
                        "Успешная регистрация 2FA",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    StatusInformer.ReportFailure($"Ошибка проверки кода регистрации 2FA");
                    MessageBox.Show("Ошибка проверки кода двухфакторной аутентификации.\n\n" +
                        "Введённый код неверен. Проверьте код из приложения Google Authenticator и попробуйте снова.\n\n" +
                        "• Убедитесь, что время на вашем устройстве синхронизировано и установлено правильно.\n" +
                        "• Введите актуальный шестизначный код, который обновляется каждые 30 секунд.\n" +
                        "• Если проблема сохраняется, попробуйте заново отсканировать QR-код и повторить регистрацию.",
                        "Ошибка 2FA",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception exc)
            {
                StatusInformer.ReportFailure($"Непредвиденная ошибка при проверке кода регистрации 2FA: {exc.Message.ToString()}");
                MessageBox.Show("Произошла непредвиденная ошибка при проверке кода: " + exc.Message.ToString() +
                    "Попробуйте повторить попытку позже.\n\n" +
                    "Если проблема сохраняется, обратитесь в службу поддержки для получения помощи.",
                    "Ошибка 2FA",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void CopySecretBtn_Click(object sender, RoutedEventArgs e)
        {
            // Select all text in TextBox
            SecretTxb.Focus();
            SecretTxb.SelectAll();

            // Copy text to clipboard
            Clipboard.SetText(SecretTxb.Text);

            // Appear animation
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            CopiedTextBlock.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            // Parallel hover and disappear animation
            var moveUp = new DoubleAnimation(0, -20, TimeSpan.FromMilliseconds(800)) { BeginTime = TimeSpan.Zero };
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(800)) { BeginTime = TimeSpan.Zero };

            // Apply animations
            CopiedTextBlock.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            (CopiedTextBlock.RenderTransform as TranslateTransform)?.BeginAnimation(TranslateTransform.YProperty, moveUp);

            // Timeout before hiding
            await Task.Delay(1000);

            // Hiding animation
            CopiedTextBlock.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }
        private void PwdPwb_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var pb = (PasswordBox)sender;
            Password = pb.Password;
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
        private void TipBtn_Click(object sender, RoutedEventArgs e)
        {
            TipPopup.IsOpen = !TipPopup.IsOpen; // переключаем видимость Popup
        }
        private void TogglePwdBtn_Click(object sender, RoutedEventArgs e)
        {
            IsPwdOn = !IsPwdOn;
            if (IsPwdOn)
            {
                PwdTxt.Focus();
                ValidationHelper.SetShowErrors(PwdTxt, true);
                ValidationHelper.SetTouched(PwdTxt, true);
                ValidationHelper.ForceValidate(PwdTxt, TextBox.TextProperty);
            }
            else
            {
                PwdPwb.Focus();
                ValidationHelper.SetShowErrors(PwdPwb, true);
                ValidationHelper.SetTouched(PwdPwb, true);
                ValidatePassword(Password, PwdPwb);
            }
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

        private async void TfaChbx_Click(object sender, RoutedEventArgs e)
        {
            await Save();
        }
    }
}
