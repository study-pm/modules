using HR.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace HR.Data.Models
{
    [XmlRoot("Preferences")]
    public class Preferences
    {
        public static readonly string basePath = "Preferences";
        public static readonly string prefsFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, basePath);
        public bool IsStayLoggedIn { get; set; }
        public bool IsLogOn { get; set; }
        [XmlIgnore]
        public HashSet<int> LogCategories { get; set; } = new HashSet<int> { 0, 1, 2, 3 };

        [XmlArray("LogCategories")]
        [XmlArrayItem("Category")]
        public List<int> LogCategoriesSerializable
        {
            get => LogCategories.ToList();
            set => LogCategories = new HashSet<int>(value ?? new List<int>());
        }
        [XmlIgnore]
        public HashSet<int> LogTypes { get; set; } = new HashSet<int> { 1, 2, 3, 4, 5 };

        [XmlArray("LogTypes")]
        [XmlArrayItem("Type")]
        public List<int> LogTypesSerializable
        {
            get => LogTypes.ToList();
            set => LogTypes = new HashSet<int>(value ?? new List<int>());
        }

        private static string GetFilePath(int uid) => System.IO.Path.Combine(prefsFolder, $"{uid.ToString()}.xml");
        public static async Task<Preferences> LoadAsync(int uid)
        {
            string filePath = GetFilePath(uid);
            if (!System.IO.File.Exists(filePath))
            {
                return new Preferences();
            }
            return await Task.Run(() =>
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(Preferences));
                    using (var stream = System.IO.File.OpenRead(filePath))
                    {
                        return (Preferences)serializer.Deserialize(stream);
                    }
                }
                catch(Exception exc)
                {
                    Debug.WriteLine(exc.Message);
                    return new Preferences();
                }
            });
        }
    }
}
