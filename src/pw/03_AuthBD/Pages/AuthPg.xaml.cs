using AuthBD.AppData;
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

namespace AuthBD.Pages
{
    /// <summary>
    /// Interaction logic for AuthPg.xaml
    /// </summary>
    public partial class AuthPg : Page
    {
        public AuthPg()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var userObj = AppConnect.modelOdb.Authors.FirstOrDefault(x => x.Login == txbLogin.Text && x.Password == psbPass.Password);
                if (userObj == null)
                {
                    MessageBox.Show("Такого пользователя нет!", "Ошибка при авторизации!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("Здравствуйте, " + userObj.AuthorName + "!", "Уведомление",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Ошибка " + Ex.Message.ToString() + "Критическая ошибка приложения!",
                    "Уведомление", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
