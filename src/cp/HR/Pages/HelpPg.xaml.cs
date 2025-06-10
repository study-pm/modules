using HR.Data.Models;
using HR.Services;
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
    /// Interaction logic for HelpPg.xaml
    /// </summary>
    public partial class HelpPg : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    {
        public bool? IsListEmpty => Staff?.Count == 0;
        public bool? IsListNotEmpty => Staff?.Count > 0;
        private ObservableCollection<Employee> _staff;
        public ObservableCollection<Employee> Staff
        {
            get => _staff;
            set
            {
                if (_staff == value) return;
                _staff = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsListEmpty));
                OnPropertyChanged(nameof(IsListNotEmpty));
            }
        }
        public HelpPg()
        {
            InitializeComponent();
            DataContext = this;
            Staff = new ObservableCollection<Employee>();

            Loaded += Page_Loaded;
        }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Staff = new ObservableCollection<Employee>(await Request.GetEmployees());
            // Alternative way: without creating a new instance
            /*
            Classes.Clear();
            foreach (var item in fromService)
                Classes.Add(item);
            */
        }
    }
}
