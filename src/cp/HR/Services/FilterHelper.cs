using HR.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Services
{
    /// <summary>
    /// Provides helper methods for filtering objects based on specified filter parameters.
    /// </summary>
    internal class FilterHelper
    {
        /// <summary>
        /// Determines whether the specified object matches the criteria defined in the given <see cref="FilterParam"/>.
        /// </summary>
        /// <param name="obj">The object to filter.</param>
        /// <param name="filterParam">The filter parameters containing the property name and filter values.</param>
        /// <returns>
        /// <c>true</c> if the object matches the filter criteria or if the filter parameter is null or invalid; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// The filtering logic supports:
        /// <list type="bullet">
        /// <item>Range filtering for comparable types when <see cref="FilterParam.ValueTo"/> is specified.</item>
        /// <item>Filtering by a collection of values when <see cref="FilterParam.Value"/> is an enumerable (excluding strings).</item>
        /// <item>Special handling for <see cref="DateTime"/> values, including string parsing.</item>
        /// <item>Case-insensitive substring search for string properties.</item>
        /// <item>Simple equality comparison for other types.</item>
        /// </list>
        /// </remarks>
        public static bool FilterByValue(object obj, FilterParam filterParam)
        {
            if (filterParam == null || string.IsNullOrEmpty(filterParam.Name))
                return true;

            if (obj == null) return false;

            var itemType = obj.GetType();
            var prop = itemType.GetProperty(filterParam.Name);
            if (prop == null) return false;

            var val = prop.GetValue(obj, null);
            var filterVal = filterParam.Value;
            var filterValTo = filterParam.ValueTo;

            if (val == null || filterVal == null)
                return false;

            // Handle range (e.g. dates and numbers)
            if (filterValTo != null)
            {
                if (val is IComparable valComp && filterVal is IComparable fromComp && filterValTo is IComparable toComp)
                {
                    return valComp.CompareTo(fromComp) >= 0 && valComp.CompareTo(toComp) <= 0;
                }
            }

            // Handle list values (if filterVal is a collection)
            if (filterVal is System.Collections.IEnumerable filterCollection && !(filterVal is string))
            {
                foreach (var fVal in filterCollection)
                {
                    if (val.Equals(fVal))
                        return true;
                }
                return false;
            }

            // Special treatment for DateTime (single value)
            if (val is DateTime itemDate)
            {
                if (filterVal is DateTime filterDate)
                {
                    return itemDate.Date == filterDate.Date;
                }
                if (filterVal is string s && DateTime.TryParse(s, out var parsed))
                {
                    return itemDate.Date == parsed.Date;
                }
            }

            // Handle strings: search substring ignoring case
            if (val is string strVal)
            {
                string filterStr = filterVal.ToString();
                if (string.IsNullOrEmpty(filterStr))
                    return true;

                return strVal.IndexOf(filterStr, StringComparison.OrdinalIgnoreCase) >= 0;
            }

            // Handle other types: simple comparison
            return val.Equals(filterVal);
        }
    }
}
