using HR.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
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
        public List<int> LogCategories { get; set; } = new List<int>();
        public List<int> LogTypes { get; set; } = new List<int>();

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
