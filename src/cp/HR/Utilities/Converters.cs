using HR.Controls;
using HR.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HR.Utilities
{
    public class AddOneConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int index)
            {
                return (index + 1).ToString();
            }
            return "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class AlternationIndexToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int index)
            {
                // Чередуем цвет для чётных и нечётных строк
                return (index % 2 == 0) ? Brushes.Transparent : new SolidColorBrush(Color.FromRgb(250, 250, 250)); // #F0F8FF
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class CollectionToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = value as ICollection;
            if (collection != null && collection.Count > 0)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ImagePathConverter : IValueConverter
    {
        private string basePath = "Images";
        private string extension = "jpg";
        private string fallbackImgPath = "/Static/Img/no-photo-male.jpg";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string) || String.IsNullOrWhiteSpace((string)value)) return fallbackImgPath;
            return $"/{basePath}/{value}.{extension}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
    public class ImagePathMultiConverter : IMultiValueConverter
    {
        private string basePath = "Images";
        private string extension = "jpg";
        private string fallbackImgPathPrefix = "pack://application:,,,/Static/Img/no-photo-";

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return null;

            string imageName = values[0] as string;
            bool gender = false;
            if (values[1] is bool)
                gender = (bool)values[1];

            string fallbackPath = fallbackImgPathPrefix + (gender ? "female" : "male") + "." + extension;

            // If no image specified, use fallback
            if (string.IsNullOrWhiteSpace(imageName))
                return Fs.CreateBitmapImage(fallbackPath);

            // Add default extension if imageName has none
            if (!Path.HasExtension(imageName)) imageName += "." + extension;

            // 1. If imageName is an absolute path and exists, load from file
            if (File.Exists(imageName))
                return Fs.CreateBitmapImageFromFile(imageName);

            // 2. Search beside the executive file (i.e. Images/imageName)
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(appDir, basePath, imageName);
            // Try adding the default extension if file not found
            if (!File.Exists(filePath) && !Path.HasExtension(filePath))
                filePath += "." + extension;

            if (File.Exists(filePath))
                return Fs.CreateBitmapImageFromFile(filePath);

            // 3. Search Images in parent project's folders (ascending)
            string projectRootImages = Fs.FindInParentDirectories(appDir, basePath, imageName, extension);
            if (projectRootImages != null)
                return Fs.CreateBitmapImageFromFile(projectRootImages);

            // Try to load as pack resource (legacy images)
            string resourcePath = $"pack://application:,,,/{basePath}/{imageName}";
            if (!resourcePath.EndsWith($".{extension}"))
                resourcePath += $".{extension}";

            try
            {
                return Fs.CreateBitmapImage(resourcePath);
            }
            catch
            {
                // Fallback if nothing found
                return Fs.CreateBitmapImage(fallbackPath);
            }
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b) return !b;
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b) return !b;
            return true;
        }
    }
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b ? Visibility.Collapsed : Visibility.Visible;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
            // Если не нужна двунаправленная привязка, можно вернуть Binding.DoNothing
            // return Binding.DoNothing;
    }
    public class ListToStringConverter : IValueConverter
    {
        private object GetNestedPropertyValue(object obj, string propertyPath)
        {
            if (obj == null || string.IsNullOrEmpty(propertyPath))
                return null;

            string[] parts = propertyPath.Split('.');
            object currentObject = obj;

            foreach (var part in parts)
            {
                if (currentObject == null)
                    return null;

                var prop = currentObject.GetType().GetProperty(part);
                if (prop == null)
                    return null;

                currentObject = prop.GetValue(currentObject);
            }

            return currentObject;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = value as IEnumerable;
            if (collection == null)
                return string.Empty;

            string propertyName = parameter as string ?? "Name";

            var list = new List<string>();
            foreach (var item in collection)
            {
                var val = GetNestedPropertyValue(item, propertyName);
                if (val != null)
                {
                    string strVal = val.ToString();
                    if (!string.IsNullOrEmpty(strVal))
                        list.Add(strVal);
                }
            }

            return string.Join(", ", list);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ErrorVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool hasError = values[0] is bool b1 && b1;
            bool isFocused = values[1] is bool b2 && b2;
            return hasError && !isFocused ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
    public class NavigationDataConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return null;

            var filterValues = values[0] as IEnumerable<FilterValue>;
            var uri = values[1] as string;

            if (filterValues == null || string.IsNullOrEmpty(uri))
                return null;

            // Можно скопировать коллекцию или передать как есть
            var selectedValues = filterValues.Where(fv => fv.IsChecked).ToList();

            return new NavigationData
            {
                Uri = uri,
                Parameter = selectedValues
            };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
    public class NotNullToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    public class PageToIsEnabledConverter : IValueConverter
    {
        // value - имя текущей страницы (string)
        // parameter - имя страницы для пункта меню (string)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string currentPage = value as string;
            string menuPage = parameter as string;

            if (string.IsNullOrEmpty(currentPage) || string.IsNullOrEmpty(menuPage))
                return true; // пункт меню доступен по умолчанию

            // Если текущая страница совпадает с пунктом меню - пункт не доступен (IsEnabled = false)
            return !string.Equals(currentPage, menuPage, StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class PageToIsCheckedConverter : IValueConverter
    {
        // value - текущая страница (string)
        // parameter - строка с перечнем страниц, связанных с пунктом меню, разделенных запятыми
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string currentPage = value as string;
            string pagesParam = parameter as string;

            if (string.IsNullOrEmpty(currentPage) || string.IsNullOrEmpty(pagesParam))
                return true;

            var pages = pagesParam.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                  .Select(p => p.Trim());

            return pages.Contains(currentPage, StringComparer.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class InvertedPageToIsCheckedConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return false;

            var pageParam = values[0] as string;
            var title = values[1] as string;

            if (pageParam == null || title == null)
                return false;

            // Логика сравнения, например:
            return pageParam.Equals(title, StringComparison.OrdinalIgnoreCase);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class StatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is byte status)
            {
                switch (status)
                {
                    case 0:
                        return Brushes.Blue;
                    case 1:
                        return Brushes.Green;
                    case 2:
                        return Brushes.Red;
                    default:
                        return Brushes.Gray;
                }
            }
            return Brushes.Gray;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class StatusToPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is byte status)) return null;
            switch (status)
            {
                case 0:
                    return Application.Current.TryFindResource("UserRegularPath") as Geometry;
                case 1:
                    return Application.Current.TryFindResource("CheckSolidPath") as Geometry;
                case 2:
                    return Application.Current.TryFindResource("LockSolidPath") as Geometry;
                default:
                    return null;
            }

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !string.IsNullOrWhiteSpace(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
