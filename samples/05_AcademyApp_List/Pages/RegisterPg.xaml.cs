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
    /// Interaction logic for RegisterPg.xaml
    /// </summary>
    public partial class RegisterPg : Page
    {
        public RegisterPg()
        {
            InitializeComponent();
        }
        void styleOnMouseOver(Button ctrl)
        {
            ctrl.Cursor = Cursors.Hand;
            // ctrl.Background = new SolidColorBrush(Colors.Red);
        }
        private void goBackBtn_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.frameMain.GoBack();
        }

        private void goBackBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            Button btn = sender as Button;
            styleOnMouseOver(btn);
        }

        private void createAccBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            styleOnMouseOver((Button)sender);
        }

        private void psbPass_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox pb = sender as PasswordBox;
            if (pb.Password != txbPass.Text)
            {
                btnCreate.IsEnabled = false;
                pb.Background = Brushes.LightCoral;
                pb.BorderBrush = Brushes.Red;
            }
            else
            {
                btnCreate.IsEnabled = true;
                pb.Background = Brushes.LightGreen;
                pb.BorderBrush = Brushes.Green;
            }
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.modelOdb.Users.Count(x => x.Login == txbLogin.Text) > 0)
            {
                MessageBox.Show("Пользователь с таким логином есть!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {
                User userObj = new User()
                {
                    Login = txbLogin.Text,
                    Name = txbName.Text,
                    Passsword = txbPass.Text,
                    IdRole = 2
                };
                AppConnect.modelOdb.Users.Add(userObj);
                AppConnect.modelOdb.SaveChanges();
                MessageBox.Show("Данные успешно добавлены!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show("Ошибка при добавлении данных!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

}
