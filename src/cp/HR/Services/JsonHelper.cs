using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HR.Services
{
    /// <summary>
    /// Provides helper methods for working with JSON serialized collections stored in files.
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// Creates and returns JSON serializer settings with optional indentation.
        /// </summary>
        /// <param name="indented">If set to <c>true</c>, the JSON output will be formatted with indentation.</param>
        /// <returns>A <see cref="JsonSerializerSettings"/> instance configured with formatting and enum converter.</returns>
        private static JsonSerializerSettings GetSettings(bool indented = false)
        {
            return new JsonSerializerSettings
            {
                Formatting = indented ? Formatting.Indented : Formatting.None,
                Converters = new List<JsonConverter> { new Newtonsoft.Json.Converters.StringEnumConverter() }
            };
        }

        /* Handle Item */
        /// <summary>
        /// Adds a new item to the JSON collection stored in the specified file.
        /// </summary>
        /// <typeparam name="T">The type of the item in the collection.</typeparam>
        /// <param name="filePath">The path to the JSON file containing the collection.</param>
        /// <param name="newItem">The new item to add to the collection.</param>
        public static void AddItem<T>(string filePath, T newItem)
        {
            var items = LoadCollection<T>(filePath);
            items.Add(newItem);
            SaveCollection(filePath, items);
        }
        /// <summary>
        /// Deletes the first item in the JSON collection that satisfies the specified predicate.
        /// </summary>
        /// <typeparam name="T">The type of the item in the collection.</typeparam>
        /// <param name="filePath">The path to the JSON file containing the collection.</param>
        /// <param name="predicate">A function to test each item for a condition.</param>
        /// <returns><c>true</c> if an item was found and deleted; otherwise, <c>false</c>.</returns>
        public static bool DeleteItem<T>(string filePath, Func<T, bool> predicate)
        {
            var items = LoadCollection<T>(filePath);
            var itemToRemove = items.FirstOrDefault(predicate);
            if (itemToRemove != null)
            {
                items.Remove(itemToRemove);
                SaveCollection(filePath, items);
                return true;
            }
            return false;
        }

        /* Handle Items */
        /// <summary>
        /// Removes all items from the JSON collection that satisfy the specified predicate.
        /// </summary>
        /// <typeparam name="T">The type of the items in the collection.</typeparam>
        /// <param name="filePath">The path to the JSON file containing the collection.</param>
        /// <param name="predicate">A function to test each item for a condition.</param>
        /// <returns>The number of items removed from the collection.</returns>
        public static int RemoveItems<T>(string filePath, Func<T, bool> predicate)
        {
            var items = LoadCollection<T>(filePath);
            int initialCount = items.Count;
            items = items.Where(item => !predicate(item)).ToList();
            int removedCount = initialCount - items.Count;
            if (removedCount > 0)
            {
                SaveCollection(filePath, items);
            }
            return removedCount;
        }
        /// <summary>
        /// Asynchronously removes items from a collection stored in a file that match the specified predicate.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="filePath">The path to the file containing the collection.</param>
        /// <param name="predicate">A function to test each item for a condition. Items matching this predicate will be removed.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the number of items removed.
        /// </returns>
        public static async Task<int> RemoveItemsAsync<T>(string filePath, Func<T, bool> predicate)
        {
            // Load collection async
            var items = await LoadCollectionAsync<T>(filePath);
            int initialCount = items.Count;

            // Filter collection
            items = items.Where(item => !predicate(item)).ToList();
            int removedCount = initialCount - items.Count;

            // Save collection async if there are removed items
            if (removedCount > 0)
            {
                await SaveCollectionAsync(filePath, items);
            }

            return removedCount;
        }

        /* Handle Collection */
        /// <summary>
        /// Clears the JSON collection by saving an empty list to the specified file.
        /// </summary>
        /// <typeparam name="T">The type of the items in the collection.</typeparam>
        /// <param name="filePath">The path to the JSON file containing the collection.</param>
        public static void ClearCollection<T>(string filePath)
        {
            var emptyList = new List<T>();
            SaveCollection(filePath, emptyList);
        }
        /// <summary>
        /// Loads a collection of items from a JSON file.
        /// </summary>
        /// <typeparam name="T">The type of the items in the collection.</typeparam>
        /// <param name="filePath">The path to the JSON file to read.</param>
        /// <returns>A list of items loaded from the file; or an empty list if the file does not exist or is empty.</returns>
        public static List<T> LoadCollection<T>(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
                return new List<T>();

            string json = System.IO.File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<T>>(json, GetSettings()) ?? new List<T>();
        }
        public static Task<List<T>> LoadCollectionAsync<T>(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
                return Task.FromResult(new List<T>());

            return Task.Run(() =>
            {
                string json = System.IO.File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<T>>(json, GetSettings()) ?? new List<T>();
            });
        }
        /// <summary>
        /// Saves a collection of items to a JSON file with indentation formatting.
        /// </summary>
        /// <typeparam name="T">The type of the items in the collection.</typeparam>
        /// <param name="filePath">The path to the JSON file to write.</param>
        /// <param name="items">The collection of items to save.</param>
        public static void SaveCollection<T>(string filePath, List<T> items)
        {
            string json = JsonConvert.SerializeObject(items, GetSettings(true));
            System.IO.File.WriteAllText(filePath, json);
        }
        /// <summary>
        /// Asynchronously saves a collection of items to a file in JSON format.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="filePath">The path to the file where the collection will be saved.</param>
        /// <param name="items">The list of items to save.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        public static async Task SaveCollectionAsync<T>(string filePath, List<T> items)
        {
            string json = JsonConvert.SerializeObject(items, GetSettings(true));
            // Write file async
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                await writer.WriteAsync(json);
            }
        }
    }
}
