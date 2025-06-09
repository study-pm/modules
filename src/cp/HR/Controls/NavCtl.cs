using HR.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows;
using PdfSharp.Pdf.Filters;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Data.Entity;
using HR.Pages;

namespace HR.Controls
{
    public class NavigationData : DependencyObject
    {
        public static readonly DependencyProperty ParameterProperty =
            DependencyProperty.Register("Parameter", typeof(object), typeof(NavigationData));
        public object Parameter
        {
            get => GetValue(ParameterProperty);
            set => SetValue(ParameterProperty, value);
        }

        public static readonly DependencyProperty UriProperty =
            DependencyProperty.Register("Uri", typeof(string), typeof(NavigationData));
        public string Uri
        {
            get => (string)GetValue(UriProperty);
            set => SetValue(UriProperty, value);
        }

        public string Name { get; set; }
    }
    public abstract class NavCtl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

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

        public RelayCommand NavigateCommand { get; }

        protected NavCtl()
        {
            Loaded += Page_Loaded;
            this.DataContext = this;

            NavigateCommand = new RelayCommand(
                execute: param =>
                {
                    if (param is NavigationData navData)
                    {
                        MainWindow.frame.Navigate(new Uri(navData.Uri, UriKind.Relative), navData.Parameter);
                    }
                },
                canExecute: param => param is NavigationData
            );
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (MainWindow.frame == null) return;
            MainWindow.frame.Navigated += Nav_Navigated;
        }
        private void Nav_Navigated(object sender, NavigationEventArgs e)
        {
            // Page name
            CurrentPage = e.Content.GetType().Name;
            var navigationParameter = e.ExtraData;
            if (navigationParameter is IEnumerable<FilterValue> selectedFilters)
            {
                // Handle collection param here
            }
            if (navigationParameter is FilterValue filterValue)
            {
                // Page subpath
                PageParam = filterValue.Title;
            }
        }
    }
}
