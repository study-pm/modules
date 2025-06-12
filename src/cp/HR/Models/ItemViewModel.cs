using HR.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HR.Models
{
    /// <summary>
    /// Represents a view model that wraps an original data model of type <typeparamref name="T"/>
    /// and an editable model of type <typeparamref name="E"/> which implements <see cref="INotifyPropertyChanged"/>.
    /// Provides change tracking and progress state management.
    /// </summary>
    /// <typeparam name="T">The type of the original data model. Must have a parameterless constructor.</typeparam>
    /// <typeparam name="E">The type of the editable model implementing <see cref="INotifyPropertyChanged"/>. Must have a parameterless constructor.</typeparam>
    public class ItemViewModel<T, E> : INotifyPropertyChanged
        where T : new()
        where E : INotifyPropertyChanged, new()
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        private T _dm;
        /// <summary>
        /// Gets the editable model instance which supports property change notifications.
        /// </summary>
        public E Dm { get; private set; }

        public bool IsChanged => CheckIsChanged();
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemViewModel{T, E}"/> class
        /// with a new instance of <typeparamref name="T"/> as the original data model,
        /// and a new instance of <typeparamref name="E"/> as the editable model.
        /// </summary>
        public ItemViewModel() : this(new T())
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemViewModel{T, E}"/> class
        /// with the specified original data model.
        /// Creates a new editable model instance and copies the original data into it.
        /// Subscribes to property change notifications on the editable model.
        /// </summary>
        /// <param name="srcData">The original data model instance.</param>
        public ItemViewModel(T srcData)
        {
            _dm = srcData;
            Dm = new E();
            Reset();
            Dm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName != nameof(IsChanged) && e.PropertyName != nameof(IsEnabled))
                {
                    OnPropertyChanged(nameof(IsChanged));
                    OnPropertyChanged(nameof(IsEnabled));
                }
            };
        }
        /// <summary>
        /// Checks whether any property value in the editable model <see cref="Dm"/> differs from the corresponding property in the original model <see cref="_dm"/>.
        /// Compares all readable public instance properties that exist in both types by name.
        /// </summary>
        /// <returns><c>true</c> if any property value differs; otherwise, <c>false</c>.</returns>
        private bool CheckIsChanged()
        {
            if (_dm == null || Dm == null)
                return false;

            var dmType = typeof(T);
            var DmType = typeof(E);

            var DmProperties = DmType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                         .Where(p => p.CanRead);

            foreach (var DmProp in DmProperties)
            {
                var dmProp = dmType.GetProperty(DmProp.Name, BindingFlags.Public | BindingFlags.Instance);
                if (dmProp == null || !dmProp.CanRead)
                    continue;

                var originalValue = dmProp.GetValue(_dm);
                var currentValue = DmProp.GetValue(Dm);

                if (originalValue == null && currentValue == null)
                    continue;

                if (originalValue == null || currentValue == null)
                    return true;

                if (!originalValue.Equals(currentValue))
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Copies all readable public instance properties from the source object to writable matching properties in the target object.
        /// Only properties with compatible types are copied.
        /// </summary>
        /// <param name="source">The source object to copy property values from.</param>
        /// <param name="target">The target object to copy property values to.</param>
        private void CopyProperties(object source, object target)
        {
            if (source == null || target == null)
                return;

            var sourceType = source.GetType();
            var targetType = target.GetType();

            var sourceProperties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                             .Where(p => p.CanRead);

            foreach (var sourceProp in sourceProperties)
            {
                var targetProp = targetType.GetProperty(sourceProp.Name, BindingFlags.Public | BindingFlags.Instance);
                if (targetProp == null || !targetProp.CanWrite)
                    continue;

                if (!targetProp.PropertyType.IsAssignableFrom(sourceProp.PropertyType))
                    continue;

                var value = sourceProp.GetValue(source);
                targetProp.SetValue(target, value);
            }
        }
        /// <summary>
        /// Resets the editable model <see cref="Dm"/> by copying all property values from the original model <see cref="_dm"/>.
        /// Raises property changed notifications for <see cref="IsChanged"/> and <see cref="IsEnabled"/>.
        /// </summary>
        public void Reset()
        {
            CopyProperties(_dm, Dm);
            OnPropertyChanged(nameof(IsChanged));
            OnPropertyChanged(nameof(IsEnabled));
        }
        /// <summary>
        /// Applies the changes from the editable model <see cref="Dm"/> back to the original model <see cref="_dm"/>.
        /// Raises property changed notifications for <see cref="IsChanged"/> and <see cref="IsEnabled"/>.
        /// </summary>
        public void Set()
        {
            CopyProperties(Dm, _dm);
            OnPropertyChanged(nameof(IsChanged));
            OnPropertyChanged(nameof(IsEnabled));
        }
    }
}
