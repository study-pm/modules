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
    /// Interaction logic for AsideLeft.xaml
    /// </summary>
    public partial class AsideLeft : UserControl
    {
        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register(nameof(CurrentPage), typeof(string), typeof(AsideLeft), new PropertyMetadata(null));
        public string CurrentPage
        {
            get => (string)GetValue(CurrentPageProperty);
            set => SetValue(CurrentPageProperty, value);
        }
        public AsideLeft()
        {
            InitializeComponent();
            Loaded += Page_Loaded;
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (MainWindow.frame == null) return;
            MainWindow.frame.Navigated += Nav_Navigated;
        }
        private void Nav_Navigated(object sender, NavigationEventArgs e)
        {
            CurrentPage = e.Content.GetType().Name;
        }

        private void StaffItem_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.frame.Navigate(new StaffPg());
        }
    }
}
