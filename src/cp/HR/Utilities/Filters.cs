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
    /// <summary>
    /// Represents a filter menu item that supports property change notifications and manages a collection of filter values.
    /// </summary>
    public class MenuFilter : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for the specified property.
        /// </summary>
        /// <param name="prop">The name of the property that changed. Automatically supplied by the caller.</param>
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        /// <summary>
        /// Gets or sets the icon graphic associated with the menu filter.
        /// </summary>
        public Geometry Icon { get; set; }
        /// <summary>
        /// Gets or sets the internal name identifier of the menu filter.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the display title of the menu filter.
        /// </summary>
        public string Title { get; set; }
        private bool isChecked;
        /// <summary>
        /// Gets or sets a value indicating whether the menu filter is checked.
        /// Setting this property raises notifications for <see cref="IsChecked"/> and <see cref="AllChecked"/>.
        /// If set to false, it also resets <see cref="AllChecked"/> to false.
        /// </summary>
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
        private ObservableCollection<FilterValue> values = new ObservableCollection<FilterValue>();
        /// <summary>
        /// Gets or sets the collection of <see cref="FilterValue"/> items associated with this filter.
        /// Subscribes to collection and item property changes to update <see cref="AllChecked"/> accordingly.
        /// </summary>
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
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether all <see cref="FilterValue"/> items in <see cref="Values"/> are checked.
        /// Setting this property sets the <see cref="FilterValue.IsChecked"/> property of all contained items and raises a property changed notification.
        /// </summary>
        public bool AllChecked
        {
            get => Values.All(x => x.IsChecked);
            set
            {
                foreach (var v in Values)
                    v.IsChecked = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="MenuFilter"/> class with an empty collection of filter values.
        /// </summary>
        public MenuFilter()
        {
            Values = new ObservableCollection<FilterValue>();
        }
    }
    /// <summary>
    /// Represents an individual filter option with a checked state and supports property change notifications.
    /// </summary>
    public class FilterValue : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when the <see cref="IsChecked"/> property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Gets or sets the unique identifier for the filter value.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the display title of the filter value.
        /// </summary>
        public string Title { get; set; }
        private bool _isChecked;
        /// <summary>
        /// Gets or sets a value indicating whether the filter value is checked.
        /// Setting this property raises a property changed notification.
        /// </summary>
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
