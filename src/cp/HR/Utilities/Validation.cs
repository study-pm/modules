﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace HR.Utilities
{
    public class DigitsOnlyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string input = value as string ?? string.Empty;

            // Проверяем, что строка содержит только цифры
            foreach (char c in input)
            {
                if (!char.IsDigit(c))
                {
                    return new ValidationResult(false, "Допустимы только цифры");
                }
            }

            return ValidationResult.ValidResult;
        }
    }
    public class ExactLengthValidationRule : ValidationRule
    {
        public int RequiredLength { get; set; } = 6;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string input = value as string ?? string.Empty;

            if (input.Length == RequiredLength)
            {
                return ValidationResult.ValidResult;
            }
            else
            {
                return new ValidationResult(false, $"Длина должна быть ровно {RequiredLength} символов");
            }
        }
    }
    public class MaxLengthValidationRule : ValidationRule
    {
        public int MaxLength { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var str = value as string ?? string.Empty;

            if (str.Length > MaxLength)
            {
                return new ValidationResult(false, $"Должно быть не более {MaxLength} символов");
            }

            return ValidationResult.ValidResult;
        }
    }
    public class MinLengthValidationRule : ValidationRule
    {
        public int MinLength { get; set; }
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var str = value as string ?? string.Empty;

            if (str.Length < MinLength)
                return new ValidationResult(false, $"Должно быть не менее {MinLength} символов");

            return new ValidationResult(true, null);
        }
    }
    public class NotEmptyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null)
                return new ValidationResult(false, "Обязательно для заполнения");
            if (value is int intVal && intVal == 0)
            {
                return new ValidationResult(false, "Обязательно для заполнения");
            }
            if (value is string strVal && string.IsNullOrWhiteSpace(strVal))
            {
                return new ValidationResult(false, "Обязательно для заполнения");
            }
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
    public class StrongPasswordValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string password = value as string ?? string.Empty;

            if (password.Length < 8)
            {
                return new ValidationResult(false, "Должен содержать не менее 8 символов");
            }

            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                return new ValidationResult(false, "Должен содержать хотя бы одну заглавную букву");
            }

            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                return new ValidationResult(false, "Должен содержать хотя бы одну строчную букву");
            }

            if (!Regex.IsMatch(password, @"\d"))
            {
                return new ValidationResult(false, "Должен содержать хотя бы одну цифру");
            }

            if (!Regex.IsMatch(password, @"[\W_]"))
            {
                return new ValidationResult(false, "Должен содержать хотя бы один специальный символ");
            }

            return ValidationResult.ValidResult;
        }
    }

    public static class ValidationHelper
    {
        // Автоматическое управление видимостью ошибок (автоматически скрывать/показывать при фокусе)
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

        // Управление ручным показом ошибок (используется в шаблоне ошибки)
        public static readonly DependencyProperty ShowErrorsProperty =
            DependencyProperty.RegisterAttached(
                "ShowErrors",
                typeof(bool),
                typeof(ValidationHelper),
                new PropertyMetadata(false));

        public static bool GetShowErrors(DependencyObject obj) => (bool)obj.GetValue(ShowErrorsProperty);
        public static void SetShowErrors(DependencyObject obj, bool value) => obj.SetValue(ShowErrorsProperty, value);

        // Флаг "Touched" - был ли пользователь в поле (для UX-дружественной валидации)
        public static readonly DependencyProperty TouchedProperty =
            DependencyProperty.RegisterAttached(
                "Touched",
                typeof(bool),
                typeof(ValidationHelper),
                new PropertyMetadata(false));

        public static bool GetTouched(DependencyObject obj) => (bool)obj.GetValue(TouchedProperty);
        public static void SetTouched(DependencyObject obj, bool value) => obj.SetValue(TouchedProperty, value);

        // Включение трекинга "Touched" (автоматически ставит Touched=true при первом выходе из поля)
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
            if (!(d is UIElement element)) return;

            if ((bool)e.NewValue)
            {
                if (element is TextBox tb) tb.TextChanged += Element_TextInput;
                else if (element is PasswordBox pb) pb.PasswordChanged += Element_TextInput;
            }
            else
            {
                if (element is TextBox tb) tb.TextChanged += Element_TextInput;
                else if (element is PasswordBox pb) pb.PasswordChanged += Element_TextInput;
            }
        }

        // --- Event handlers --- //
        private static void Element_GotFocus(object sender, RoutedEventArgs e)
        {
            SetShowErrors(sender as DependencyObject, false);
            // Если нужно, можно скрывать ошибку при фокусе
            SetErrorTextBlockVisibility(sender as UIElement, Visibility.Collapsed);
        }

        private static void Element_LostFocus(object sender, RoutedEventArgs e)
        {
            SetShowErrors(sender as DependencyObject, true);
            SetErrorTextBlockVisibility(sender as UIElement, Visibility.Visible);
        }
        private static void Element_TextInput(object sender, RoutedEventArgs e)
        {
            if (sender is DependencyObject d)
                SetTouched(d, true);
        }

        // --- Utility methods for manual elements management --- //
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
        /// <summary>
        /// Emulates Touched and ShowErrors to force display errors
        /// </summary>
        /// <param name="controls">Controls list</param>
        public static void ForceErrorsDisplay(List<Control> controls, bool IsSetTouched = true)
        {
            controls.ForEach(ctl => SetShowErrors(ctl, true));
            controls.ForEach(ctl => SetTouched(ctl, IsSetTouched));
        }
        /// <summary>
        /// Triggers user interface force update
        /// </summary>
        /// <param name="ctl">Input control</param>
        public static void ForceUpdateUI(Control ctl)
        {
            ctl.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Render);
            HR.Utilities.ValidationHelper.SetErrorTextBlockVisibility(ctl, Visibility.Visible);
        }
        public static void ForceValidate(FrameworkElement element, DependencyProperty property)
        {
            var binding = element.GetBindingExpression(property);
            binding?.UpdateSource();
        }
        /// <summary>
        /// Checks all controls for errors and returns validation state for each
        /// </summary>
        /// <param name="controls">Input controls</param>
        /// <returns>Dictionary of controls and their validation state: true for valid, false for error</returns>
        public static Dictionary<Control, bool> GetValidityState(List<Control> controls)
        {
            Dictionary<Control, bool> result = new Dictionary<Control, bool>();
            controls.ForEach(ctl => result.Add(ctl, !Validation.GetHasError(ctl)));
            return result;
        }
        /// <summary>
        /// Clears validation state and reset all invalid values
        /// </summary>
        /// <param name="ctl">Input control</param>
        /// <param name="prop">DepedencyProperty to reset</param>
        public static void ResetInvalid(Control ctl, DependencyProperty prop)
        {
            var binding = ctl.GetBindingExpression(prop);
            if (binding == null) return;
            Validation.ClearInvalid(binding);
            binding.UpdateSource();
        }
        public static void ResetTouched(params UIElement[] elements)
        {
            foreach (var el in elements)
                SetTouched(el, false);
        }
        public static void SetErrorTextBlockVisibility(UIElement element, Visibility visibility)
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
        public static void OpenPopup(UIElement element)
        {
            if (element == null) return;

            var adornerLayer = AdornerLayer.GetAdornerLayer(element);
            if (adornerLayer == null) return;

            var adorners = adornerLayer.GetAdorners(element);
            if (adorners == null) return;

            foreach (var adorner in adorners)
            {
                var popup = FindPopup(adorner);
                if (popup != null)
                    popup.IsOpen = true;
            }
        }
        private static Popup FindPopup(DependencyObject parent)
        {
            if (parent == null) return null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is Popup popup)
                    return popup;
                var result = FindPopup(child);
                if (result != null)
                    return result;
            }
            return null;
        }
        public static void ValidatePassword(PasswordBox passwordBox, string password, int minLength = 8)
        {
            if (passwordBox == null) throw new ArgumentNullException(nameof(passwordBox));

            var rule = new PasswordLengthValidationRule { MinLength = minLength };
            var result = rule.Validate(password, CultureInfo.CurrentCulture);

            var bindingExpression = passwordBox.GetBindingExpression(PasswordBox.TagProperty);
            if (bindingExpression == null) return;

            if (result.IsValid)
            {
                Validation.ClearInvalid(bindingExpression);
            }
            else
            {
                var validationError = new ValidationError(rule, bindingExpression)
                {
                    ErrorContent = result.ErrorContent
                };
                Validation.MarkInvalid(bindingExpression, validationError);
            }
        }
    }

}
