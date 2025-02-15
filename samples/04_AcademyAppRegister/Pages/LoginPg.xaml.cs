using AcademyApp.AppData;
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

namespace AcademyApp.Pages
{
    /// <summary>
    /// Interaction logic for LoginPg.xaml
    /// </summary>
    public partial class LoginPg : Page
    {
        public LoginPg()
        {
            InitializeComponent();
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;
            btn.Background = new SolidColorBrush(Colors.LightGreen);
            btn.BorderBrush = new SolidColorBrush(Colors.DarkGreen);
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;
            btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffd000"));
            btn.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#cfa95b"));

        }

        private void btnLogIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var userObj = AppConnect.modelOdb.Users.FirstOrDefault(x => x.Login == txbLogin.Text && x.Passsword == psbPassword.Password);
                if (userObj == null)
                {
                    MessageBox.Show("Такого пользователя нет!", "Ошибка при авторизации!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    switch (userObj.IdRole)
                    {
                        case 1:
                            MessageBox.Show("Здравствуйте, Администратор " + userObj.Name + "!",
                            "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                        case 2:
                            MessageBox.Show("Здравствуйте, Ученик " + userObj.Name + "!",
                            "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                        default:
                            MessageBox.Show("Данные не обнаружены!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Warning);
                            break;
                    }
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Ошибка " + Ex.Message.ToString() + "Критическая ошибка приложения!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnRegIn_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.frameMain.Navigate(new CreateAcc());
        }
    }
}
