using HR.Data.Models;
using HR.Models;
using HR.Services;
using HR.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.Entity;
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
    public class GenderToBoolConverter : IValueConverter
    {
        // parameter: "Male" или "Female"
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool gender && parameter is string genderParam)
            {
                if (genderParam == "Male")
                    return gender == false;
                else if (genderParam == "Female")
                    return gender == true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked && parameter is string genderParam)
            {
                if (genderParam == "Male")
                    return false;
                else if (genderParam == "Female")
                    return true;
            }
            return Binding.DoNothing;
        }
    }
    public class StaffViewModel : INotifyPropertyChanged
    {
        private readonly Staff _staff;
        public Staff GetModel() => _staff;
        public StaffViewModel(Staff staff)
        {
            _staff = staff;
        }
        public int Id => _staff.Id;
        public int DepartmentId
        {
            get => _staff.DepartmentId;
            set
            {
                if (_staff.DepartmentId != value)
                {
                    _staff.DepartmentId = value;
                    OnPropertyChanged();
                }
            }
        }

        public Department Department
        {
            get => _staff.Department;
            set
            {
                if (_staff.Department != value)
                {
                    _staff.Department = value;
                    OnPropertyChanged();
                }
            }
        }

        public int PositionId
        {
            get => _staff.PositionId;
            set
            {
                if (_staff.PositionId != value)
                {
                    _staff.PositionId = value;
                    OnPropertyChanged();
                }
            }
        }

        public Position Position
        {
            get => _staff.Position;
            set
            {
                if (_staff.Position != value)
                {
                    _staff.Position = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public class EmployeeViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        private List<StaffSnapshot> originalStaffs;

        private class StaffSnapshot
        {
            public int Id { get; set; }
            public int DepartmentId { get; set; }
            public int PositionId { get; set; }
        }
        private void SaveOriginalStaffs(IEnumerable<Staff> staffModels)
        {
            originalStaffs = staffModels.Select(s => new StaffSnapshot
            {
                Id = s.Id,
                DepartmentId = s.DepartmentId,
                PositionId = s.PositionId
            }).OrderBy(s => s.Id).ToList();
        }

        private Employee dm;
        public bool IsChanged => GivenName != dm.GivenName || Patronymic != dm.Patronymic || Surname != dm.Surname || Image != dm.Image ||
                    Gender != dm.Gender ||
                    CareerStart != dm.CareerStart || !AreStaffCollectionsEqual(Staffs);
        public bool AreStaffCollectionsEqual(IEnumerable<StaffViewModel> col1)
        {
            var current = col1.OrderBy(s => s.Id).ToList();
            if (originalStaffs == null || current.Count != originalStaffs.Count)
                return false;

            for (int i = 0; i < current.Count; i++)
            {
                var s1 = current[i];
                var s2 = originalStaffs[i];

                if (s1.Id != s2.Id) return false;
                if (s1.DepartmentId != s2.DepartmentId) return false;
                if (s1.PositionId != s2.PositionId) return false;
            }
            return true;
        }
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

        private bool _gender;
        public bool Gender
        {
            get => _gender;
            set
            {
                if (_gender == value) return;
                _gender = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsChanged));
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
        private DateTime? careerStart;
        public DateTime? CareerStart
        {
            get => careerStart;
            set
            {
                if (careerStart == value) return;
                careerStart = value;
                SelectedYear = careerStart?.Year;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedYear));
                OnPropertyChanged(nameof(IsChanged));
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        public bool HasImage => Image != null;
        private string _image;
        public string Image
        {
            get => _image;
            set
            {
                if (_image == value) return;
                _image = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsChanged));
                OnPropertyChanged(nameof(IsChanged));
                OnPropertyChanged(nameof(HasImage));
            }
        }
        public ObservableCollection<int> Years { get; }

        private int? selectedYear = null;
        public int? SelectedYear
        {
            get => selectedYear;
            set
            {
                if (selectedYear == value) return;
                selectedYear = value;
                if (selectedYear != null)
                    CareerStart = new DateTime((int)selectedYear, 1, 1);
                OnPropertyChanged();
                OnPropertyChanged(nameof(CareerStart));
                OnPropertyChanged(nameof(IsChanged));
                OnPropertyChanged(nameof(IsEnabled));
            }
        }
        private ObservableCollection<StaffViewModel> staffs;
        public ObservableCollection<StaffViewModel> Staffs
        {
            get => staffs;
            set
            {
                if (staffs == value) return;
                staffs = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsChanged));
                OnPropertyChanged(nameof(IsEnabled));
            }
        }
        private ObservableCollection<Department> departments;
        public ObservableCollection<Department> Departments
        {
            get => departments;
            set
            {
                if (departments == value) return;
                departments = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<Position> positions;
        public ObservableCollection<Position> Positions
        {
            get => positions;
            set
            {
                if (positions == value) return;
                positions = value;
                OnPropertyChanged();
            }
        }
        public EmployeeViewModel(Employee dataModel = null)
        {
            int maxAge = 120;
            int currentYear = DateTime.Now.Year;
            int maxYear = currentYear;
            int minYear = currentYear - maxAge;

            Years = new ObservableCollection<int>();
            for (int year = minYear; year <= maxYear; year++)
            {
                Years.Add(year);
            }

            dm = dataModel ?? new Employee();

            LoadStaffsFromModel(dm.Staffs);
            SaveOriginalStaffs(dm.Staffs); // <-- только здесь!
            SubscribeToStaffsCollectionChanged();
        }
        private void LoadStaffsFromModel(IEnumerable<Staff> staffModels)
        {
            if (staffModels == null)
            {
                Staffs = new ObservableCollection<StaffViewModel>();
                return;
            }

            var wrappers = staffModels.Select(s => new StaffViewModel(s)).ToList();

            // Подписываемся на PropertyChanged каждого элемента
            foreach (var wrapper in wrappers)
            {
                wrapper.PropertyChanged += StaffViewModel_PropertyChanged;
            }

            Staffs = new ObservableCollection<StaffViewModel>(wrappers);
        }
        private void StaffViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // При любом изменении внутри StaffViewModel обновляем IsChanged
            if (e.PropertyName != nameof(IsChanged))
            {
                OnPropertyChanged(nameof(IsChanged));
                OnPropertyChanged(nameof(IsEnabled));
            }
        }
        private void SubscribeToStaffsCollectionChanged()
        {
            if (Staffs != null)
            {
                Staffs.CollectionChanged += Staffs_CollectionChanged;
            }
        }

        private void Staffs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (StaffViewModel newItem in e.NewItems)
                {
                    newItem.PropertyChanged += StaffViewModel_PropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (StaffViewModel oldItem in e.OldItems)
                {
                    oldItem.PropertyChanged -= StaffViewModel_PropertyChanged;
                }
            }
            OnPropertyChanged(nameof(IsChanged));
            OnPropertyChanged(nameof(IsEnabled));
        }
        public void Reset()
        {
            GivenName = dm.GivenName;
            Patronymic = dm.Patronymic;
            Surname = dm.Surname;
            CareerStart = dm.CareerStart;
            Gender = dm.Gender;
            Image = dm.Image;

            // Восстанавливаем Staffs из originalStaffs, а не из dm.Staffs!
            var restoredStaffs = originalStaffs.Select(s => new Staff
            {
                Id = s.Id,
                DepartmentId = s.DepartmentId,
                PositionId = s.PositionId,
                // ... другие нужные свойства, если есть
            }).ToList();

            LoadStaffsFromModel(restoredStaffs);
            SubscribeToStaffsCollectionChanged();

            OnPropertyChanged(nameof(IsChanged));
            OnPropertyChanged(nameof(IsEnabled));
        }
        public void Set()
        {
            dm.GivenName = GivenName;
            dm.Patronymic = Patronymic;
            dm.Surname = Surname;
            dm.CareerStart = CareerStart;
            dm.Gender = Gender;
            dm.Image = Image;

            dm.Staffs = new ObservableCollection<Staff>(Staffs.Select(vm => vm.GetModel()));

            SaveOriginalStaffs(dm.Staffs); // <-- обновляем оригинал после сохранения

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

        public RelayCommand AddImgCmd { get; }
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

        List<Control> controls = new List<Control>();

        private bool HasValidationErrors => Validation.GetHasError(GivenNameTxb) ||
                                            Validation.GetHasError(PatronymicTxb) ||
                                            Validation.GetHasError(SurnameTxb);

        public EmployeePg(Employee employee = null)
        {
            InitializeComponent();
            controls = new List<Control> { SurnameTxb, GivenNameTxb, PatronymicTxb };
            vm = new EmployeeViewModel(employee);
            DataContext = vm;
            Loaded += Page_Loaded;

            AddImgCmd = new RelayCommand(
                _ =>
                {
                    ChangeImage();
                },
                _ => !vm.IsProgress
            );
            ResetCommand = new RelayCommand(
                _ =>
                {
                    vm.Reset();
                    ResetValidation();
                },
                _ => vm.IsEnabled
            );

            SubmitCommand = new RelayCommand(
                _ =>
                {
                    if (!Validate()) return;

                    Task task = SaveData(employee);
                },
                _ => vm.IsEnabled && !HasValidationErrors
            );

            // Subscribe to validation errors
            Validation.AddErrorHandler(SurnameTxb, ValidationErrorHandler);
        }

        private void ChangeImage()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Выберите изображение";
            ofd.Filter = "Изображения (*.jpeg;*.png;*.gif;*jpg)|*.jpeg;*.png;*.gif;*jpg";
            if (ofd.ShowDialog() != true)
                return;
            string imgPath = ofd.FileName;
            string imgFullName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(imgPath).ToLower();
            ImgHelper.SetPreviewImage(imgPath, EmployeeImg);
            MessageBox.Show($"Images {imgPath} {imgFullName}");
            vm.Image = ImgHelper.SaveImg(imgPath);
        }
        private void ResetValidation()
        {
            // Reset Errors
            controls.ForEach(control => ValidationHelper.SetShowErrors(control, true));
            // Reset Touched state
            controls.ForEach(control => ValidationHelper.SetTouched(control, false));
            // Reset Messages
            controls.ForEach(control => ValidationHelper.ResetInvalid(control, TextBox.TextProperty));
        }
        private async Task SaveData(Employee item = null)
        {
            int op = item == null ? 0 : 2;
            var (cat, name, scope) = (EventCategory.Data, op == 0 ? "Create" : "Update", "Сотрудник");
            try
            {
                vm.IsProgress = true;
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat,
                    Name = name,
                    Op = op,
                    Scope = scope,
                    Type = EventType.Progress,
                    Message = (op == 0 ? "Добавление" : "Сохранение") + " данных"
                });

                Employee data = item == null ? new Employee() : await Request.ctx.Employees.FindAsync(item.Id);
                data.Surname = vm.Surname;
                data.GivenName = vm.GivenName;
                data.Patronymic = vm.Patronymic;
                data.CareerStart = vm.CareerStart;
                data.Gender = vm.Gender;
                data.Image = vm.Image;

                if (op == 0)
                    Request.ctx.Employees.Add(data);

                // --- Staffs syncing ---
                var newStaffs = vm.Staffs.Select(vmStaff => vmStaff.GetModel()).ToList();
                SyncStaffsCollection(data, newStaffs);

                var entry = Services.Request.ctx.Entry(data);
                if (entry.State == EntityState.Unchanged)
                {
                    Debug.WriteLine("No changes detected", "EmployeePg");
                    RaiseAppEvent(new AppEventArgs
                    {
                        Category = cat,
                        Name = name,
                        Op = op,
                        Scope = scope,
                        Type = EventType.Cancel,
                        Message = "Отмена сохранения данных",
                        Details = "Изменения отсутствуют"
                    });
                    return;
                }
                await Request.ctx.SaveChangesAsync();
                vm.Set();
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat,
                    Name = name,
                    Op = op,
                    Scope = scope,
                    Type = EventType.Success,
                    Message = "Данные успешно сохранены"
                });
            }
            catch (Exception exc)
            {
                // Rollback context changes
                foreach (var changedEntry in Request.ctx.ChangeTracker.Entries())
                {
                    switch (changedEntry.State)
                    {
                        case EntityState.Modified:
                        case EntityState.Added:
                        case EntityState.Deleted:
                            changedEntry.State = EntityState.Unchanged;
                            break;
                    }
                }
                Debug.WriteLine(exc, "EmployeePg");
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat,
                    Name = name,
                    Op = op,
                    Scope = scope,
                    Type = EventType.Error,
                    Message = $"Ошибка {(op == 0 ? "добавления" : "сохранения")} данных",
                    Details = exc.Message
                });
                MessageBox.Show($"Ошибка {(op == 0 ? "добавления" : "сохранения")} данных: {exc.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                vm.IsProgress = false;
            }
        }
        public void SaveImg(string imgFullName)
        {
            string basePath = "Images";
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectRoot = System.IO.Path.Combine(appDir, "..", "..");
            string fullPath = System.IO.Path.GetFullPath(projectRoot);
            string destFolder = System.IO.Path.Combine(fullPath, basePath);
            string targetPath = System.IO.Path.Combine(destFolder, imgFullName);
        }
        private async Task SetData()
        {
            var (cat, name, op, scope) = (EventCategory.Data, "Read", 2, "Сотрудник");
            try
            {
                vm.IsProgress = true;
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat,
                    Name = name,
                    Op = op,
                    Scope = scope,
                    Type = EventType.Progress,
                    Message = "Загрузка данных"
                });
                // Run tasks in parallel
                var departmentsTask = Request.LoadDepartments();
                var positionsTask = Request.LoadPositions();

                await Task.WhenAll(departmentsTask);

                // Update UI via Dispatcher
                await Dispatcher.InvokeAsync(() =>
                {
                    vm.Departments = new ObservableCollection<Department>(departmentsTask.Result);
                    vm.Positions = new ObservableCollection<Position>(positionsTask.Result);
                });
                vm.Reset();
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat,
                    Name = name,
                    Op = op,
                    Scope = scope,
                    Type = EventType.Success,
                    Message = "Данные успешно загружены"
                });
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc, "ClassesPg");
                RaiseAppEvent(new AppEventArgs
                {
                    Category = cat,
                    Name = name,
                    Op = op,
                    Scope = scope,
                    Type = EventType.Error,
                    Message = $"Ошибка извлечения данных",
                    Details = exc.Message
                });
                MessageBox.Show($"Ошибка извлечения данных: {exc.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                vm.IsProgress = false;
            }
        }
        private void SyncStaffsCollection(Employee employee, IEnumerable<Staff> newStaffs)
        {
            // Delete missing
            var toRemove = employee.Staffs.Where(s => !newStaffs.Any(ns => ns.Id == s.Id)).ToList();
            foreach (var staff in toRemove)
                employee.Staffs.Remove(staff);

            // Add new
            var toAdd = newStaffs.Where(ns => !employee.Staffs.Any(s => s.Id == ns.Id)).ToList();
            foreach (var staff in toAdd)
                employee.Staffs.Add(staff);

            // Update existing
            foreach (var staff in employee.Staffs)
            {
                var updated = newStaffs.FirstOrDefault(ns => ns.Id == staff.Id);
                if (updated != null)
                {
                    staff.DepartmentId = updated.DepartmentId;
                    staff.PositionId = updated.PositionId;
                }
            }
        }
        private bool Validate()
        {
            // Force errors display
            controls.ForEach(control => ValidationHelper.SetShowErrors(control, true));
            controls.ForEach(control => ValidationHelper.ForceValidate(control, TextBox.TextProperty));
            ValidationHelper.ForceErrorsDisplay(controls);
            // Get validity state
            Dictionary<Control, bool> validity = new Dictionary<Control, bool>();
            controls.ForEach(ctl => validity.Add(ctl, !Validation.GetHasError(ctl)));
            // Force update UI if error
            foreach (var state in validity)
                if (!state.Value) ValidationHelper.ForceUpdateUI(state.Key);
            // Display validation warning popup
            if (validity.Values.Contains(false))
            {
                ValidationMessage.Text = "Проверьте правильность введенных значений";
                ValidationPopup.IsOpen = true;
                return false;
            }
            return true;
        }
        private void ValidationErrorHandler(object sender, ValidationErrorEventArgs e)
        {
            ResetCommand.RaiseCanExecuteChanged();
            SubmitCommand.RaiseCanExecuteChanged();
        }

        private void EnlargeImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn)) return;

            ImgHelper.OpenImgPopup(btn);
        }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await SetData();
        }
    }
}
