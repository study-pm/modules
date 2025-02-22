using RecipeBook.AppData;
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
using System.Xml.Linq;

namespace RecipeBook.Pages
{
    /// <summary>
    /// Interaction logic for AughPg.xaml
    /// </summary>
    public partial class AughPg : Page
    {
        public AughPg()
        {
            InitializeComponent();
        }
        private bool checkFormValidity()
        {
            if (!Validity.checkFilled(txbLogin.Text))
            {
                MessageBox.Show("Поле 'Логин' должно быть заполнено", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!Validity.checkFilled(psbPass.Password))
            {
                MessageBox.Show("Поле 'Пароль' должно быть заполнено", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }
        private void loginBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!checkFormValidity()) return;
            try
            {
                var userObj = AppConnect.modelOdb.Authors.FirstOrDefault(x => x.Login == txbLogin.Text && x.Password == psbPass.Password);
                if (userObj == null)
                {
                    MessageBox.Show("Такого пользователя нет!", "Ошибка при авторизации!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    //MessageBox.Show("Здравствуйте, " + userObj.AuthorName + "!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    AppData.User.AuthorId = userObj.AuthorID;
                    AppFrame.frameMain.Navigate(new Pages.RecipesPg(userObj));
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Ошибка " + Ex.Message.ToString() + "Критическая ошибка приложения!",
                    "Уведомление", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void registerBtn_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.frameMain.Navigate(new RegPg());
        }
    }
}
