using HR.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HR.Utilities
{
    public class FilterParam
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public FilterParam(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }
    public class MenuFilter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        public Geometry Icon { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        private bool isChecked;
        public bool IsChecked
        {
            get => isChecked;
            set
            {
                if (isChecked == value) return;
                isChecked = value;
                if (!value) AllChecked = false;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AllChecked));
            }
        }
        private string pageUri;
        public string PageUri
        {
            get => pageUri;
            set
            {
                if (pageUri != value)
                {
                    pageUri = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Page {  get; set; }
        private ObservableCollection<FilterValue> values = new ObservableCollection<FilterValue>();
        public ObservableCollection<FilterValue> Values
        {
            get => values;
            set
            {
                if (values == value) return;
                // Unsubscribe for collection changes
                if (values != null)
                {
                    values.CollectionChanged -= Values_CollectionChanged;
                    foreach (var v in values)
                        v.PropertyChanged -= Value_PropertyChanged;
                }

                values = value;

                // Subscribe for collection changes
                if (values != null)
                {
                    values.CollectionChanged += Values_CollectionChanged;
                    foreach (var v in values)
                        v.PropertyChanged += Value_PropertyChanged;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(AllChecked));
            }
        }
        private void Values_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(AllChecked));
        }

        private void Value_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FilterValue.IsChecked))
            {
                OnPropertyChanged(nameof(AllChecked));
                OnPropertyChanged(nameof(AnyChecked));
                if (AnyChecked && !IsChecked) IsChecked = true;
                else if (!AnyChecked && IsChecked) IsChecked = false;
            }
        }
        public bool AllChecked
        {
            get => Values != null ? Values.All(x => x.IsChecked) : false;
            set
            {
                foreach (var v in Values)
                    v.IsChecked = value;
                OnPropertyChanged();
            }
        }
        public bool AnyChecked => Values.Any(x => x.IsChecked);
        public MenuFilter()
        {
            Values = new ObservableCollection<FilterValue>();
        }
    }
    public class FilterValue : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public int Id { get; set; }
        public string Title { get; set; }
        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChecked)));
                }
            }
        }
    }
}
