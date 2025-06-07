using HR.Pages;
using HR.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace HR.Controls
{
    /// <summary>
    /// Interaction logic for TopHeader.xaml
    /// </summary>
    public partial class TopHeader : NavCtl
    {
        public static readonly DependencyProperty MenuOpenProp =
        DependencyProperty.Register("IsMenuOpen", typeof(string), typeof(Header));

        public string IsMenuOpen
        {
            get { return (string)GetValue(MenuOpenProp); }
            set { SetValue(MenuOpenProp, value); }
        }
        public TopHeader()
        {
            InitializeComponent();
        }
        private void HomeItem_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.frame.Navigate(new HomePg());
        }
        private void AuthItem_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.frame.Navigate(new AuthPg());
        }
        private void LogoutItem_Click(object sender, RoutedEventArgs e)
        {
            App.Current.LogOutCommand.Execute(null);
        }
        private void PreferencesItem_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.frame.Navigate(new PreferencesPg());
        }
        public void SetNarrowMode(bool isNarrow)
        {
            LeftMenu.Visibility = isNarrow ? Visibility.Visible : Visibility.Collapsed;
            TopMenu.Visibility = isNarrow ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
