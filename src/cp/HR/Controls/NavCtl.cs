using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows;

namespace HR.Controls
{
    public abstract class NavCtl : UserControl
    {
        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register(nameof(CurrentPage), typeof(string), typeof(NavCtl), new PropertyMetadata(null));

        public string CurrentPage
        {
            get => (string)GetValue(CurrentPageProperty);
            set => SetValue(CurrentPageProperty, value);
        }

        protected NavCtl()
        {
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
    }
}
