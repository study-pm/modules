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
        private NavigationService _navigationService;
        private MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
        public AsideLeft()
        {
            InitializeComponent();
            Loaded += Page_Loaded;
        }
        private void NavigationService_Navigated(object sender, NavigationEventArgs e)
        {
            var navigationParameter = e.ExtraData;
            CurrentPage = e.Content.GetType().Name;
            // Sync AsideLeft item to TopMenu item for HelpPg
            if (CurrentPage == "HelpPg")
            {
                var item = ((MainWindow)((App)Application.Current).MainWindow)?.MenuVM.Filters.First(x => x.PageUri == "Pages/HelpPg.xaml");
                item.IsChecked = true;
            }

            if (navigationParameter != null) return;
            if (mainWindow?.MenuVM is MenuViewModel && mainWindow.MenuVM.Filters is IEnumerable<MenuFilter>)
            {
                foreach (var filter in mainWindow.MenuVM.Filters)
                    if (filter.Page != CurrentPage) filter.IsChecked = false;
            }
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Subscribe to navigation events for the main frame
            _navigationService = MainWindow.frame.NavigationService;
            if (_navigationService != null)
            {
               _navigationService.Navigated += NavigationService_Navigated;
            }
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
                NavigateCommand.Execute(new NavigationData() { Uri = "Pages/HelpPg.xaml", Parameter = mainItem.Values });
        }
    }
}
