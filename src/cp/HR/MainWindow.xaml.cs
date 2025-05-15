using HR.Pages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace HR
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Frame frame {  get; set; }
        private const double MediumWidth = 1080;
        private const double NarrowWidth = 720;
        private const double MinimalWidth = 70;
        private const double NormalWidth = 250;
        private const double NormalMargin = 30;
        public MainWindow()
        {
            InitializeComponent();
            MainWindow.frame = mainFrame;
            // Handle window resize and elements visibility
            this.SizeChanged += MainWindow_SizeChanged;
            UpdateVisibility(this.ActualWidth);
            // Implement navigation command bindings
            CommandBinding goToPageBinding = new CommandBinding(NavigationCommands.GoToPage, GoToPage_Executed, GoToPage_CanExecute);
            this.CommandBindings.Add(goToPageBinding);
        }
        private void GoToPage_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;

            if (!(e.Parameter is string uri) || string.IsNullOrEmpty(uri))
                // Можно добавить логику проверки, например, что параметр не пустой
                return;

            try
            {
                var resourceUri = new Uri($"pack://application:,,,/{uri}", UriKind.Absolute);
                var streamInfo = Application.GetResourceStream(resourceUri);

                if (streamInfo == null) return;

                e.CanExecute = true;
            }
            catch
            {
                // Ошибка при формировании URI или получении ресурса - команда недоступна
                return;
            }
        }

        private void GoToPage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is string uri)
            {
                mainFrame.Navigate(new Uri(uri, UriKind.Relative));
            }
        }
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateVisibility(e.NewSize.Width);
        }
        private void SetRightSide(bool IsVisible)
        {
            RightAside.Visibility = IsVisible ? Visibility.Visible : Visibility.Collapsed;
            RightSplitter.Visibility = IsVisible ? Visibility.Visible : Visibility.Collapsed;
            MainLayoutContainer.ColumnDefinitions[4].Width = IsVisible ? new GridLength(NormalWidth, GridUnitType.Pixel) : new GridLength(0);
            MainLayoutContainer.ColumnDefinitions[4].MinWidth = IsVisible ? MinimalWidth : 0;
        }
        private void SetLeftSide(bool IsVisible)
        {
            LeftAside.Visibility = IsVisible ? Visibility.Visible : Visibility.Collapsed;
            LeftSplitter.Visibility = IsVisible ? Visibility.Visible : Visibility.Collapsed;
            MainLayoutContainer.ColumnDefinitions[0].Width = IsVisible ? new GridLength(NormalWidth, GridUnitType.Pixel) : new GridLength(0);
            MainLayoutContainer.ColumnDefinitions[0].MinWidth = IsVisible ? MinimalWidth : 0;
        }
        private void UpdateVisibility(double width)
        {
            if (width < NarrowWidth)
            {
                SetLeftSide(false);
                SetRightSide(false);
                TopHeader.SetNarrowMode(true);
            }
            else if (width < MediumWidth)
            {
                SetLeftSide(false);
                SetRightSide(true);
                TopHeader.SetNarrowMode(false);
            }
            else
            {
                SetLeftSide(true);
                SetRightSide(true);
                TopHeader.SetNarrowMode(false);
            }
        }
    }
}
