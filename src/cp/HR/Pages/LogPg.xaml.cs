using HR.Data.Models;
using HR.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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
using static HR.Services.AppEventHelper;

namespace HR.Pages
{
    public class EventCategoryToPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            EventCategory status;

            if (value is EventCategory ec)
                status = ec;
            else if (value is byte b && Enum.IsDefined(typeof(EventCategory), b))
                status = (EventCategory)b;
            else
                return null;

            switch (status)
            {
                case EventCategory.Auth:
                    return Application.Current.TryFindResource("KeySolidPath") as Geometry;
                case EventCategory.Data:
                    return Application.Current.TryFindResource("DatabaseSolidPath") as Geometry;
                case EventCategory.Navigation:
                    return Application.Current.TryFindResource("LocationArrowSolidPath") as Geometry;
                case EventCategory.Service:
                    return Application.Current.TryFindResource("ToolsSolidPath") as Geometry;
                default:
                    return null;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class EventCategoryToTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AppEventHelper.EventCategory eventCategory)
            {
                return eventCategory.ToTitle();
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class EventTypeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            EventType status;

            if (value is EventType et)
                status = et;
            else if (value is byte b && Enum.IsDefined(typeof(EventType), b))
                status = (EventType)b;
            else
                return Brushes.Gray;

            string resourceKey;

            switch (status)
            {
                case EventType.Progress:
                    resourceKey = "AwaitingBrush";
                    break;
                case EventType.Success:
                    resourceKey = "SuccessBrush";
                    break;
                case EventType.Error:
                    resourceKey = "ErrorBrush";
                    break;
                case EventType.Info:
                    resourceKey = "InfoBrush";
                    break;
                case EventType.Warning:
                    resourceKey = "WarningBrush";
                    break;
                default:
                    resourceKey = "greyDarkBrush";
                    break;
            }

            if (Application.Current.Resources.Contains(resourceKey))
            {
                return Application.Current.Resources[resourceKey] as Brush ?? Brushes.Gray;
            }
            else
            {
                return Brushes.Gray;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class EventTypeToPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            EventType status;

            if (value is EventType et)
                status = et;
            else if (value is byte b && Enum.IsDefined(typeof(EventType), b))
                status = (EventType)b;
            else
                return null;

            switch (status)
            {
                case EventType.Progress:
                    return Application.Current.TryFindResource("ClockSolidPath") as Geometry;
                case EventType.Success:
                    return Application.Current.TryFindResource("CheckCircleSolidPath") as Geometry;
                case EventType.Error:
                    return Application.Current.TryFindResource("ExclamationCircleSolidPath") as Geometry;
                case EventType.Info:
                    return Application.Current.TryFindResource("InfoCircleSolidPath") as Geometry;
                case EventType.Warning:
                    return Application.Current.TryFindResource("ExclamationTriangleSolidPath") as Geometry;
                default:
                    return null;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class EventTypeToTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AppEventHelper.EventType eventType)
            {
                return eventType.ToTitle(); // Вызов вашего метода расширения
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
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

        private void DataGridColumnHeader_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (sender is DataGridColumnHeader header && header.ContextMenu != null)
            {
                foreach (MenuItem item in header.ContextMenu.Items)
                {
                    if (int.TryParse(item.Tag?.ToString(), out int columnIndex))
                    {
                        var column = dataGrid.Columns[columnIndex];
                        item.IsChecked = column.Visibility == Visibility.Visible;
                    }
                }
            }
        }
        private void DataGridColumnHeaderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && int.TryParse(menuItem.Tag?.ToString(), out int columnIndex))
            {
                var column = dataGrid.Columns[columnIndex];
                // Toggle visibility based on checkbox state
                column.Visibility = menuItem.IsChecked ? Visibility.Visible : Visibility.Collapsed;
            }
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
