using HR.Data.Models;
using HR.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
using static HR.Services.AppEventHelper;

namespace HR.Pages
{
    /// <summary>
    /// Interaction logic for LogPg.xaml
    /// </summary>
    public partial class LogPg : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        private bool _isProgress;
        public bool IsProgress
        {
            get => _isProgress;
            set
            {
                if (_isProgress == value) return;
                _isProgress = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<AppEventArgs> _log;
        public ObservableCollection<AppEventArgs> Log
        {
            get => _log;
            set
            {
                if (_log == value) return;
                _log = value;
                OnPropertyChanged();
            }
        }
        public LogPg()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += Page_Loaded;
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            // Numbering from 1
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Log = new ObservableCollection<AppEventArgs>(await Request.GetLog(App.Current.CurrentUser.Id));
        }
    }
}
