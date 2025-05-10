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
        private const double Breakpoint1Width = 1080;
        private const double Breakpoint2Width = 720;
        private const double MinimalWidth = 220;
        private const double NormalWidth = 300;
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
            // Можно добавить логику проверки, например, что параметр не пустой
            e.CanExecute = e.Parameter is string uri && !string.IsNullOrEmpty(uri);
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
            if (width < Breakpoint2Width)
            {
                SetLeftSide(false);
                SetRightSide(false);
                Main.Margin = new Thickness(NormalMargin, 0, NormalMargin, 0);
            }
            else if (width < Breakpoint1Width)
            {
                SetLeftSide(true);
                SetRightSide(false);
                Main.Margin = new Thickness(0, 0, NormalMargin, 0);
            }
            else
            {
                SetLeftSide(true);
                SetRightSide(true);
                Main.Margin = new Thickness(0);
            }
        }
    }
}
