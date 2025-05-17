using HR.Data.Models;
using HR.Models;
using HR.Services;
using HR.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for StaffPg.xaml
    /// </summary>
    public partial class StaffPg : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        private ObservableCollection<Employee> staff;
        public ObservableCollection<Employee> Staff {
            get => staff;
            set
            {
                if (staff == value) return;
                staff = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsStaffEmpty));
                OnPropertyChanged(nameof(IsStaffNotEmpty));
            }
        }
        public bool? IsStaffEmpty => Staff?.Count == 0;
        public bool? IsStaffNotEmpty => Staff?.Count > 0;
        private bool isGridView;
        public bool IsGridView {
            get => isGridView;
            set
            {
                isGridView = value;
                OnPropertyChanged();
            }
        }
        public StaffPg()
        {
            InitializeComponent();
            DataContext = this;
            // @TODO: Read user preferences and set here
            IsGridView = true;
        }
        private async Task SetEmployees()
        {
            Staff = new ObservableCollection<Employee>(await Request.GetEmployees());
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await SetEmployees();
        }
    }
}
