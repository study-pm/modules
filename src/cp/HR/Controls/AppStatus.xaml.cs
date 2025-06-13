using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Threading;
using HR.Services;
using HR.Utilities;
using static HR.Services.AppEventHelper;

namespace HR.Controls
{
    public class StatusToVisibilityConverter : IValueConverter
    {
        public EventType TargetStatus { get; set; } = EventType.Success;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EventType status)
            {
                EventType target = TargetStatus;
                if (parameter != null && Enum.TryParse(parameter.ToString(), out EventType paramStatus))
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

        private string _details;
        public string Details
        {
            get => _details;
            set
            {
                _details = value;
                OnPropertyChanged(nameof(Details));
            }
        }
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
        private DateTime _timestamp;
        public DateTime Timestamp
        {
            get => _timestamp;
            set
            {
                _timestamp = value;
                OnPropertyChanged(nameof(Timestamp));
            }
        }
        public EventType _status;
        public EventType Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(Timestamp));
                if (value == EventType.Progress) StartAnimation();
                else StopAnimation();
            }
        }
        public AppStatus()
        {
            InitializeComponent();
            this.DataContext = this;
            AppEventHelper.AppEvent += (sender, args) =>
            {
                Details = args.Details;
                Message = args.Message;
                Status = args.Type;
                Timestamp = args.Timestamp;
            };
        }
        private DispatcherTimer _animTimer;
        private int _animStep = 0;

        private void StartAnimation()
        {
            Prosess1.Visibility = Visibility.Visible;
            _animTimer = new DispatcherTimer();
            _animTimer.Interval = TimeSpan.FromMilliseconds(500); // скорость анимации
            _animTimer.Tick += AnimTimer_Tick;
            _animTimer.Start();
        }

        private void AnimTimer_Tick(object sender, EventArgs e)
        {
            _animStep = (_animStep + 1) % 3;
            Prosess2.Visibility = _animStep == 0 ? Visibility.Visible : Visibility.Collapsed;
            Prosess3.Visibility = _animStep == 1 ? Visibility.Visible : Visibility.Collapsed;
            Prosess1.Visibility = _animStep == 2 ? Visibility.Visible : Visibility.Collapsed;
        }

        // Если нужно остановить анимацию:
        private void StopAnimation()
        {
            _animTimer?.Stop();
            Prosess1.Visibility = Visibility.Collapsed;
            Prosess2.Visibility = Visibility.Collapsed;
            Prosess3.Visibility = Visibility.Collapsed;
        }

    }
}
