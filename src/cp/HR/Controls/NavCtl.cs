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
    public class NavigationData
    {
        public string Name { get; set; }
        public string Uri { get; set; }
        public object Parameter { get; set; }
    }
    public abstract class NavCtl : UserControl
    {
        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register(nameof(CurrentPage), typeof(string), typeof(NavCtl), new PropertyMetadata(null));

        public string CurrentPage
        {
            get => (string)GetValue(CurrentPageProperty);
            set => SetValue(CurrentPageProperty, value);
        }
        public static readonly DependencyProperty PageParamProperty =
            DependencyProperty.Register(nameof(PageParam), typeof(string), typeof(NavCtl), new PropertyMetadata(null));

        public string PageParam
        {
            get => (string)GetValue(PageParamProperty);
            set => SetValue(PageParamProperty, value);
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
            // Имя типа страницы
            CurrentPage = e.Content.GetType().Name;
            PageParam = e.ExtraData as string;
        }
    }
}
