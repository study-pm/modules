using HR.Controls;
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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using static HR.Services.AppEventHelper;

namespace HR.Pages
{
    [XmlRoot("Preferences")]
    public class PreferencesModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        private bool _isLogOn;
        public bool IsLogOn
        {
            get => _isLogOn;
            set
            {
                if (_isLogOn == value) return;
                _isLogOn = value;
                OnPropertyChanged();
            }
        }
        private bool _isLeftAsideOff;
        public bool IsLeftAsideOff
        {
            get => _isLeftAsideOff;
            set
            {
                if (_isLeftAsideOff == value) return;
                _isLeftAsideOff = value;
                OnPropertyChanged();
            }
        }
        private bool _isRigtAsideOff;
        public bool IsRightAsideOff
        {
            get => _isRigtAsideOff;
            set
            {
                if (_isRigtAsideOff == value) return;
                _isRigtAsideOff = value;
                OnPropertyChanged();
            }
        }
        private bool _isStayLoggedIn;
        public bool IsStayLoggedIn
        {
            get => _isStayLoggedIn;
            set
            {
                if (_isStayLoggedIn == value) return;
                _isStayLoggedIn = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<int> _logCategories;
        public ObservableCollection<int> LogCategories
        {
            get => _logCategories;
            set
            {
                if (_logCategories == value) return;
                _logCategories = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<CheckableItem> _categories;
        public ObservableCollection<CheckableItem> Categories
        {
            get => _categories;
            set
            {
                if (_categories == value) return;
                // Unsubscribe from stale items, if any
                if (_categories != null)
                {
                    foreach (var item in _categories)
                        item.PropertyChanged -= CategoryItem_PropertyChanged;
                }

                _categories = value;

                // Subscribe to new items
                if (_categories != null)
                {
                    foreach (var item in _categories)
                        item.PropertyChanged += CategoryItem_PropertyChanged;
                }
                OnPropertyChanged();
            }
        }

        private ObservableCollection<int> _logTypes;
        public ObservableCollection<int> LogTypes
        {
            get => _logTypes;
            set
            {
                if (_logTypes == value) return;
                _logTypes = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<CheckableItem> _types;
        public ObservableCollection<CheckableItem> Types
        {
            get => _types;
            set
            {
                if (_types == value) return;
                // Unsubscribe from stale items, if any
                if (_types != null)
                {
                    foreach (var item in _types)
                        item.PropertyChanged -= TypeItem_PropertyChanged;
                }

                _types = value;

                // Subscribe to new items
                if (_types != null)
                {
                    foreach (var item in _types)
                        item.PropertyChanged += TypeItem_PropertyChanged;
                }
                OnPropertyChanged();
            }
        }
        public PreferencesModel()
        {
            // Initialize Categories, add event subscribers
            Categories = new ObservableCollection<CheckableItem>(
                Enum.GetValues(typeof(AppEventHelper.EventCategory))
                    .Cast<AppEventHelper.EventCategory>()
                    .Select(c => new CheckableItem
                    {
                        Id = (int)c,
                        Name = c.ToString(),
                        Title = c.ToTitle(),
                        IsChecked = false
                    })
            );
            // Initialize Types, add event subscribers
            Types = new ObservableCollection<CheckableItem>(
                Enum.GetValues(typeof(AppEventHelper.EventType))
                    .Cast<AppEventHelper.EventType>()
                    .Select(c => new CheckableItem
                    {
                        Id = (int)c,
                        Name = c.ToString(),
                        Title = c.ToTitle(),
                        IsChecked = false
                    })
            );
        }
        private void CategoryItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CheckableItem.IsChecked))
            {
                var item = sender as CheckableItem;
                if (item == null) return;

                if (item.IsChecked)
                {
                    if (!LogCategories.Contains(item.Id))
                        LogCategories.Add(item.Id);
                }
                else
                {
                    if (LogCategories.Contains(item.Id))
                        LogCategories.Remove(item.Id);
                }
            }
        }
        public void SyncCheckedCategories()
        {
            if (Categories == null || LogCategories == null)
                return;

            var selectedIds = LogCategories.ToHashSet();

            foreach (var categoryItem in Categories)
            {
                categoryItem.IsChecked = selectedIds.Contains(categoryItem.Id);
            }
        }
        public void SyncCheckedTypes()
        {
            if (Types == null || LogTypes == null)
                return;

            var selectedIds = LogTypes.ToHashSet();

            foreach (var categoryItem in Types)
            {
                categoryItem.IsChecked = selectedIds.Contains(categoryItem.Id);
            }
        }
        public void SyncCollections()
        {
            SyncCheckedCategories();
            SyncCheckedTypes();
        }

        private void TypeItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CheckableItem.IsChecked))
            {
                var item = sender as CheckableItem;
                if (item == null) return;

                if (item.IsChecked)
                {
                    if (!LogTypes.Contains(item.Id))
                        LogTypes.Add(item.Id);
                }
                else
                {
                    if (LogTypes.Contains(item.Id))
                        LogTypes.Remove(item.Id);
                }
            }
        }
    }

    /// <summary>
    /// Interaction logic for PreferencesPg.xaml
    /// </summary>
    public partial class PreferencesPg : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        private MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

        private NavigationService _navigationService;

        private int uid = ((App)(Application.Current)).CurrentUser.Id;

        private string prefsDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Preferences");
        private string prefsPath;

        public RelayCommand ResetCmd { get; }
        public RelayCommand SubmitCmd { get; }

        private ItemViewModel<Preferences, PreferencesModel> _vM;
        public ItemViewModel<Preferences, PreferencesModel> vm
        {
            get => _vM;
            set
            {
                _vM = value;
                OnPropertyChanged();
            }
        }
        public PreferencesPg()
        {
            InitializeComponent();
            Unloaded += Page_Unloaded;

            prefsPath = System.IO.Path.Combine(prefsDir, uid.ToString() + ".xml");
            vm = new ItemViewModel<Preferences, PreferencesModel>(App.Current.Preferences);
            DataContext = vm;
            vm.Dm.SyncCollections();

            ResetCmd = new RelayCommand(
                _ => {
                        vm.Reset();
                        vm.Dm.SyncCollections();
                    },
                _ => vm.IsEnabled
            );

            SubmitCmd = new RelayCommand(
                _ => Save(),
                _ => vm.IsEnabled
            );

            // Subscribe to navigation events for the main frame
            _navigationService = MainWindow.frame.NavigationService;
            if (_navigationService != null)
            {
                _navigationService.Navigating += NavigationService_Navigating;
            }
        }
        private async void Save()
        {
            try
            {
                vm.IsProgress = true;
                RaiseAppEvent(new AppEventArgs { Category = EventCategory.Data, Type = EventType.Progress, Message = "Сохранение предпочтений пользователя" });
                await XmlHelper.SaveAsync(vm.Preset(), prefsPath);
                vm.Set();
                RaiseAppEvent(new AppEventArgs { Category = EventCategory.Data, Type = EventType.Success, Message = "Предпочтения пользователя успешно сохранены" });
                // Handle user authentication state file
                if (vm.Dm.IsStayLoggedIn) await Services.Request.SaveUidToFileAsync(uid, Data.Models.User.uidFilePath);
                else await Request.DeleteUidFileAsync(Data.Models.User.uidFilePath);

                mainWindow.UpdateVisibility(mainWindow.ActualWidth); // Toggle panel visibility instantly
            }
            catch (Exception exc)
            {
                RaiseAppEvent(new AppEventArgs { Category = EventCategory.Data, Type = EventType.Error, Message = "Ошибка сохранения предпочтений пользователя", Details = exc.Message });
            }
            finally
            {
                vm.IsProgress = false;
            }
        }

        private void NavigationService_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Content == this) return; // Skip if navigation to current page

            if (!vm.IsChanged) return;
            var result = MessageBox.Show("Форма содержит несохраненные изменения. Вы действительно хотите уйти без сохранения данных?", "Несохраненные изменения", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
                e.Cancel = true; // Cancel navigation
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
