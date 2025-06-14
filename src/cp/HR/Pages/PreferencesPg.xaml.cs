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
                // Отписываемся от старых элементов, если нужно
                if (_categories != null)
                {
                    foreach (var item in _categories)
                        item.PropertyChanged -= CategoryItem_PropertyChanged;
                }

                _categories = value;

                // Подписываемся на новые элементы
                if (_categories != null)
                {
                    foreach (var item in _categories)
                        item.PropertyChanged += CategoryItem_PropertyChanged;
                }
                OnPropertyChanged();
            }
        }
        public PreferencesModel()
        {
            // Инициализируем Categories и подписываемся на события
            Categories = new ObservableCollection<CheckableItem>(
                Enum.GetValues(typeof(AppEventHelper.EventCategory))
                    .Cast<AppEventHelper.EventCategory>()
                    .Select(c => new CheckableItem
                    {
                        Id = (int)c,
                        Title = c.ToString(),
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
    }

    /// <summary>
    /// Interaction logic for PreferencesPg.xaml
    /// </summary>
    public partial class PreferencesPg : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

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
            prefsPath = System.IO.Path.Combine(prefsDir, uid.ToString() + ".xml");
            vm = new ItemViewModel<Preferences, PreferencesModel>(App.Current.Preferences);
            DataContext = vm;
            vm.Dm.SyncCheckedCategories();

            ResetCmd = new RelayCommand(
                _ => {
                        vm.Reset();
                        vm.Dm.SyncCheckedCategories();
                    },
                _ => vm.IsEnabled
            );

            SubmitCmd = new RelayCommand(
                _ => Save(),
                _ => vm.IsEnabled
            );
        }
        private async void Save()
        {
            try
            {
                vm.IsProgress = true;
                RaiseAppEvent(new AppEventArgs { Category = EventCategory.Data, Type = EventType.Progress, Message = "Сохранение предпочтений пользователя" });
                MessageBox.Show(App.Current.Preferences.LogCategories.Count.ToString());
                await XmlHelper.SaveAsync(vm.Preset(), prefsPath);
                vm.Set();
                RaiseAppEvent(new AppEventArgs { Category = EventCategory.Data, Type = EventType.Success, Message = "Предпочтения пользователя успешно сохранены" });
                // Handle user authentication state file
                if (vm.Dm.IsStayLoggedIn) await Services.Request.SaveUidToFileAsync(uid, Data.Models.User.uidFilePath);
                else await Request.DeleteUidFileAsync(Data.Models.User.uidFilePath);
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
    }
}
