using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace HR.Controls
{
    /// <summary>
    /// A custom <see cref="WrapPanel"/> that stretches child elements horizontally to fill the available width.
    /// </summary>
    public class StretchingWrapPanel : WrapPanel
    {
        /// <summary>
        /// Measures the size required for child elements with an unlimited available size,
        /// then calls the base <see cref="WrapPanel.MeasureOverride"/> with the given constraint.
        /// </summary>
        /// <param name="constraint">The available size that this panel can give to child elements.</param>
        /// <returns>The size required by the panel and its children.</returns>
        protected override Size MeasureOverride(Size constraint)
        {
            foreach (UIElement child in InternalChildren)
            {
                // Measure each child with infinite available size to allow them to determine their desired size freely.
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            }
            return base.MeasureOverride(constraint);
        }
        /// <summary>
        /// Arranges child elements in horizontal lines, stretching their widths to fill the panel's width.
        /// Wraps to a new line when the current line is full.
        /// </summary>
        /// <param name="finalSize">The final area within the parent that this panel should use to arrange itself and its children.</param>
        /// <returns>The actual size used by the panel.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            double x = 0, y = 0;
            double lineHeight = 0;
            List<UIElement> lineElements = new List<UIElement>();

            foreach (UIElement child in InternalChildren)
            {
                double childWidth = child.DesiredSize.Width;
                double childHeight = child.DesiredSize.Height;
                // If adding the next child exceeds the line width, arrange the current line and start a new one.
                if (x + childWidth > finalSize.Width && lineElements.Count > 0)
                {
                    ArrangeLine(lineElements, y, lineHeight, finalSize.Width);
                    y += lineHeight;
                    x = 0;
                    lineHeight = 0;
                    lineElements.Clear();
                }

                lineElements.Add(child);
                x += childWidth;
                lineHeight = Math.Max(lineHeight, childHeight);
            }
            // Arrange any remaining elements in the last line.
            if (lineElements.Count > 0)
                ArrangeLine(lineElements, y, lineHeight, finalSize.Width);

            return finalSize;
        }
        /// <summary>
        /// Arranges a single line of child elements by distributing the total width among them,
        /// respecting each element's <see cref="FrameworkElement.MaxWidth"/> if set.
        /// </summary>
        /// <param name="elements">The list of child elements in the line.</param>
        /// <param name="y">The vertical offset where the line should be arranged.</param>
        /// <param name="height">The height of the line.</param>
        /// <param name="totalWidth">The total available width for the line.</param>
        private void ArrangeLine(List<UIElement> elements, double y, double height, double totalWidth)
        {
            int count = elements.Count;

            // Get the MaxWidth of each element, or positive infinity if not set.
            double[] maxWidths = elements
                .Select(e => (e as FrameworkElement)?.MaxWidth ?? double.PositiveInfinity)
                .ToArray();

            // Начинаем с равномерного распределения ширины
            double availableWidth = totalWidth;
            double[] widths = new double[count];
            bool[] fixedWidth = new bool[count]; // отмечаем элементы, у которых достигнут maxWidth

            int flexibleCount = count;

            // Initially distribute available width evenly among all elements.
            for (int i = 0; i < count; i++)
                widths[i] = availableWidth / flexibleCount;

            bool changed;
            // Iteratively adjust widths to respect MaxWidth constraints.
            do
            {
                changed = false;
                double totalFixedWidth = 0;
                flexibleCount = 0;

                for (int i = 0; i < count; i++)
                {
                    if (fixedWidth[i])
                    {
                        totalFixedWidth += widths[i];
                    }
                    else
                    {
                        if (widths[i] > maxWidths[i])
                        {
                            widths[i] = maxWidths[i];
                            fixedWidth[i] = true;
                            changed = true;
                            totalFixedWidth += widths[i];
                        }
                        else
                        {
                            flexibleCount++;
                        }
                    }
                }

                if (changed && flexibleCount > 0)
                {
                    // Redistribute remaining width among flexible elements.
                    double remainingWidth = totalWidth - totalFixedWidth;
                    double newWidth = remainingWidth / flexibleCount;
                    for (int i = 0; i < count; i++)
                    {
                        if (!fixedWidth[i])
                            widths[i] = newWidth;
                    }
                }
            } while (changed && flexibleCount > 0);

            // Arrange elements horizontally with the calculated widths and the specified height.
            double x = 0;
            for (int i = 0; i < count; i++)
            {
                var child = elements[i];
                double w = widths[i];
                double h = height;

                child.Arrange(new Rect(x, y, w, h));
                x += w;
            }
        }
    }

}
