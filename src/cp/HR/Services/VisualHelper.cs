using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace HR.Services
{
    /// <summary>
    /// Provides utility methods for navigating and interacting with the visual tree of UI elements
    /// </summary>
    public class VisualHelper
    {
        /// <summary>
        /// Recursively finds all visual child elements of the specified type <typeparamref name="T"/> within a given <see cref="DependencyObject"/>.
        /// </summary>
        /// <typeparam name="T">The type of visual children to find. Must be a <see cref="DependencyObject"/>.</typeparam>
        /// <param name="depObj">The parent <see cref="DependencyObject"/> whose visual tree will be searched.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> containing all visual children of type <typeparamref name="T"/> found within the visual tree of <paramref name="depObj"/>.
        /// If <paramref name="depObj"/> is null or has no children of the specified type, the returned sequence will be empty.
        /// </returns>
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T t)
                    {
                        yield return t;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
        /// <summary>
        /// Retrieves a specific <see cref="DataGridCell"/> from a <see cref="DataGridRow"/>
        /// at the given column index.
        /// </summary>
        /// <param name="grid">The <see cref="DataGrid"/> that contains the row and cell.</param>
        /// <param name="row">The <see cref="DataGridRow"/> from which to retrieve the cell.</param>
        /// <param name="columnIndex">The zero-based index of the column for which to retrieve the cell.</param>
        /// <returns>
        /// The <see cref="DataGridCell"/> at the specified column index within the given row,
        /// or <see langword="null"/> if the row is null, the cell cannot be found,
        /// or if the row/cell presenter is not yet generated.
        /// </returns>
        /// <remarks>
        /// This method attempts to find the <see cref="DataGridCellsPresenter"/> within the row.
        /// If the presenter is not immediately available (e.g., due to UI virtualization),
        /// it will try to scroll the row into view to force its generation and then reattempt to find the cell.
        /// </remarks>
        public static DataGridCell GetCell(DataGrid grid, DataGridRow row, int columnIndex)
        {
            if (row == null) return null;

            var presenter = FindVisualChild<DataGridCellsPresenter>(row);
            if (presenter == null)
            {
                grid.ScrollIntoView(row, grid.Columns[columnIndex]);
                presenter = FindVisualChild<DataGridCellsPresenter>(row);
            }

            return presenter?.ItemContainerGenerator.ContainerFromIndex(columnIndex) as DataGridCell;
        }
        /// <summary>
        /// Recursively searches the visual tree of a <see cref="DependencyObject"/>
        /// to find the first child of a specified type.
        /// </summary>
        /// <typeparam name="T">The type of the visual child to find (must inherit from <see cref="DependencyObject"/>).</typeparam>
        /// <param name="obj">The parent <see cref="DependencyObject"/> to start the search from.</param>
        /// <returns>
        /// The first visual child of type <typeparamref name="T"/> found in the visual tree,
        /// or <see langword="null"/> if no child of the specified type is found.
        /// </returns>
        public static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T t)
                    return t;
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }
    }
}
