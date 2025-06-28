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

    public class EducationViewModel : INotifyPropertyChanged
    {
        private readonly Education _education;
        public Education GetModel() => _education;
        public EducationViewModel(Education education)
        {
            _education = education;
        }
        public int Id => _education.Id;
        public int DegreeId
        {
            get => _education.DegreeId;
            set
            {
                if (_education.DegreeId != value)
                {
                    _education.DegreeId = value;
                    OnPropertyChanged();
                }
            }
        }

        public Degree Degree
        {
            get => _education.Degree;
            set
            {
                if (_education.Degree != value)
                {
                    _education.Degree = value;
                    OnPropertyChanged();
                }
            }
        }

        public int? OrganizationId
        {
            get => _education.OrganizationId;
            set
            {
                if (_education.OrganizationId != value)
                {
                    _education.OrganizationId = value;
                    OnPropertyChanged();
                }
            }
        }
        public Organization Organization
        {
            get => _education.Organization;
            set
            {
                if (_education.Organization != value)
                {
                    _education.Organization = value;
                    OnPropertyChanged();
                }
            }
        }

        public int? QualificationId
        {
            get => _education.QualificationId;
            set
            {
                if (_education.QualificationId != value)
                {
                    _education.QualificationId = value;
                    OnPropertyChanged();
                }
            }
        }
        public Qualification Qualification
        {
            get => _education.Qualification;
            set
            {
                if (_education.Qualification != value)
                {
                    _education.Qualification = value;
                    OnPropertyChanged();
                }
            }
        }

        public int? SpecialtyId
        {
            get => _education.SpecialtyId;
            set
            {
                if (_education.SpecialtyId != value)
                {
                    _education.SpecialtyId = value;
                    OnPropertyChanged();
                }
            }
        }
        public Specialty Specialty
        {
            get => _education.Specialty;
            set
            {
                if (_education.Specialty != value)
                {
                    _education.Specialty = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
        private List<EducationSnapshot> originalEducations;

        private class EducationSnapshot
        {
            public int Id { get; set; }
            public int DegreeId { get; set; }
            public int? OrganizationId { get; set; }
            public int? QualificationId { get; set; }
            public int? SpecialtyId { get; set; }
        }
        private void SaveOriginalEducations(IEnumerable<Education> educationModels)
        {
            originalEducations = educationModels.Select(s => new EducationSnapshot
            {
                Id = s.Id,
                DegreeId = s.DegreeId,
                OrganizationId = s.OrganizationId,
                QualificationId = s.QualificationId,
                SpecialtyId = s.SpecialtyId
            }).OrderBy(s => s.Id).ToList();
        }

        private Employee dm;
        public bool IsChanged => GivenName != dm.GivenName || Patronymic != dm.Patronymic || Surname != dm.Surname || Image != dm.Image ||
                    Gender != dm.Gender ||
                    CareerStart != dm.CareerStart || !AreStaffCollectionsEqual(Staffs) || !AreEducationCollectionsEqual(Educations);
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
        public bool AreEducationCollectionsEqual(IEnumerable<EducationViewModel> col1)
        {
            var current = col1.OrderBy(s => s.Id).ToList();
            if (originalEducations == null || current.Count != originalEducations.Count)
                return false;

            for (int i = 0; i < current.Count; i++)
            {
                var s1 = current[i];
                var s2 = originalEducations[i];

                if (s1.Id != s2.Id) return false;
                if (s1.DegreeId != s2.DegreeId) return false;
                if (s1.OrganizationId != s2.OrganizationId) return false;
                if (s1.QualificationId != s2.QualificationId) return false;
                if (s1.SpecialtyId != s2.SpecialtyId) return false;
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
        private ObservableCollection<EducationViewModel> educations;
        public ObservableCollection<EducationViewModel> Educations
        {
            get => educations;
            set
            {
                if (educations == value) return;
                educations = value;
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
        private ObservableCollection<Organization> organizations;
        public ObservableCollection<Organization> Organizations
        {
            get => organizations;
            set
            {
                if (organizations == value) return;
                organizations = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<Qualification> qualification;
        public ObservableCollection<Qualification> Qualifications
        {
            get => qualification;
            set
            {
                if (qualification == value) return;
                qualification = value;
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
        private ObservableCollection<Degree> degrees;
        public ObservableCollection<Degree> Degrees
        {
            get => degrees;
            set
            {
                if (degrees == value) return;
                degrees = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<Specialty> specialties;
        public ObservableCollection<Specialty> Specialties
        {
            get => specialties;
            set
            {
                if (specialties == value) return;
                specialties = value;
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
            SaveOriginalStaffs(dm.Staffs);
            SubscribeToStaffsCollectionChanged();

            LoadEducationsFromModel(dm.Educations);
            SaveOriginalEducations(dm.Educations);
            SubscribeToEducationsCollectionChanged();
        }
        public void LoadStaffsFromModel(IEnumerable<Staff> staffModels)
        {
            if (staffModels == null)
            {
                Staffs = new ObservableCollection<StaffViewModel>();
                return;
            }

            var wrappers = staffModels.Select(s => new StaffViewModel(s)).ToList();

            foreach (var wrapper in wrappers)
            {
                wrapper.PropertyChanged += ViewModel_PropertyChanged;
            }

            Staffs = new ObservableCollection<StaffViewModel>(wrappers);
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
                    newItem.PropertyChanged += ViewModel_PropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (StaffViewModel oldItem in e.OldItems)
                {
                    oldItem.PropertyChanged -= ViewModel_PropertyChanged;
                }
            }
            OnPropertyChanged(nameof(IsChanged));
            OnPropertyChanged(nameof(IsEnabled));
        }
        public void LoadEducationsFromModel(IEnumerable<Education> eductionModels)
        {
            if (eductionModels == null)
            {
                Educations = new ObservableCollection<EducationViewModel>();
                return;
            }

            var wrappers = eductionModels.Select(s => new EducationViewModel(s)).ToList();

            foreach (var wrapper in wrappers)
            {
                wrapper.PropertyChanged += ViewModel_PropertyChanged;
            }

            Educations = new ObservableCollection<EducationViewModel>(wrappers);
        }
        private void SubscribeToEducationsCollectionChanged()
        {
            if (Educations != null)
            {
                Educations.CollectionChanged += Educations_CollectionChanged;
            }
        }
        private void Educations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (EducationViewModel newItem in e.NewItems)
                {
                    newItem.PropertyChanged += ViewModel_PropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (EducationViewModel oldItem in e.OldItems)
                {
                    oldItem.PropertyChanged -= ViewModel_PropertyChanged;
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

            var restoredStaffs = originalStaffs.Select(s => new Staff
            {
                Id = s.Id,
                DepartmentId = s.DepartmentId,
                PositionId = s.PositionId,
            }).ToList();

            LoadStaffsFromModel(restoredStaffs);
            SubscribeToStaffsCollectionChanged();

            var restoredEducations = originalEducations.Select(s => new Education
            {
                Id = s.Id,
                DegreeId = s.DegreeId,
                OrganizationId = s.OrganizationId,
                QualificationId = s.QualificationId,
                SpecialtyId = s.SpecialtyId,
            }).ToList();

            LoadEducationsFromModel(restoredEducations);
            SubscribeToEducationsCollectionChanged();

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
            dm.Educations = new ObservableCollection<Education>(Educations.Select(vm => vm.GetModel()));

            SaveOriginalStaffs(dm.Staffs); // <-- обновляем оригинал после сохранения
            SaveOriginalEducations(dm.Educations); // <-- обновляем оригинал после сохранения

            OnPropertyChanged(nameof(IsChanged));
            OnPropertyChanged(nameof(IsEnabled));
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(IsChanged))
            {
                OnPropertyChanged(nameof(IsChanged));
                OnPropertyChanged(nameof(IsEnabled));
            }
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

        private NavigationService _navigationService;

        public RelayCommand AddImgCmd { get; }
        public RelayCommand AddEducationCmd { get; }
        public RelayCommand AddStaffCmd { get; }
        public RelayCommand DelEducationCmd { get; }
        public RelayCommand DelStaffCmd { get; }
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
            Unloaded += Page_Unloaded;

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
            AddEducationCmd = new RelayCommand(
                _ =>
                {
                    Education newEducation = new Education
                    {
                        Degree = null,
                        DegreeId = 0,
                        Organization = null,
                        OrganizationId = 0,
                        Qualification = null,
                        QualificationId = 0,
                        Specialty = null,
                        SpecialtyId = 0,
                    };
                    vm.Educations.Add(new EducationViewModel(newEducation));
                },
                _ => !vm.IsProgress
            );
            AddStaffCmd = new RelayCommand(
                _ =>
                {
                    Staff newStaff = new Staff
                    {
                        Department = null,
                        DepartmentId = 0,
                        Position = null,
                        PositionId = 0
                    };
                    vm.Staffs.Add(new StaffViewModel(newStaff));
                },
                _ => !vm.IsProgress
            );
            DelEducationCmd = new RelayCommand(
                execute: param =>
                {
                    if (!(param is EducationViewModel educationVm)) return;
                    vm.Educations.Remove(educationVm);
                },
                _ => !vm.IsProgress
            );
            DelStaffCmd = new RelayCommand(
                execute: param =>
                {
                    if (!(param is StaffViewModel staffVm)) return;
                    vm.Staffs.Remove(staffVm);
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

            // Subscribe to navigation events for the main frame
            _navigationService = MainWindow.frame.NavigationService;
            if (_navigationService != null)
            {
                _navigationService.Navigating += NavigationService_Navigating;
            }
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
            vm.Image = ImgHelper.SaveImg(imgPath, 40);
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

                MessageBox.Show($"Save image {data.Image}");

                if (op == 0)
                    Request.ctx.Employees.Add(data);

                // --- Staffs syncing ---
                var newStaffs = vm.Staffs.Select(vmStaff => vmStaff.GetModel()).ToList();
                SyncStaffsCollection(data, newStaffs);

                if (!Request.ctx.ChangeTracker.Entries().Any(e => e.State != EntityState.Unchanged))
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
                // --- Educations syncing ---
                var newEducations = vm.Educations.Select(vmEducation => vmEducation.GetModel()).ToList();
                SyncEducationsCollection(data, newEducations);

                if (!Request.ctx.ChangeTracker.Entries().Any(e => e.State != EntityState.Unchanged))
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
                await Request.ctx.Entry(data).Collection(e => e.Staffs).LoadAsync();
                vm.LoadStaffsFromModel(data.Staffs);
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
                var degreesTask = Request.LoadDegrees();
                var departmentsTask = Request.LoadDepartments();
                var organizationsTask = Request.LoadOrganizations();
                var qualificationsTask = Request.LoadQualifications();
                var positionsTask = Request.LoadPositions();
                var specialtiesTask = Request.LoadSpecialties();

                await Task.WhenAll(departmentsTask);

                // Update UI via Dispatcher
                await Dispatcher.InvokeAsync(() =>
                {
                    vm.Degrees = new ObservableCollection<Degree>(degreesTask.Result);
                    vm.Departments = new ObservableCollection<Department>(departmentsTask.Result);
                    vm.Organizations = new ObservableCollection<Organization>(organizationsTask.Result);
                    vm.Qualifications = new ObservableCollection<Qualification>(qualificationsTask.Result);
                    vm.Positions = new ObservableCollection<Position>(positionsTask.Result);
                    vm.Specialties = new ObservableCollection<Specialty>(specialtiesTask.Result);
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
            {
                employee.Staffs.Remove(staff);
                Request.ctx.Staffs.Remove(staff); // Remove from EF context
            }

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
        private void SyncEducationsCollection(Employee employee, IEnumerable<Education> newEducations)
        {
            // Delete missing
            var toRemove = employee.Educations.Where(s => !newEducations.Any(ns => ns.Id == s.Id)).ToList();
            foreach (var education in toRemove)
            {
                employee.Educations.Remove(education);
                Request.ctx.Educations.Remove(education); // Remove from EF context
            }

            // Add new
            var toAdd = newEducations.Where(ns => !employee.Educations.Any(s => s.Id == ns.Id)).ToList();
            foreach (var education in toAdd)
                employee.Educations.Add(education);

            // Update existing
            foreach (var education in employee.Educations)
            {
                var updated = newEducations.FirstOrDefault(ns => ns.Id == education.Id);
                if (updated != null)
                {
                    education.DegreeId = updated.DegreeId;
                    education.OrganizationId = updated.OrganizationId;
                    education.QualificationId = updated.QualificationId;
                    education.SpecialtyId = updated.SpecialtyId;
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
            // Check Staffs ComboBoxes
            var comboBoxes = VisualHelper.FindVisualChildren<ComboBox>(StaffsIc).ToList();
            foreach (var combo in comboBoxes)
            {
                ValidationHelper.SetShowErrors(combo, true);
                ValidationHelper.ForceValidate(combo, ComboBox.SelectedValueProperty); // или SelectedItemProperty, если у вас биндинг по SelectedItem
                validity.Add(combo, !Validation.GetHasError(combo));
            }
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

        private void NavigationService_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Content == this) return; // Skip if navigation to current page

            if (!vm.IsChanged) return;
            var result = MessageBox.Show("Форма содержит несохраненные изменения. Вы действительно хотите уйти без сохранения данных?", "Несохраненные изменения", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
                e.Cancel = true; // Cancel navigation
        }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await SetData();
        }
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_navigationService != null)
            {
                _navigationService.Navigating -= NavigationService_Navigating;
                _navigationService = null;
            }
        }
    }
}
