using System;
using System.Collections.Generic;
using System.Linq;
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

namespace HR.Controls
{
    /// <summary>
    /// Interaction logic for IconHighlightButton.xaml
    /// </summary>
    public partial class IconHighlightButton : UserControl
    {
        private Color? _baseColor;
        private Path _iconPath;

        public new object Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public new static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(nameof(Content), typeof(object), typeof(IconHighlightButton), new PropertyMetadata(null));

        public static readonly RoutedEvent ClickEvent =
            EventManager.RegisterRoutedEvent(nameof(Click), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(IconHighlightButton));

        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }

        public IconHighlightButton()
        {
            InitializeComponent();

            this.Loaded += IconHighlightButton_Loaded;

            this.MouseEnter += IconHighlightButton_MouseEnter;
            this.MouseLeave += IconHighlightButton_MouseLeave;
            this.MouseLeftButtonDown += IconHighlightButton_MouseLeftButtonDown;
            this.MouseLeftButtonUp += IconHighlightButton_MouseLeftButtonUp;

            this.LostMouseCapture += IconHighlightButton_LostMouseCapture;
        }

        private void IconHighlightButton_Loaded(object sender, RoutedEventArgs e)
        {
            if (ContentPresenter.Content is DependencyObject content)
            {
                _iconPath = FindFirstPath(content);
                if (_iconPath != null)
                {
                    if (_iconPath.Fill is SolidColorBrush scb)
                    {
                        _baseColor = scb.Color;
                    }
                    else
                    {
                        // Если не SolidColorBrush, возьмём черный как запасной вариант
                        _baseColor = Colors.Black;
                    }
                }
            }
            UpdateIconColor(_baseColor ?? Colors.Black);
        }

        private void IconHighlightButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (_baseColor.HasValue)
                UpdateIconColor(ChangeColorBrightness(_baseColor.Value, 0.2)); // +20%
        }

        private void IconHighlightButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_baseColor.HasValue)
                UpdateIconColor(_baseColor.Value);
        }

        private void IconHighlightButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_baseColor.HasValue)
                UpdateIconColor(ChangeColorBrightness(_baseColor.Value, 0.4)); // +40%
            this.CaptureMouse();
        }

        private void IconHighlightButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.IsMouseCaptured)
            {
                this.ReleaseMouseCapture();
                if (IsMouseOver)
                {
                    RaiseEvent(new RoutedEventArgs(ClickEvent));
                    UpdateIconColor(ChangeColorBrightness(_baseColor.Value, 0.2)); // Наведение
                }
                else
                {
                    UpdateIconColor(_baseColor.Value);
                }
            }
        }

        private void IconHighlightButton_LostMouseCapture(object sender, MouseEventArgs e)
        {
            UpdateIconColor(IsMouseOver && _baseColor.HasValue ? ChangeColorBrightness(_baseColor.Value, 0.2) : _baseColor ?? Colors.Black);
        }

        private void UpdateIconColor(Color color)
        {
            if (_iconPath != null)
            {
                _iconPath.Fill = new SolidColorBrush(color);
            }
        }

        /// <summary>
        /// Изменяет яркость цвета на заданный коэффициент (от -1 до +1)
        /// +0.2 = светлее на 20%, -0.2 = темнее на 20%
        /// </summary>
        /// <param name="color">Исходный цвет</param>
        /// <param name="correctionFactor">Коррекция яркости</param>
        /// <returns>Новый цвет</returns>
        private Color ChangeColorBrightness(Color color, double correctionFactor)
        {
            double red = color.R;
            double green = color.G;
            double blue = color.B;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = red + (255 - red) * correctionFactor;
                green = green + (255 - green) * correctionFactor;
                blue = blue + (255 - blue) * correctionFactor;
            }

            return Color.FromArgb(color.A, (byte)red, (byte)green, (byte)blue);
        }

        private Path FindFirstPath(DependencyObject parent)
        {
            if (parent == null)
                return null;

            if (parent is Path path)
                return path;

            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                var result = FindFirstPath(child);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
