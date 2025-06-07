using HR.Controls;
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
            if (e.Content != this) return;
            var navigationParameter = e.ExtraData;

            // Если пришёл один FilterValue — используем его
            if (navigationParameter is HR.Utilities.FilterValue filterValue)
            {
                HandleFilterValue(filterValue);
            }
            // Если пришла коллекция FilterValue — используем ее
            if (navigationParameter is IEnumerable<FilterValue> filterValues)
            {
                HandleFilterValues(filterValues);
            }
            // Если пришёл NavigationData — извлекаем Parameter
            else if (navigationParameter is NavigationData navData)
            {
                // Если Parameter — один FilterValue
                if (navData.Parameter is HR.Utilities.FilterValue filter)
                {
                    HandleFilterValue(filter);
                }
                // Если Parameter — коллекция FilterValue
                else if (navData.Parameter is IEnumerable<HR.Utilities.FilterValue> filters)
                {
                    HandleFilterValues(filters);
                }
                else
                {
                    // Обработка других типов параметров, если нужно
                    MessageBox.Show("NavigationData.Parameter имеет неожиданный тип: " + navData.Parameter?.GetType().Name);
                }
            }
            else if (navigationParameter != null)
            {
                MessageBox.Show("ELSE: " + navigationParameter.GetType().Name);
            }
        }
        private void HandleFilterValue(Utilities.FilterValue filter)
        {
            // Логика обработки параметра
            // Например, показать выбранный фильтр в интерфейсе
            MessageBox.Show($"Выбран класс: {filter.Title}");
        }

        private void HandleFilterValues(IEnumerable<FilterValue> filters)
        {
            // Логика обработки списка выбранных фильтров
            MessageBox.Show("ClassesPg Filters Change");
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
