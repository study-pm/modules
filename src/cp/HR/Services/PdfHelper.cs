using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;
using System.IO.Packaging;
using System.IO;
using System.Windows.Controls;
using System.Xml.Linq;

namespace HR.Services
{
    public static class PdfHelper
    {
        /// <summary>
        /// Converts the given <see cref="ICollectionView"/> into a <see cref="FlowDocument"/> containing a table representation of the collection's items.
        /// </summary>
        /// <param name="collectionView">The collection view to convert. It should contain items of the same type.</param>
        /// <returns>
        /// A <see cref="FlowDocument"/> with a table where each column corresponds to a property of the item type,
        /// and each row corresponds to an item in the collection. If the collection is empty or the current item is null, returns an empty document.
        /// </returns>
        public static FlowDocument ConvertCollectionViewToFlowDocument(ICollectionView collectionView)
        {
            var flowDoc = new FlowDocument();

            var table = new Table();
            flowDoc.Blocks.Add(table);

            // Get the type of the collection items
            var itemType = collectionView.CurrentItem?.GetType();
            if (itemType == null)
                return flowDoc; // empty document

            // Get properties for columns
            PropertyInfo[] properties = itemType.GetProperties();

            // Create table columns
            for (int i = 0; i < properties.Length; i++)
            {
                table.Columns.Add(new TableColumn());
            }

            // Add header (TableRowGroup + TableRow)
            var headerGroup = new TableRowGroup();
            table.RowGroups.Add(headerGroup);
            var headerRow = new TableRow();
            headerGroup.Rows.Add(headerRow);

            foreach (var prop in properties)
            {
                var headerCell = new TableCell(new System.Windows.Documents.Paragraph(new Run(prop.Name)))
                {
                    FontWeight = FontWeights.Bold,
                    Background = Brushes.LightGray,
                    Padding = new System.Windows.Thickness(4)
                };
                headerRow.Cells.Add(headerCell);
            }

            // Add data rows
            var dataGroup = new TableRowGroup();
            table.RowGroups.Add(dataGroup);

            foreach (var item in collectionView)
            {
                var dataRow = new TableRow();
                dataGroup.Rows.Add(dataRow);

                foreach (var prop in properties)
                {
                    object value = prop.GetValue(item);
                    string text = value?.ToString() ?? "";

                    var cell = new TableCell(new System.Windows.Documents.Paragraph(new Run(text)))
                    {
                        Padding = new System.Windows.Thickness(4)
                    };
                    dataRow.Cells.Add(cell);
                }
            }
            return flowDoc;
        }
        /// <summary>
        /// Exports the specified <see cref="FlowDocument"/> to a PDF file at the given file path.
        /// </summary>
        /// <param name="doc">The <see cref="FlowDocument"/> to export.</param>
        /// <param name="pdfFilePath">The full file path where the PDF file will be saved.</param>
        /// <remarks>
        /// This method converts the FlowDocument to an XPS document in memory, then uses PdfSharp's XPS converter
        /// to generate the PDF file. It requires the PdfSharp.Xps library.
        /// </remarks>
        public static void ExportFlowDocumentToPdf(FlowDocument doc, string pdfFilePath)
        {
            using (var ms = new MemoryStream())
            using (Package package = Package.Open(ms, FileMode.Create, FileAccess.ReadWrite))
            {
                var xpsDoc = new System.Windows.Xps.Packaging.XpsDocument(package);
                var xpsWriter = System.Windows.Xps.Packaging.XpsDocument.CreateXpsDocumentWriter(xpsDoc);
                var paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
                xpsWriter.Write(paginator);
                xpsDoc.Close();

                ms.Position = 0;

                var pdfXpsDoc = PdfSharp.Xps.XpsModel.XpsDocument.Open(ms);
                PdfSharp.Xps.XpsConverter.Convert(pdfXpsDoc, pdfFilePath, 0);
            }
        }
        /// <summary>
        /// Creates a <see cref="Document"/> representing the data from the specified <see cref="ICollectionView"/>.
        /// </summary>
        /// <param name="collectionView">The collection view containing the data to include in the document.</param>
        /// <returns>
        /// A <see cref="Document"/> object with a table that includes all items from the collection view.
        /// The table columns correspond to the properties of the collection's item type.
        /// If the collection is empty or the current item is null, returns an empty document with default page setup.
        /// </returns>
        /// <remarks>
        /// The document uses A4 page format and automatically calculates column widths based on the number of properties.
        /// The first row of the table contains property names as headers with a light gray background.
        /// </remarks>
        public static Document CreateDocumentFromCollectionView(ICollectionView collectionView)
        {
            var document = new Document();
            var section = document.AddSection();

            // Clone page setup to have access to margins and width
            section.PageSetup = document.DefaultPageSetup.Clone();
            section.PageSetup.PageFormat = PageFormat.A4;

            var itemType = collectionView.CurrentItem?.GetType();
            if (itemType == null)
                return document; // empty document

            PropertyInfo[] properties = itemType.GetProperties();

            // Add title paragraph
            section.AddParagraph("Отчет по данным").Format.Font.Size = 16;

            // Create table
            var table = section.AddTable();
            table.Borders.Width = 0.5;

            // Calculate usable page width (excluding margins)
            var pageWidth = document.DefaultPageSetup.PageWidth;
            var leftMargin = document.DefaultPageSetup.LeftMargin;
            var rightMargin = document.DefaultPageSetup.RightMargin;

            var usableWidth = pageWidth - leftMargin - rightMargin;

            // Calculate column width based on number of properties
            Unit colWidth = usableWidth / properties.Length;

            // Add columns with calculated width
            foreach (var prop in properties)
            {
                table.AddColumn(colWidth);
            }

            // Add header row with property names
            var headerRow = table.AddRow();
            headerRow.Shading.Color = MigraDoc.DocumentObjectModel.Colors.LightGray;
            for (int i = 0; i < properties.Length; i++)
            {
                headerRow.Cells[i].AddParagraph(properties[i].Name);
                headerRow.Cells[i].Format.Font.Bold = true;
                headerRow.Cells[i].Format.Alignment = ParagraphAlignment.Center;
                headerRow.Cells[i].VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;
            }

            // Add data rows from collection
            foreach (var item in collectionView)
            {
                var row = table.AddRow();
                for (int i = 0; i < properties.Length; i++)
                {
                    var value = properties[i].GetValue(item)?.ToString() ?? "";
                    row.Cells[i].AddParagraph(value);
                    row.Cells[i].Format.Alignment = ParagraphAlignment.Left;
                    row.Cells[i].VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;
                }
            }

            return document;
        }
        /// <summary>
        /// Creates a <see cref="Document"/> from the specified <see cref="ICollectionView"/>, optionally customizing the output with various parameters.
        /// </summary>
        /// <param name="collectionView">The collection view containing the data to include in the document. Must not be null or empty.</param>
        /// <param name="documentTitle">An optional title to display at the top of the document.</param>
        /// <param name="skipProperties">An optional collection of property names to exclude from the document columns.</param>
        /// <param name="customHeaders">An optional dictionary mapping property names to custom header text for the table columns.</param>
        /// <param name="customConverters">An optional dictionary mapping property names to functions that convert property values to strings for display.</param>
        /// <param name="columnWidths">An optional dictionary mapping property names to column widths in centimeters. Columns without specified widths share remaining space equally.</param>
        /// <param name="addRowNumbers">If set to <c>true</c>, adds a row number column at the beginning of the table.</param>
        /// <param name="isLandscape">If set to <c>true</c>, formats the page in landscape orientation with A4 size.</param>
        /// <returns>
        /// A <see cref="Document"/> object representing the data in a formatted table with the specified customizations.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="collectionView"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if <paramref name="collectionView"/> is empty.</exception>
        /// <remarks>
        /// The method clones the default page setup and adjusts margins and orientation.
        /// It calculates column widths based on specified widths and remaining space.
        /// Table headers and cells are formatted with basic styling, including bold headers and alternating shading.
        /// Custom converters allow formatting of property values, and custom headers allow renaming column titles.
        /// </remarks>
        public static Document CreateDocumentFromCollectionView(
            ICollectionView collectionView,
            string documentTitle = null,
            IEnumerable<string> skipProperties = null,
            Dictionary<string, string> customHeaders = null,
            Dictionary<string, Func<object, string>> customConverters = null,
            Dictionary<string, double> columnWidths = null,
            bool addRowNumbers = false,
            bool isLandscape = false
            )
        {
            if (collectionView == null)
                throw new ArgumentNullException(nameof(collectionView));

            var enumerator = collectionView.GetEnumerator();
            if (!enumerator.MoveNext())
                throw new InvalidOperationException("CollectionView is empty.");

            var itemType = enumerator.Current.GetType();

            var skipSet = new HashSet<string>(skipProperties ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            customHeaders = customHeaders ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            customConverters = customConverters ?? new Dictionary<string, Func<object, string>>(StringComparer.OrdinalIgnoreCase);
            columnWidths = columnWidths ?? new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

            var properties = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                     .Where(p => !skipSet.Contains(p.Name))
                                     .ToArray();

            var document = new Document();
            var section = document.AddSection();

            // Clone page setup to access margins and size to get access to page margins and size
            section.PageSetup = document.DefaultPageSetup.Clone();

            if (isLandscape)
            {
                // Set page size for landscape orientation
                section.PageSetup.PageWidth = Unit.FromCentimeter(29.7); // ширина A4 в альбомной ориентации
                section.PageSetup.PageHeight = Unit.FromCentimeter(21.0); // высота A4 в альбомной ориентации
            }

            // Set margins to 1 cm
            section.PageSetup.LeftMargin = Unit.FromCentimeter(1);
            section.PageSetup.RightMargin = Unit.FromCentimeter(1);
            section.PageSetup.TopMargin = Unit.FromCentimeter(1);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(1);

            // Add document title if provided
            if (!string.IsNullOrEmpty(documentTitle))
            {
                var titleParagraph = section.AddParagraph(documentTitle);
                titleParagraph.Format.Font.Size = 16;
                titleParagraph.Format.Font.Bold = true;
                titleParagraph.Format.SpaceAfter = Unit.FromCentimeter(0.5);
                titleParagraph.Format.Alignment = ParagraphAlignment.Center;
            }

            var table = section.AddTable();
            table.Borders.Width = 0.5;
            table.Format.SpaceBefore = Unit.FromCentimeter(0.1);
            table.Format.SpaceAfter = Unit.FromCentimeter(0.1);

            // Available width (page with excluding margins width)
            Unit pageWidth = section.PageSetup.PageWidth;
            Unit leftMargin = section.PageSetup.LeftMargin;
            Unit rightMargin = section.PageSetup.RightMargin;
            Unit usableWidth = pageWidth - leftMargin - rightMargin;

            // Calculate column widths
            Unit totalSpecifiedWidth = Unit.FromCentimeter(0);
            int unspecifiedCount = 0;
            foreach (var prop in properties)
            {
                if (columnWidths.TryGetValue(prop.Name, out double widthCm))
                    totalSpecifiedWidth += Unit.FromCentimeter(widthCm);
                else
                    unspecifiedCount++;
            }

            // In there is a numeration column, reduce available width
            if (addRowNumbers)
                usableWidth -= Unit.FromCentimeter(0.5);

            // Distribute remaining space equally between unset width columns
            Unit remainingWidth = usableWidth - totalSpecifiedWidth;
            if (remainingWidth < 0)
                remainingWidth = Unit.FromCentimeter(0); // negative width protection
            Unit defaultColWidth = unspecifiedCount > 0 ? remainingWidth / unspecifiedCount : Unit.FromCentimeter(0);

            // Minimum column width
            Unit minColWidth = Unit.FromCentimeter(1);

            // Add column for auto numeration
            if (addRowNumbers)
            {
                table.AddColumn(Unit.FromCentimeter(0.75)); // auto number column width
            }
            // Add columns setting their widths
            foreach (var prop in properties)
            {
                Unit colWidth;
                if (columnWidths.TryGetValue(prop.Name, out double widthCm))
                    colWidth = Unit.FromCentimeter(widthCm);
                else
                    colWidth = defaultColWidth;

                // Ensure minimum width
                if (colWidth < minColWidth)
                    colWidth = minColWidth;

                table.AddColumn(colWidth);
            }

            // Table header
            var headerRow = table.AddRow();
            headerRow.Shading.Color = MigraDoc.DocumentObjectModel.Colors.LightGray;
            headerRow.HeadingFormat = true;
            headerRow.Format.Font.Bold = true;
            headerRow.Format.Alignment = ParagraphAlignment.Center;
            headerRow.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;

            int cellIndex = 0;

            if (addRowNumbers)
            {
                headerRow.Cells[cellIndex].AddParagraph("№");
                headerRow.Cells[cellIndex].Format.Alignment = ParagraphAlignment.Center;
                headerRow.Cells[cellIndex].VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;
                cellIndex++;
            }

            for (int i = 0; i < properties.Length; i++, cellIndex++)
            {
                var prop = properties[i];
                string headerText = customHeaders.TryGetValue(prop.Name, out var customHeader) ? customHeader : prop.Name;
                headerRow.Cells[cellIndex].AddParagraph(headerText);
                headerRow.Cells[cellIndex].Format.Alignment = ParagraphAlignment.Center;
                headerRow.Cells[cellIndex].VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;
            }

            // Add data
            enumerator.Reset();
            if (!enumerator.MoveNext())
                return document;

            int rowNumber = 1;
            do
            {
                var row = table.AddRow();
                row.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;

                cellIndex = 0;
                if (addRowNumbers)
                {
                    row.Cells[cellIndex].AddParagraph(rowNumber.ToString());
                    row.Cells[cellIndex].Format.Alignment = ParagraphAlignment.Center;
                    row.Cells[cellIndex].VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Top;
                    cellIndex++;
                }
                for (int i = 0; i < properties.Length; i++, cellIndex++)
                {
                    var prop = properties[i];
                    object rawValue = prop.GetValue(enumerator.Current);
                    string text = customConverters.TryGetValue(prop.Name, out var conv) ? conv(rawValue) : (rawValue?.ToString() ?? "");

                    // Add text to cell
                    var paragraph = row.Cells[cellIndex].AddParagraph(text);
                    paragraph.Format.Font.Size = 10;

                    row.Cells[cellIndex].Format.Alignment = ParagraphAlignment.Left;
                    row.Cells[cellIndex].VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Top;
                }
                rowNumber++;
            } while (enumerator.MoveNext());

            return document;
        }
        /// <summary>
        /// Creates a <see cref="Document"/> from the specified <see cref="ICollectionView"/>, generating a table representation of the collection's items.
        /// </summary>
        /// <param name="collectionView">The collection view containing the data to be rendered into the document.</param>
        /// <param name="documentTitle">An optional title to display at the top of the document.</param>
        /// <param name="skipProperties">An optional collection of property names to exclude from the table columns.</param>
        /// <param name="customHeaders">An optional dictionary mapping property names to custom column header titles.</param>
        /// <param name="customConverters">An optional dictionary mapping property names to custom converter functions for cell text formatting.
        /// The converter function takes the raw property value and the current item, and returns a string representation.</param>
        /// <param name="columnWidths">An optional dictionary specifying column widths in centimeters for given property names.
        /// Columns without specified widths will share remaining space equally.</param>
        /// <param name="addRowNumbers">If true, adds a leading column with row numbers.</param>
        /// <param name="isLandscape">If true, sets the page orientation to landscape (A4 size).</param>
        /// <returns>A <see cref="Document"/> object representing the collection data formatted as a table.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="collectionView"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the collection view is empty.</exception>
        public static Document CreateDocumentFromCollectionView(
            ICollectionView collectionView,
            string documentTitle = null,
            IEnumerable<string> skipProperties = null,
            Dictionary<string, string> customHeaders = null,
            Dictionary<string, Func<object, object, string>> customConverters = null,
            Dictionary<string, double> columnWidths = null,
            bool addRowNumbers = false,
            bool isLandscape = false
            )
        {
            if (collectionView == null)
                throw new ArgumentNullException(nameof(collectionView));

            var enumerator = collectionView.GetEnumerator();
            if (!enumerator.MoveNext())
                throw new InvalidOperationException("CollectionView is empty.");

            var itemType = enumerator.Current.GetType();

            var skipSet = new HashSet<string>(skipProperties ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            customHeaders = customHeaders ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            customConverters = customConverters ?? new Dictionary<string, Func<object, object, string>>(StringComparer.OrdinalIgnoreCase);
            columnWidths = columnWidths ?? new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

            var properties = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                     .Where(p => !skipSet.Contains(p.Name))
                                     .ToArray();

            var document = new Document();
            var section = document.AddSection();

            // Clone page setup to access margins and size to get access to page margins and size
            section.PageSetup = document.DefaultPageSetup.Clone();

            if (isLandscape)
            {
                // Set page size for landscape orientation
                section.PageSetup.PageWidth = Unit.FromCentimeter(29.7); // ширина A4 в альбомной ориентации
                section.PageSetup.PageHeight = Unit.FromCentimeter(21.0); // высота A4 в альбомной ориентации
            }

            // Set margins to 1 cm
            section.PageSetup.LeftMargin = Unit.FromCentimeter(1);
            section.PageSetup.RightMargin = Unit.FromCentimeter(1);
            section.PageSetup.TopMargin = Unit.FromCentimeter(1);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(1);

            // Add document title if provided
            if (!string.IsNullOrEmpty(documentTitle))
            {
                var titleParagraph = section.AddParagraph(documentTitle);
                titleParagraph.Format.Font.Size = 16;
                titleParagraph.Format.Font.Bold = true;
                titleParagraph.Format.SpaceAfter = Unit.FromCentimeter(0.5);
                titleParagraph.Format.Alignment = ParagraphAlignment.Center;
            }

            var table = section.AddTable();
            table.Borders.Width = 0.5;
            table.Format.SpaceBefore = Unit.FromCentimeter(0.1);
            table.Format.SpaceAfter = Unit.FromCentimeter(0.1);

            // Available width (page with excluding margins width)
            Unit pageWidth = section.PageSetup.PageWidth;
            Unit leftMargin = section.PageSetup.LeftMargin;
            Unit rightMargin = section.PageSetup.RightMargin;
            Unit usableWidth = pageWidth - leftMargin - rightMargin;

            // Calculate column widths
            Unit totalSpecifiedWidth = Unit.FromCentimeter(0);
            int unspecifiedCount = 0;
            foreach (var prop in properties)
            {
                if (columnWidths.TryGetValue(prop.Name, out double widthCm))
                    totalSpecifiedWidth += Unit.FromCentimeter(widthCm);
                else
                    unspecifiedCount++;
            }

            // In there is a numeration column, reduce available width
            if (addRowNumbers)
                usableWidth -= Unit.FromCentimeter(0.5);

            // Distribute remaining space equally between unset width columns
            Unit remainingWidth = usableWidth - totalSpecifiedWidth;
            if (remainingWidth < 0)
                remainingWidth = Unit.FromCentimeter(0); // negative width protection
            Unit defaultColWidth = unspecifiedCount > 0 ? remainingWidth / unspecifiedCount : Unit.FromCentimeter(0);

            // Minimum column width
            Unit minColWidth = Unit.FromCentimeter(1);

            // Add column for auto numeration
            if (addRowNumbers)
            {
                table.AddColumn(Unit.FromCentimeter(0.75)); // auto number column width
            }
            // Add columns setting their widths
            foreach (var prop in properties)
            {
                Unit colWidth;
                if (columnWidths.TryGetValue(prop.Name, out double widthCm))
                    colWidth = Unit.FromCentimeter(widthCm);
                else
                    colWidth = defaultColWidth;

                // Ensure minimum width
                if (colWidth < minColWidth)
                    colWidth = minColWidth;

                table.AddColumn(colWidth);
            }

            // Table header
            var headerRow = table.AddRow();
            headerRow.Shading.Color = MigraDoc.DocumentObjectModel.Colors.LightGray;
            headerRow.HeadingFormat = true;
            headerRow.Format.Font.Bold = true;
            headerRow.Format.Alignment = ParagraphAlignment.Center;
            headerRow.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;

            int cellIndex = 0;

            if (addRowNumbers)
            {
                headerRow.Cells[cellIndex].AddParagraph("№");
                headerRow.Cells[cellIndex].Format.Alignment = ParagraphAlignment.Center;
                headerRow.Cells[cellIndex].VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;
                cellIndex++;
            }

            for (int i = 0; i < properties.Length; i++, cellIndex++)
            {
                var prop = properties[i];
                string headerText = customHeaders.TryGetValue(prop.Name, out var customHeader) ? customHeader : prop.Name;
                headerRow.Cells[cellIndex].AddParagraph(headerText);
                headerRow.Cells[cellIndex].Format.Alignment = ParagraphAlignment.Center;
                headerRow.Cells[cellIndex].VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;
            }

            // Add data
            enumerator.Reset();
            if (!enumerator.MoveNext())
                return document;

            int rowNumber = 1;
            do
            {
                var row = table.AddRow();
                row.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;

                cellIndex = 0;
                if (addRowNumbers)
                {
                    row.Cells[cellIndex].AddParagraph(rowNumber.ToString());
                    row.Cells[cellIndex].Format.Alignment = ParagraphAlignment.Center;
                    row.Cells[cellIndex].VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Top;
                    cellIndex++;
                }
                for (int i = 0; i < properties.Length; i++, cellIndex++)
                {
                    var prop = properties[i];
                    object rawValue = prop.GetValue(enumerator.Current);
                    string text = customConverters.TryGetValue(prop.Name, out var conv) ? conv(rawValue, enumerator.Current) : (rawValue?.ToString() ?? "");

                    // Add text to cell
                    var paragraph = row.Cells[cellIndex].AddParagraph(text);
                    paragraph.Format.Font.Size = 10;

                    row.Cells[cellIndex].Format.Alignment = ParagraphAlignment.Left;
                    row.Cells[cellIndex].VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Top;
                }
                rowNumber++;
            } while (enumerator.MoveNext());

            return document;
        }
        /// <summary>
        /// Saves the specified <see cref="Document"/> as a PDF file to the given file path.
        /// </summary>
        /// <param name="document">The <see cref="Document"/> to render and save as PDF.</param>
        /// <param name="filePath">The full file path where the PDF file will be saved.</param>
        /// <remarks>
        /// This method uses <see cref="PdfDocumentRenderer"/> from MigraDoc to render the document and save it as a PDF.
        /// Ensure that the document is properly constructed before calling this method.
        /// </remarks>
        public static void SaveDocumentToPdf(Document document, string filePath)
        {
            var pdfRenderer = new PdfDocumentRenderer()
            {
                Document = document
            };
            pdfRenderer.RenderDocument();
            pdfRenderer.PdfDocument.Save(filePath);
        }
    }
}
