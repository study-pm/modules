using HR.Data.Models;
using HR.Services;
using HR.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
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
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        internal User dm;
        public bool IsChanged => dm.Is2faOn != Is2FA;
        public bool IsEnabled => IsChanged && !IsInProgress;
        private bool _isInProgress;
        public bool IsInProgress
        {
            get => _isInProgress;
            set
            {
                if (_isInProgress == value) return;
                _isInProgress = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEnabled));
            }
        }
        private bool _is2fa;
        public bool Is2FA
        {
            get => _is2fa;
            set
            {
                if (_is2fa == value) return;
                _is2fa = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsChanged));
                OnPropertyChanged(nameof(IsEnabled));
            }
        }
        public SettingsViewModel()
        {
            Is2FA = false;
        }
        public SettingsViewModel(User dataModel)
        {
            dm = dataModel;
            Is2FA = dm.Is2faOn;
        }
        public void Reset()
        {
            Is2FA = dm.Is2faOn;
        }
        public void Set()
        {
            dm.Is2faOn = Is2FA;
            OnPropertyChanged(nameof(IsChanged));
            OnPropertyChanged(nameof(IsEnabled));
        }
    }
    /// <summary>
    /// Interaction logic for SettingsPg.xaml
    /// </summary>
    public partial class SettingsPg : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        private int uid = ((App)(Application.Current)).CurrentUser.Id;
        private const double MinColumnWidth = 350;
        private SettingsViewModel _vm;
        public SettingsViewModel vm
        {
            get => _vm;
            set
            {
                _vm = value;
                OnPropertyChanged();
            }
        }
        public SettingsPg()
        {
            InitializeComponent();
            this.SizeChanged += SettingsPg_SizeChanged;
            Loaded += SettingsPg_Loaded;
        }
        private void AdjustGridLayout(double availableWidth)
        {
            MainGrid.RowDefinitions.Clear();
            MainGrid.ColumnDefinitions.Clear();

            if (availableWidth >= MinColumnWidth * 2)
            {
                // Две колонки, одна строка
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                Grid.SetRow(FirstSection, 0);
                Grid.SetColumn(FirstSection, 0);

                Grid.SetRow(SecondSection, 0);
                Grid.SetColumn(SecondSection, 1);
            }
            else
            {
                // Одна колонка, две строки
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                Grid.SetRow(FirstSection, 0);
                Grid.SetColumn(FirstSection, 0);

                Grid.SetRow(SecondSection, 1);
                Grid.SetColumn(SecondSection, 0);
            }
        }
        private void SettingsPg_Loaded(object sender, RoutedEventArgs e)
        {
            AdjustGridLayout(this.ActualWidth);
        }

        private void SettingsPg_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdjustGridLayout(e.NewSize.Width);
        }
        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            vm.Reset();
        }
        private void SubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            vm.Set();
        }
    }
}
