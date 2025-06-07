using HR.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace HR.Pages
{
    /// <summary>
    /// Interaction logic for ClassesPg.xaml
    /// </summary>
    public partial class ClassesPg : Page
    {
        private NavigationService _navigationService;
        public FilterValue FilterParam { get; set; }
        public ClassesPg()
        {
            InitializeComponent();

            // Subscribe to navigation events for the main frame
            _navigationService = MainWindow.frame.NavigationService;
            if (_navigationService != null)
            {
                _navigationService.Navigated += NavigationService_Navigated;
            }
        }
        private void NavigationService_Navigated(object sender, NavigationEventArgs e)
        {
            // Check if the current page
            if (e.Content != this) return;
            var navigationParameter = e.ExtraData;
            if (navigationParameter is FilterValue filterValue)
            {
                MessageBox.Show("IF");
                // Используйте filterValue.Id, filterValue.Title и т.д.
                // Например, обновить UI или загрузить данные
                HandleFilterValue(filterValue);
            }
            else if (navigationParameter != null)
            {
                MessageBox.Show("ELSE");
                // Обработка других типов параметров, если нужно
            }
        }
        private void HandleFilterValue(FilterValue filter)
        {
            // Логика обработки параметра
            // Например, показать выбранный фильтр в интерфейсе
            MessageBox.Show($"Выбран класс: {filter.Title}");
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (FilterParam != null)
            {
                MessageBox.Show($"Выбран класс: {FilterParam.Title}");
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_navigationService != null)
            {
                _navigationService.Navigated -= NavigationService_Navigated;
                _navigationService = null;
            }
        }
    }
}
