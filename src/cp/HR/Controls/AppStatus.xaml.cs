using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
using HR.Utilities;

namespace HR.Controls
{
    public class StatusToVisibilityConverter : IValueConverter
    {
        public StatusType TargetStatus { get; set; } = StatusType.Success;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is StatusType status)
            {
                StatusType target = TargetStatus;
                if (parameter != null && Enum.TryParse(parameter.ToString(), out StatusType paramStatus))
                    target = paramStatus;
                return status == target ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
    /// <summary>
    /// Interaction logic for AppStatus.xaml
    /// </summary>
    public partial class AppStatus : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public string _message;
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }
        public DateTime Timestamp { get; set; }
        public StatusType _status;
        public StatusType Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
                Timestamp = DateTime.Now;
                OnPropertyChanged(nameof(Timestamp));
            }
        }
        public AppStatus()
        {
            InitializeComponent();
            this.DataContext = this;
            StatusInformer.StatusChanged += (sender, args) =>
            {
                Message = args.Message;
                Status = args.Type;
            };
        }
    }
}
