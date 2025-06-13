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

            ResetCmd = new RelayCommand(
                _ => vm.Reset(),
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
                await XmlHelper.SaveAsync(vm.Dm, prefsPath);
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
