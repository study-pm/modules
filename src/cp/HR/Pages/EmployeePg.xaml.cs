using HR.Data.Models;
using HR.Models;
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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HR.Pages
{
    public class EmployeeViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        private Employee dm;
        public bool IsChanged => GivenName  != dm.GivenName ||
                                 Patronymic != dm.Patronymic ||
                                 Surname    != dm.Surname ||
                    (SelectedYear.HasValue
                        ? (!dm.CareerStart.HasValue || SelectedYear.Value != dm.CareerStart.Value.Year)
                        : dm.CareerStart.HasValue);
        public bool IsEnabled => IsChanged && !IsProgress;
        private bool _isProgress;
        public bool IsProgress
        {
            get => _isProgress;
            set
            {
                if (_isProgress == value) return;
                _isProgress = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        private string _givenName;
        public string GivenName
        {
            get => _givenName;
            set
            {
                if (_givenName == value) return;
                _givenName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsChanged));
                OnPropertyChanged(nameof(IsEnabled));
            }
        }
        private string _patronymic;
        public string Patronymic
        {
            get => _patronymic;
            set
            {
                if (_patronymic == value) return;
                _patronymic = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsChanged));
                OnPropertyChanged(nameof(IsEnabled));
            }
        }
        private string _surname;
        public string Surname
        {
            get => _surname;
            set
            {
                if (_surname == value) return;
                _surname = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsChanged));
                OnPropertyChanged(nameof(IsEnabled));
            }
        }
        public ObservableCollection<int> Years { get; }

        private int? selectedYear;
        public int? SelectedYear
        {
            get => selectedYear;
            set
            {
                if (selectedYear == value) return;
                {
                    selectedYear = value;
                    OnPropertyChanged(nameof(SelectedYear));
                }
            }
        }
        public EmployeeViewModel(Employee dataModel = null)
        {
            int minAge = 18;
            int maxAge = 120;
            int currentYear = DateTime.Now.Year;
            int maxYear = currentYear - minAge;
            int minYear = currentYear - maxAge;

            Years = new ObservableCollection<int>();
            for (int year = minYear; year <= maxYear; year++)
            {
                Years.Add(year);
            }

            dm = dataModel ?? new Employee();

            Reset();
        }
        public void Reset()
        {
            GivenName = dm.GivenName;
            Patronymic = dm.Patronymic;
            Surname = dm.Surname;

            if (dm.CareerStart.HasValue)
            {
                SelectedYear = dm.CareerStart.Value.Year;
            }
            OnPropertyChanged(nameof(IsChanged));
            OnPropertyChanged(nameof(IsEnabled));
        }
        public void Set()
        {
            dm.GivenName = Surname;
            dm.Patronymic = Patronymic;
            dm.Surname = Surname;
            OnPropertyChanged(nameof(IsChanged));
            OnPropertyChanged(nameof(IsEnabled));
        }
    }
    /// <summary>
    /// Interaction logic for EmployeePg.xaml
    /// </summary>
    public partial class EmployeePg : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        public RelayCommand ResetCommand { get; }
        public RelayCommand SubmitCommand { get; }

        private EmployeeViewModel _vm;
        public EmployeeViewModel vm
        {
            get => _vm;
            set
            {
                _vm = value;
                OnPropertyChanged();
            }
        }

        private bool HasValidationErrors => Validation.GetHasError(GivenNameTxb) ||
                                            Validation.GetHasError(PatronymicTxb) ||
                                            Validation.GetHasError(SurnameTxb);

        public EmployeePg(Employee employee = null)
        {
            InitializeComponent();
            vm = new EmployeeViewModel(employee);
            DataContext = vm;

            ResetCommand = new RelayCommand(
                _ => vm.Reset(),
                _ => vm.IsEnabled
            );

            SubmitCommand = new RelayCommand(
                _ =>
                {
                    SaveEmployee();
                },
                _ => vm.IsEnabled && !HasValidationErrors
            );

            // Subscribe to validation errors
            Validation.AddErrorHandler(SurnameTxb, ValidationErrorHandler);
        }
        private void ValidationErrorHandler(object sender, ValidationErrorEventArgs e)
        {
            ResetCommand.RaiseCanExecuteChanged();
            SubmitCommand.RaiseCanExecuteChanged();
        }
        private async void SaveEmployee()
        {
            try
            {
                vm.IsProgress = true;
                await Services.Request.MockAsync(2000);
                vm.Set();
                StatusInformer.ReportSuccess("Данные успешно сохранены");
            }
            catch (Exception exc)
            {
                StatusInformer.ReportFailure($"Ошибка при сохранении данных: {exc.ToString()}");
                MessageBox.Show($"Ошибка при сохранении данных: {exc.ToString()}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                vm.IsProgress = false;
                vm.Reset();
            }
        }
    }
}
