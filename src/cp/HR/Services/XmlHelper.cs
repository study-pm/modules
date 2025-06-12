using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HR.Services
{
    /// <summary>
    /// Provides helper methods for asynchronous XML serialization and deserialization of objects.
    /// </summary>
    public static class XmlHelper
    {
        /// <summary>
        /// Asynchronously loads an object of type <typeparamref name="T"/> from an XML file.
        /// If the file does not exist or deserialization fails, returns a new instance of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object to load. Must have a parameterless constructor.</typeparam>
        /// <param name="filePath">The full path to the XML file.</param>
        /// <returns>A task representing the asynchronous operation, with the deserialized object or a new instance of <typeparamref name="T"/> as result.</returns>
        public static async Task<T> LoadAsync<T>(string filePath) where T : new()
        {
            if (!System.IO.File.Exists(filePath))
            {
                return new T();
            }

            return await Task.Run(() =>
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(T));
                    using (var stream = System.IO.File.OpenRead(filePath))
                    {
                        return (T)serializer.Deserialize(stream);
                    }
                }
                catch (Exception exc)
                {
                    Debug.WriteLine(exc.Message);
                    return new T();
                }
            });
        }
        /// <summary>
        /// Asynchronously saves an object of type <typeparamref name="T"/> to an XML file.
        /// Creates the directory if it does not exist.
        /// </summary>
        /// <typeparam name="T">The type of the object to save.</typeparam>
        /// <param name="obj">The object instance to serialize and save.</param>
        /// <param name="filePath">The full path to the XML file.</param>
        /// <returns>A task representing the asynchronous save operation.</returns>
        public static async Task SaveAsync<T>(T obj, string filePath)
        {
            await Task.Run(() =>
            {
                try
                {
                    var directory = System.IO.Path.GetDirectoryName(filePath);
                    if (!System.IO.Directory.Exists(directory))
                        System.IO.Directory.CreateDirectory(directory);

                    var serializer = new XmlSerializer(typeof(T));
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        serializer.Serialize(stream, obj);
                    }
                }
                catch (Exception exc)
                {
                    Debug.WriteLine(exc.Message);
                    throw;
                }
            });
        }
    }
}
