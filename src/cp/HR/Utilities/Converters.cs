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
                // Alterate color for even and odd rows
                return (index % 2 == 0) ? Brushes.Transparent : new SolidColorBrush(Color.FromRgb(250, 250, 250)); // #F0F8FF
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class CapitalizeFirstLetterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string input && !string.IsNullOrWhiteSpace(input))
            {
                if (input.Length == 1)
                    return input.ToUpper(culture);

                return char.ToUpper(input[0], culture) + input.Substring(1);
            }

            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("ConvertBack is not supported.");
        }
    }
    public class CollectionToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool notInverse = true; // Default param value (not inverted)
            if (parameter is string paramString)
            {
                // Try parsing param as bool
                if (bool.TryParse(paramString, out bool parsed)) notInverse = parsed;
            }
            if (value is ICollection collection)
            {
                if (collection.Count > 0)
                    return notInverse ? Visibility.Visible : Visibility.Collapsed;
                else
                    return notInverse ? Visibility.Collapsed : Visibility.Hidden;
            }
            else if (value is IEnumerable enumerable)
            {
                bool hasAny = enumerable.Cast<object>().Any();
                if (hasAny)
                    return notInverse ? Visibility.Visible : Visibility.Collapsed;
                else
                    return notInverse ? Visibility.Collapsed : Visibility.Hidden;
            }
            return notInverse ? Visibility.Collapsed : Visibility.Hidden;
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
            if (!(value is string imageName) || String.IsNullOrWhiteSpace(imageName))
                return fallbackImgPath;

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
                return Fs.CreateBitmapImage(fallbackImgPath);
            }
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
            // return Binding.DoNothing; // if two way binding is not used
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
    public class IntToSymbolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue && intValue != 0)
            {
                string symbol = parameter as string ?? ": ";
                return symbol;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class IntToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                // Hide element if valus is 0
                return intValue == 0 ? Visibility.Collapsed : Visibility.Visible;
            }
            // Hide element if value is not int or null
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class NotNullOrEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;

            if (value is string str)
                return string.IsNullOrEmpty(str) ? Visibility.Collapsed : Visibility.Visible;

            if (value is ICollection collection)
                return collection.Count == 0 ? Visibility.Collapsed : Visibility.Visible;

            // Check for null for other types
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
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
    public class NullOrEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;
            return string.IsNullOrEmpty(str) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class PageToIsEnabledConverter : IValueConverter
    {
        // value - current page name (string)
        // parameter - menu item page name (string)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string currentPage = value as string;
            string menuPage = parameter as string;

            if (string.IsNullOrEmpty(currentPage) || string.IsNullOrEmpty(menuPage))
                return true; // default available menu item

            // Item is not available if current page matches menu item (IsEnabled = false)
            return !string.Equals(currentPage, menuPage, StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class PageToIsCheckedConverter : IValueConverter
    {
        // value - current page(string)
        // parameter - string with comma-separated pages list associated with menu item
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string currentPage = value as string;
            string pagesParam = parameter as string;

            if (string.IsNullOrEmpty(currentPage) || string.IsNullOrEmpty(pagesParam))
                return false;

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
                    return Application.Current.TryFindResource("UserCheckSolidPath") as Geometry;
                case 2:
                    return Application.Current.TryFindResource("UserLockSolidPath") as Geometry;
                case 3:
                    return Application.Current.TryFindResource("UserTimesSolidPath") as Geometry;
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
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;
            if (string.IsNullOrWhiteSpace(text))
                return Visibility.Collapsed;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class VisibilityToBooleanConverter : IValueConverter
    {
        // Transform Visibility to bool: Visible -> true, Collapsed/Hidden -> false
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
                return visibility == Visibility.Visible;
            return false;
        }

        // Transform bool to Visibility
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool flag)
                return flag ? Visibility.Visible : Visibility.Collapsed;
            return Visibility.Collapsed;
        }
    }
}
