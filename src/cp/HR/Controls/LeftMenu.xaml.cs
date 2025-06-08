using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for LeftMenu.xaml
    /// </summary>
    public partial class LeftMenu : NavCtl
    {
        private bool isOpen = false;
        public bool IsOpen
        {
            get => isOpen;
            set
            {
                if (isOpen == value) return;
                isOpen = value;
                OnPropertyChanged();
            }
        }
        public LeftMenu()
        {
            InitializeComponent();
            DataContext = this;
            SideMenuPopup.Closed += SideMenuPopup_Closed;
        }
        private void OpenMenu(object sender, RoutedEventArgs e)
        {
            SideMenuPopup.IsOpen = true;
            IsOpen = true;
        }
        private void CloseMenu(object sender, RoutedEventArgs e)
        {
            SideMenuPopup.IsOpen = false;
            IsOpen = false;
        }
        private void SideMenuPopup_Closed(object sender, EventArgs e)
        {
            IsOpen = false;
        }
        private void ToggleMenu(object sender, RoutedEventArgs e) => IsOpen = !IsOpen;

    }
}
