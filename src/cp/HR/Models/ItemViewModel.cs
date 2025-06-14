using HR.Data.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        /// Determines whether the given pair of types represent a collection pair consisting of a List and an ObservableCollection, in either order.
        /// </summary>
        /// <param name="type1">The first type to check.</param>
        /// <param name="type2">The second type to check.</param>
        /// <returns><c>true</c> if one type is a generic List and the other is a generic ObservableCollection; otherwise, <c>false</c>.</returns>
        private bool IsCollectionPair(Type type1, Type type2)
        {
            return (IsGenericType(type1, typeof(List<>)) && IsGenericType(type2, typeof(ObservableCollection<>))) ||
                   (IsGenericType(type1, typeof(ObservableCollection<>)) && IsGenericType(type2, typeof(List<>)));
        }
        /// <summary>
        /// Determines whether a given type is a generic type of a specified generic type definition.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="genericType">The generic type definition to compare against (e.g., typeof(List<>)).</param>
        /// <returns><c>true</c> if <paramref name="type"/> is a generic type and its generic type definition matches <paramref name="genericType"/>; otherwise, <c>false</c>.</returns>
        private bool IsGenericType(Type type, Type genericType)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == genericType;
        }
        /// <summary>
        /// Compares two collections to determine if they contain different elements, regardless of order and duplicates.
        /// </summary>
        /// <param name="first">The first collection to compare.</param>
        /// <param name="second">The second collection to compare.</param>
        /// <returns><c>true</c> if the collections are different (different counts or different sets of elements); otherwise, <c>false</c>.</returns>
        private bool AreCollectionsDifferent(System.Collections.IEnumerable first, System.Collections.IEnumerable second)
        {
            if (ReferenceEquals(first, second))
                return false;

            if (first == null || second == null)
                return true;

            var firstList = first.Cast<object>().ToList();
            var secondList = second.Cast<object>().ToList();

            if (firstList.Count != secondList.Count)
                return true;

            var firstSet = new HashSet<object>(firstList);
            var secondSet = new HashSet<object>(secondList);

            return !firstSet.SetEquals(secondSet);
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

                // Check if props are List<T> and ObservableCollection<T> collections
                if (IsCollectionPair(originalValue.GetType(), currentValue.GetType()))
                {
                    if (AreCollectionsDifferent(originalValue as System.Collections.IEnumerable, currentValue as System.Collections.IEnumerable))
                        return true;
                    else
                        continue;
                }

                // Compare with equals only if not collections
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

                var sourceValue = sourceProp.GetValue(source);
                if (sourceValue == null)
                {
                    targetProp.SetValue(target, null);
                    continue;
                }

                // If types match and targetProp can get sourceProp directly
                if (targetProp.PropertyType.IsAssignableFrom(sourceProp.PropertyType))
                {
                    targetProp.SetValue(target, sourceValue);
                    continue;
                }

                // Copy from List<T> to ObservableCollection<T>
                if (IsGenericType(sourceProp.PropertyType, typeof(List<>)) &&
                    IsGenericType(targetProp.PropertyType, typeof(ObservableCollection<>)))
                {
                    var itemType = sourceProp.PropertyType.GetGenericArguments()[0];
                    var list = (System.Collections.IEnumerable)sourceValue;

                    var observableCollectionType = typeof(ObservableCollection<>).MakeGenericType(itemType);
                    var newObservableCollection = Activator.CreateInstance(observableCollectionType, list);

                    targetProp.SetValue(target, newObservableCollection);
                    continue;
                }

                // Copy from ObservableCollection<T> to List<T>
                if (IsGenericType(sourceProp.PropertyType, typeof(ObservableCollection<>)) &&
                    IsGenericType(targetProp.PropertyType, typeof(List<>)))
                {
                    var itemType = sourceProp.PropertyType.GetGenericArguments()[0];
                    var observableCollection = (System.Collections.IEnumerable)sourceValue;

                    var listType = typeof(List<>).MakeGenericType(itemType);
                    var newList = Activator.CreateInstance(listType, observableCollection);

                    targetProp.SetValue(target, newList);
                    continue;
                }
            }
        }
        /// <summary>
        /// Creates a new instance of type <typeparamref name="T"/> and copies the properties from the <c>Dm</c> object to this new instance.
        /// </summary>
        /// <typeparam name="T">The type of the object to create and copy properties to. Must have a parameterless constructor.</typeparam>
        /// <returns>A new instance of <typeparamref name="T"/> with properties copied from <c>Dm</c>.</returns>
        public T Preset()
        {
            var preset = new T();
            CopyProperties(Dm, preset);
            return preset;
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
