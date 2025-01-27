using CookingBook.AppData;
using System;
using System.Collections.Generic;
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

namespace CookingBook.Pages
{
    /// <summary>
    /// Логика взаимодействия для Registration.xaml
    /// </summary>
    public partial class Registration : Page
    {
        public Registration()
        {
            InitializeComponent();
        }

        private Boolean ValidateEmpty(string input)
        {
            return true;
        }

        private void backBtn_Click(object sender, RoutedEventArgs e)
        {
            AppData.AppFrame.frmMain.GoBack();
        }        
        private void pass2_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (pass1Box.Password != pass2Box.Password)
            {
                submitBtn.IsEnabled = false;
                pass2Box.Background = Brushes.LightCoral;
                pass2Box.BorderBrush = Brushes.Red;
            }
            else
            {
                submitBtn.IsEnabled = true;
                pass2Box.Background = Brushes.LightGreen;
                pass2Box.BorderBrush = Brushes.Green;
            }
        }

        private void submitBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AppConnect.modelOdb.Authors.Count(x => x.Login == loginInput.Text) > 0)
                {
                    MessageBox.Show("Пользователь с таким логином есть!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (String.IsNullOrEmpty(loginInput.Text) || String.IsNullOrWhiteSpace(loginInput.Text) || String.IsNullOrEmpty(nameInput.Text) || String.IsNullOrWhiteSpace(nameInput.Text) || String.IsNullOrEmpty(pass1Box.Password) || String.IsNullOrWhiteSpace(pass1Box.Password))
                {
                    MessageBox.Show("Все поля должны быть заполнены", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }                
                Author userObj = new Author()
                {
                    Login = loginInput.Text,
                    AuthorName = nameInput.Text,
                    Password = pass1Box.Password,
                    Birthdate = birthdatePick.SelectedDate,
                };
                AppConnect.modelOdb.Authors.Add(userObj);
                AppConnect.modelOdb.SaveChanges();
                MessageBox.Show("Данные успешно добавлены!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show("Ошибка при добавлении данных!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
