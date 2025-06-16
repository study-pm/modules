using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HR.Services
{
    /// <summary>
    /// Provides helper methods for working with CSV data, including escaping CSV values,
    /// exporting data from an <see cref="ICollectionView"/> to CSV format, and saving CSV content to a file asynchronously.
    /// </summary>
    public static class CsvHelper
    {
        /// <summary>
        /// Escapes a string value to be safely included in a CSV file.
        /// It doubles any double quotes and wraps the value in quotes if it contains commas or new lines.
        /// </summary>
        /// <param name="value">The string value to escape.</param>
        /// <returns>The escaped CSV string.</returns>
        public static string EscapeCsv(string value)
        {
            if (value.Contains("\""))
                value = value.Replace("\"", "\"\"");

            if (value.Contains(",") || value.Contains("\n") || value.Contains("\r"))
                value = $"\"{value}\"";

            return value;
        }
        /// <summary>
        /// Exports the data from an <see cref="ICollectionView"/> to a CSV formatted string.
        /// Allows skipping specified properties, applying custom converters for property values,
        /// and using custom headers for CSV columns.
        /// </summary>
        /// <param name="collectionView">The collection view containing the data to export.</param>
        /// <param name="filePath">The file path where the CSV will be saved (not used in this method but typically for context).</param>
        /// <param name="skipProperties">An optional list of property names to exclude from the CSV output.</param>
        /// <param name="customConverters">
        /// An optional dictionary mapping property names to functions that convert property values to strings.
        /// </param>
        /// <param name="customHeaders">
        /// An optional dictionary mapping property names to custom header names to use in the CSV output.
        /// </param>
        /// <returns>A CSV formatted string representing the data in the collection view.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="collectionView"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the collection view is empty.</exception>
        public static string ExportCollectionViewToCsv(
            ICollectionView collectionView,
            string filePath,
            IEnumerable<string> skipProperties = null,
            Dictionary<string, Func<object, string>> customConverters = null,
            Dictionary<string, string> customHeaders = null)
        {
            if (collectionView == null)
                throw new ArgumentNullException(nameof(collectionView));

            var skipSet = new HashSet<string>(skipProperties ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            customConverters = customConverters ?? new Dictionary<string, Func<object, string>>(StringComparer.OrdinalIgnoreCase);
            customHeaders = customHeaders ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var enumerator = collectionView.GetEnumerator();
            if (!enumerator.MoveNext())
                throw new InvalidOperationException("CollectionView is empty.");

            var itemType = enumerator.Current.GetType();
            var properties = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                     .Where(p => !skipSet.Contains(p.Name))
                                     .ToArray();

            var sb = new StringBuilder();

            // CSV headers — use customHeaders if provided, otherwise property names
            var headers = properties.Select(p =>
                customHeaders.TryGetValue(p.Name, out var header) ? header : p.Name);

            sb.AppendLine(string.Join(",", headers.Select(EscapeCsv)));

            // First data row
            sb.AppendLine(string.Join(",", properties.Select(p => EscapeCsv(GetValue(enumerator.Current, p, customConverters)))));

            // Remaining rows
            while (enumerator.MoveNext())
            {
                sb.AppendLine(string.Join(",", properties.Select(p => EscapeCsv(GetValue(enumerator.Current, p, customConverters)))));
            }

            return sb.ToString();
        }
        /// <summary>
        /// Retrieves the string representation of a property value from an object,
        /// applying a custom converter if one is provided for the property.
        /// </summary>
        /// <param name="obj">The object from which to get the property value.</param>
        /// <param name="prop">The property information.</param>
        /// <param name="converters">A dictionary of custom converters keyed by property name.</param>
        /// <returns>The string representation of the property value, or an empty string if the value is null.</returns>
        private static string GetValue(object obj, PropertyInfo prop, Dictionary<string, Func<object, string>> converters)
        {
            var value = prop.GetValue(obj);
            if (converters.TryGetValue(prop.Name, out var converter))
            {
                return converter(value);
            }
            return value?.ToString() ?? "";
        }
        /// <summary>
        /// Asynchronously saves the given CSV content to a file at the specified path.
        /// </summary>
        /// <param name="filePath">The full path to the CSV file to save.</param>
        /// <param name="csvContent">The CSV content as a string.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        public static async Task SaveCsvAsync(string filePath, string csvContent)
        {
            // Using StreamWriter with async write to save the CSV content
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                await writer.WriteAsync(csvContent);
            }
        }
    }
}
