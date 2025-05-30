using HR.Controls;
using HR.Data.Models;
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

namespace HR.Pages
{
    public class PreferencesViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        internal Preferences model;
        public bool IsChanged => model.IsStayLoggedIn != IsStayLoggedIn;
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
        private bool _isStayLoggedIn;
        public bool IsStayLoggedIn
        {
            get => _isStayLoggedIn;
            set
            {
                if (_isStayLoggedIn == value) return;
                _isStayLoggedIn = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsChanged));
                OnPropertyChanged(nameof(IsEnabled));
            }
        }
        public PreferencesViewModel()
        {
            IsStayLoggedIn = false;
        }
        public PreferencesViewModel(Preferences dataModel)
        {
            model = dataModel;
            IsStayLoggedIn = model.IsStayLoggedIn;
        }
        public void Reset()
        {
            IsStayLoggedIn = model.IsStayLoggedIn;
        }
        public void Set()
        {
            model.IsStayLoggedIn = IsStayLoggedIn;
            OnPropertyChanged(nameof(IsChanged));
            OnPropertyChanged(nameof(IsEnabled));
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
        private PreferencesViewModel _vM;
        public PreferencesViewModel vm
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
            vm = new PreferencesViewModel();
            DataContext = vm;
        }
        private async Task<PreferencesViewModel> GetPreferences()
        {
            try
            {
                vm.IsInProgress = true;
                return new PreferencesViewModel(await Services.Request.GetPreferences(uid));
            }
            catch (Exception exc)
            {
                StatusInformer.ReportFailure($"Ошибка извлечения предпочтений пользователя: ${exc.Message}");
                return null;
            }
            finally
            {
                vm.IsInProgress = false;
                StatusInformer.ReportSuccess("Предпочтения пользователя успешно извлечены");
            }
        }
        private async Task SetPreferences()
        {
            try
            {
                vm.IsInProgress = true;
                StatusInformer.ReportProgress("Сохранение предпочтений пользователя");
                await vm.model.SaveAsync();
                StatusInformer.ReportSuccess("Предпочтения пользователя успешно сохранены");
            }
            catch (Exception exc)
            {
                StatusInformer.ReportFailure($"Ошибка сохранения предпочтений пользователя: ${exc.Message}");
            }
            finally
            {
                // Restore persisted data model
                vm.model = (await GetPreferences()).model;
                vm.IsInProgress = false;
            }
        }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            vm = await GetPreferences();
            DataContext = vm;
        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            vm.Reset();
        }

        private async void SubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            vm.Set();
            await SetPreferences();
            // Handle user authentication state file
            if (vm.IsStayLoggedIn) await Services.Request.SaveUidToFileAsync(uid, Data.Models.User.uidFilePath);
            else await Request.DeleteUidFileAsync(Data.Models.User.uidFilePath);
        }
    }
}
