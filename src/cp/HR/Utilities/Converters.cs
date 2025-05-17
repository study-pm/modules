using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

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

}
