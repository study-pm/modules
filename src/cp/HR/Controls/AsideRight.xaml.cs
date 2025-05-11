using HR.Pages;
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

namespace HR.Controls
{
    /// <summary>
    /// Interaction logic for AsideRight.xaml
    /// </summary>
    public partial class AsideRight : NavCtl
    {
        public AsideRight()
        {
            InitializeComponent();
        }
        private void LogoutItem_Click(object sender, RoutedEventArgs e)
        {
            App.Current.LogOutCommand.Execute(null);
        }
    }
}
