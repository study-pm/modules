using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Documents;

namespace HR.Services
{
    /// <summary>
    /// Provides helper methods for printing data from a collection view.
    /// </summary>
    public class PrintHelper
    {
        /// <summary>
        /// Prints the contents of the specified <see cref="ICollectionView"/> as a formatted table.
        /// </summary>
        /// <param name="collectionView">The collection view containing the data to print. Must not be null or empty.</param>
        /// <param name="printDescription">An optional description used as the print job name and document title. Default is "Печать данных".</param>
        /// <param name="propertiesToPrint">
        /// An optional list of property names to include in the printout.
        /// If null or empty, all public instance properties of the item type will be printed.
        /// </param>
        /// <param name="customHeaders">
        /// An optional dictionary mapping property names to custom header strings for the table columns.
        /// If not specified, property names are used as headers.
        /// </param>
        /// <param name="converters">
        /// An optional dictionary mapping property names to functions that convert property values to strings for display.
        /// If not specified, the default <c>ToString()</c> conversion is used.
        /// </param>
        /// <param name="columnWidths">
        /// An optional dictionary mapping property names to column widths in device-independent pixels.
        /// Columns without specified widths share the remaining available width equally.
        /// </param>
        /// <param name="addRowNumbers">If true, a row number column is added as the first column.</param>
        /// <returns><c>true</c> if the print job was initiated; <c>false</c> if the collection was empty or the print dialog was cancelled.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="collectionView"/> is null.</exception>
        public static bool PrintCollectionView(
            ICollectionView collectionView,
            string printDescription = "Печать данных",
            IEnumerable<string> propertiesToPrint = null,
            Dictionary<string, string> customHeaders = null,
            Dictionary<string, Func<object, string>> converters = null,
            Dictionary<string, double> columnWidths = null,
            bool addRowNumbers = false
            )
        {
            if (collectionView == null)
                throw new ArgumentNullException(nameof(collectionView));

            var enumerator = collectionView.GetEnumerator();
            if (!enumerator.MoveNext())
                return false;

            var itemType = enumerator.Current.GetType();
            PropertyInfo[] allProperties = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var propsToPrint = (propertiesToPrint != null && propertiesToPrint.Any())
                ? allProperties.Where(p => propertiesToPrint.Contains(p.Name)).ToArray()
                : allProperties;

            customHeaders = customHeaders ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            converters = converters ?? new Dictionary<string, Func<object, string>>(StringComparer.OrdinalIgnoreCase);
            columnWidths = columnWidths ?? new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() != true)
                return false;

            // Determine page size (simplified)
            double pageWidth = printDialog.PrintableAreaWidth;
            double pageHeight = printDialog.PrintableAreaHeight;

            // Create FlowDocument for printing
            FlowDocument flowDoc = new FlowDocument
            {
                PagePadding = new Thickness(25),
                ColumnWidth = pageWidth,
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 12,
                PageHeight = pageHeight,
                PageWidth = pageWidth
            };

            // Document title
            Paragraph title = new Paragraph(new Run(printDescription))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            flowDoc.Blocks.Add(title);

            // Create table
            Table table = new Table
            {
                CellSpacing = 0,
                BorderThickness = new Thickness(0.5),
                BorderBrush = Brushes.DarkGray
            };

            int totalColumns = propsToPrint.Length + (addRowNumbers ? 1 : 0);

            // Calculate column widths
            int unspecifiedCount = 0;
            double specifiedWidthSum = 0;
            double rowNumberColumnWidth = addRowNumbers ? 40 : 0;

            // Calculate specified widths and unspecified quantity
            for (int i = 0; i < propsToPrint.Length; i++)
            {
                string propName = propsToPrint[i].Name;
                if (columnWidths.TryGetValue(propName, out double w))
                    specifiedWidthSum += w;
                else
                    unspecifiedCount++;
            }

            // Calculate available width for unset width columns (reserving a little extra space)
            const double safetyMargin = 5; // пикселей
            double availableWidth = pageWidth - flowDoc.PagePadding.Left - flowDoc.PagePadding.Right - rowNumberColumnWidth - specifiedWidthSum - safetyMargin;
            if (availableWidth < 0)
                availableWidth = 0;

            // Width for each unset column
            double unspecifiedColumnWidth = unspecifiedCount > 0 ? availableWidth / unspecifiedCount : 0;

            // Create columns regarding provided widths
            for (int i = 0; i < totalColumns; i++)
            {
                if (addRowNumbers && i == 0)
                {
                    // Rows numbering column — fixed width
                    table.Columns.Add(new TableColumn { Width = new GridLength(rowNumberColumnWidth) });
                }
                else
                {
                    string propName = propsToPrint[addRowNumbers ? i - 1 : i].Name;

                    if (columnWidths.TryGetValue(propName, out double w))
                    {
                        table.Columns.Add(new TableColumn { Width = new GridLength(w) });
                    }
                    else
                    {
                        table.Columns.Add(new TableColumn { Width = new GridLength(unspecifiedColumnWidth) });
                    }
                }
            }

            // Add header row
            TableRowGroup headerGroup = new TableRowGroup();
            TableRow headerRow = new TableRow();
            headerGroup.Rows.Add(headerRow);
            table.RowGroups.Add(headerGroup);

            if (addRowNumbers)
            {
                headerRow.Cells.Add(CreateHeaderCell("№"));
            }

            foreach (var prop in propsToPrint)
            {
                string header = customHeaders.TryGetValue(prop.Name, out var customHeader) ? customHeader : prop.Name;
                headerRow.Cells.Add(CreateHeaderCell(header));
            }

            // Add data rows
            TableRowGroup bodyGroup = new TableRowGroup();
            table.RowGroups.Add(bodyGroup);

            enumerator.Reset();
            int rowNumber = 1;
            while (enumerator.MoveNext())
            {
                var item = enumerator.Current;
                TableRow row = new TableRow();
                bodyGroup.Rows.Add(row);

                if (addRowNumbers)
                {
                    var cell = CreateBodyCell(rowNumber.ToString());
                    cell.TextAlignment = TextAlignment.Center;  // Center cell content
                    row.Cells.Add(cell);
                }

                foreach (var prop in propsToPrint)
                {
                    object value = prop.GetValue(item);
                    string text;
                    if (converters.TryGetValue(prop.Name, out var conv))
                        text = conv(value);
                    else
                        text = value?.ToString() ?? "";

                    row.Cells.Add(CreateBodyCell(text));
                }

                rowNumber++;
            }


            flowDoc.Blocks.Add(table);

            // Print the document
            IDocumentPaginatorSource idpSource = flowDoc;
            printDialog.PrintDocument(idpSource.DocumentPaginator, printDescription);

            return true;
        }

        private static TableCell CreateHeaderCell(string text)
        {
            return new TableCell(new Paragraph(new Run(text)))
            {
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(5),
                BorderThickness = new Thickness(0.5),
                BorderBrush = Brushes.Black,
                TextAlignment = TextAlignment.Center,
                Background = Brushes.LightGray
            };
        }

        private static TableCell CreateBodyCell(string text)
        {
            Paragraph p = new Paragraph(new Run(text))
            {
                TextAlignment = TextAlignment.Left,
                Margin = new Thickness(0),
                Padding = new Thickness(5)
            };

            return new TableCell(p)
            {
                BorderThickness = new Thickness(0.5),
                BorderBrush = Brushes.Black
            };
        }
    }
}
