using HR.Data.Models;
using HR.Pages;
using HR.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
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

namespace HR.Controls
{
    /// <summary>
    /// Interaction logic for AsideLeft.xaml
    /// </summary>
    public partial class AsideLeft : NavCtl
    {
        public AsideLeft()
        {
            InitializeComponent();
        }
        private void RadioButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var radioButton = sender as RadioButton;
            if (radioButton == null)
                return;

            // If the RadioButton is already checked, uncheck it and mark event as handled
            if (radioButton.IsChecked == true)
            {
                radioButton.IsChecked = false;

                // If your ViewModel binding is TwoWay, this will update IsChecked in VM as well
                e.Handled = true; // prevent default toggle behavior
            }
            MenuFilter mainItem = radioButton.DataContext as MenuFilter;
            if (mainItem.Title == "Справка")
                MainWindow.frame.Navigate(new HelpPg());
        }
    }
}
