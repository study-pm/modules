using RecipeBook.AppData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Xml;

namespace RecipeBook.Pages
{
    /// <summary>
    /// Interaction logic for RegPg.xaml
    /// </summary>
    public partial class RegPg : Page
    {
        public RegPg()
        {
            InitializeComponent();

            dtpBirth.DisplayDateEnd = DateTime.Now.AddYears(-14);
            dtpBirth.DisplayDateStart = DateTime.Now.AddYears(-120);
        }

        private bool checkFormValidity()
        {
            if (!Validity.checkFilled(txbName.Text))
            {
                MessageBox.Show("Поле 'Имя' должно быть заполнено", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!Validity.checkFilled(txbLogin.Text))
            {
                MessageBox.Show("Поле 'Логин' должно быть заполнено", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!Validity.checkFilled(psbPass1.Password))
            {
                MessageBox.Show("Поле 'Пароль' должно быть заполнено", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (psbPass1.Password != psbPass2.Password)
            {
                MessageBox.Show("Пароли должны совпадать", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!Validity.checkPass(psbPass1.Password))
            {
                MessageBox.Show("Пароль должен соответствовать требованиям безопасности: содержать не менее 10 символов, " +
                    "включая латинские буквы в верхнем и нижнем регистрах, цифры и специальные символоы",
                    "Уведомление", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (Validity.checkFilled(txbExp.Text) && !Validity.checkWhole(txbExp.Text))
            {
                MessageBox.Show("Стаж должен составлять целое число лет", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (Validity.checkFilled(txbEmail.Text) && !Validity.checkEmail(txbEmail.Text))
            {
                MessageBox.Show("Электронный адрес должен соответствовать формату", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (Validity.checkFilled(txbPhone.Text) && !Validity.checkPhone(txbPhone.Text))
            {
                MessageBox.Show("Телефонный номер должен соответствовать указанному формату: содержать ровно 10 цифр " +
                    "(без кода страны и начального '+')", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void psbPass1_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox ctl = (PasswordBox)sender;
            Validity.setValidityState(ctl, Validity.checkPass(ctl.Password));
            if (Validity.checkFilled(ctl.Password)) txblPass.Visibility = Visibility.Hidden;
            else txblPass.Visibility = Visibility.Visible;
        }
        private void psbPass2_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox ctl = (PasswordBox)sender;
            Validity.setValidityState(ctl, ctl.Password == psbPass1.Password);
        }
        private void txbEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox ctl = (TextBox)sender;
            if (Validity.checkFilled(ctl.Text)) Validity.setValidityState(ctl, Validity.checkEmail(ctl.Text));
            else Validity.setValidityState(ctl, true);

        }
        private void txbLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox ctl = (TextBox)sender;
            Validity.setValidityState(ctl, Validity.checkFilled(ctl.Text));
        }
        private void txbName_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox ctl = (TextBox)sender;
            Validity.setValidityState(ctl, Validity.checkFilled(ctl.Text));
        }

        private void txbExp_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox ctl = (TextBox)sender;
            if (Validity.checkFilled(ctl.Text)) Validity.setValidityState(ctl, Validity.checkWhole(ctl.Text));
            else Validity.setValidityState(ctl, true);
        }

        private void txbPhone_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox ctl = (TextBox)sender;
            if (Validity.checkFilled(ctl.Text)) Validity.setValidityState(ctl, Validity.checkPhone(ctl.Text));
            else Validity.setValidityState(ctl, true);
        }

        private void psbPass1_LostFocus(object sender, RoutedEventArgs e)
        {
            Validity.setValidityState((PasswordBox)sender);
        }
        private void psbPass2_LostFocus(object sender, RoutedEventArgs e)
        {
            Validity.setValidityState((PasswordBox)sender);
        }
        private void txbEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            Validity.setValidityState((TextBox)sender);
        }
        private void txbExp_LostFocus(object sender, RoutedEventArgs e)
        {
            Validity.setValidityState((TextBox)sender);
        }
        private void txbLogin_LostFocus(object sender, RoutedEventArgs e)
        {
            Validity.setValidityState((TextBox)sender);
        }
        private void txbName_LostFocus(object sender, RoutedEventArgs e)
        {
            Validity.setValidityState((TextBox)sender);
        }
        private void txbPhone_LostFocus(object sender, RoutedEventArgs e)
        {
            Validity.setValidityState((TextBox)sender);
        }

        private void backBtn_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.frameMain.GoBack();
        }
        private void registerBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!checkFormValidity()) return;

            try
            {
                if (AppConnect.modelOdb.Authors.Count(x => x.Login == txbLogin.Text) > 0)
                {
                    MessageBox.Show("Пользователь с таким логином есть!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int? exp = null;
                if (Validity.checkFilled(txbExp.Text)) exp = Convert.ToInt32(txbExp.Text);

                Author user = new Author()
                {
                    AuthorName = txbName.Text,
                    Login = txbLogin.Text,
                    Password = psbPass1.Password,
                    Birthdate = dtpBirth.SelectedDate,
                    Experience = exp,
                    Email = Validity.checkFilled(txbEmail.Text) ? txbEmail.Text : null,
                    Phone = Validity.checkFilled(txbPhone.Text) ? txbPhone.Text : null
                };

                AppConnect.modelOdb.Authors.Add(user);
                AppConnect.modelOdb.SaveChanges();
                MessageBox.Show("Данные успешно добавлены!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) {
                MessageBox.Show($"Ошибка при добавлении данных! {ex.Message}", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
