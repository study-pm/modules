using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace HR.Utilities
{
    public class NotEmptyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            /*
             var str = value as string;
            if (string.IsNullOrWhiteSpace(str))
                return new ValidationResult(false, "Поле обязательно для заполнения");
            return ValidationResult.ValidResult;
            */
            //return new ValidationResult(true, null);
            string input = value as string;
            if (string.IsNullOrWhiteSpace(input))
                return new ValidationResult(false, "Поле обязательно для заполнения");
            /*
            if (input.Length < 5)
                return new ValidationResult(false, "Поле должно быть содержать более {5} символов");
            */
            return new ValidationResult(true, null);
            // return new ValidationResult(false, "Пользователь {логин} не найден");
        }
    }
    public class MinLengthValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string input = value as string;
            if (input.Length < 5)
                return new ValidationResult(false, "Поле должно быть содержать более {5} символов");
            return new ValidationResult(true, null);
        }
    }
    public class PasswordLengthValidationRule : ValidationRule
    {
        public int MinLength { get; set; } = 6;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var str = value as string;
            if (string.IsNullOrEmpty(str) || str.Length < MinLength)
                return new ValidationResult(false, $"Минимум {MinLength} символов");
            return ValidationResult.ValidResult;
        }
    }
    public static class ValidationHelper
    {
        public static readonly DependencyProperty AutoErrorVisibilityProperty =
            DependencyProperty.RegisterAttached(
                "AutoErrorVisibility",
                typeof(bool),
                typeof(ValidationHelper),
                new PropertyMetadata(false, OnAutoErrorVisibilityChanged));

        public static bool GetAutoErrorVisibility(DependencyObject obj) => (bool)obj.GetValue(AutoErrorVisibilityProperty);
        public static void SetAutoErrorVisibility(DependencyObject obj, bool value) => obj.SetValue(AutoErrorVisibilityProperty, value);

        private static void OnAutoErrorVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                if ((bool)e.NewValue)
                {
                    element.GotFocus += Element_GotFocus;
                    element.LostFocus += Element_LostFocus;
                }
                else
                {
                    element.GotFocus -= Element_GotFocus;
                    element.LostFocus -= Element_LostFocus;
                }
            }
        }
        public static readonly DependencyProperty TouchedProperty =
        DependencyProperty.RegisterAttached(
            "Touched",
            typeof(bool),
            typeof(ValidationHelper),
            new PropertyMetadata(false));

        public static bool GetTouched(DependencyObject obj) => (bool)obj.GetValue(TouchedProperty);
        public static void SetTouched(DependencyObject obj, bool value) => obj.SetValue(TouchedProperty, value);

        public static readonly DependencyProperty EnableTouchedTrackingProperty =
            DependencyProperty.RegisterAttached(
                "EnableTouchedTracking",
                typeof(bool),
                typeof(ValidationHelper),
                new PropertyMetadata(false, OnEnableTouchedTrackingChanged));

        public static bool GetEnableTouchedTracking(DependencyObject obj) => (bool)obj.GetValue(EnableTouchedTrackingProperty);
        public static void SetEnableTouchedTracking(DependencyObject obj, bool value) => obj.SetValue(EnableTouchedTrackingProperty, value);

        private static void OnEnableTouchedTrackingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                if ((bool)e.NewValue)
                    element.LostFocus += Element_LostFocus;
                else
                    element.LostFocus -= Element_LostFocus;
            }
        }

        private static void Element_GotFocus(object sender, RoutedEventArgs e)
        {
            SetErrorTextBlockVisibility(sender as UIElement, Visibility.Collapsed);
        }

        private static void Element_LostFocus(object sender, RoutedEventArgs e)
        {
            SetErrorTextBlockVisibility(sender as UIElement, Visibility.Visible);
            if (sender is DependencyObject d)
                SetTouched(d, true);
        }

        private static void SetErrorTextBlockVisibility(UIElement element, Visibility visibility)
        {
            if (element == null) return;

            var adornerLayer = AdornerLayer.GetAdornerLayer(element);
            if (adornerLayer == null) return;

            var adorners = adornerLayer.GetAdorners(element);
            if (adorners == null) return;

            foreach (var adorner in adorners)
            {
                var textBlock = FindTextBlock(adorner);
                if (textBlock != null)
                    textBlock.Visibility = visibility;
            }
        }

        private static TextBlock FindTextBlock(DependencyObject parent)
        {
            if (parent == null) return null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is TextBlock tb)
                    return tb;
                var result = FindTextBlock(child);
                if (result != null)
                    return result;
            }
            return null;
        }
    }

}
