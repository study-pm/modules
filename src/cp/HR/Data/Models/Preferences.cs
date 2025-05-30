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
    public class Preferences
    {
        public static readonly string basePath = "Preferences";
        public static readonly string prefsFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, basePath);
        public int UserId {  get; set; }
        public bool IsStayLoggedIn { get; set; }
        private static string GetFilePath(int uid) => System.IO.Path.Combine(prefsFolder, $"{uid.ToString()}.xml");
        public static async Task<Preferences> LoadAsync(int uid)
        {
            string filePath = GetFilePath(uid);
            if (!System.IO.File.Exists(filePath))
            {
                return new Preferences { UserId = uid, IsStayLoggedIn = false };
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
                    return new Preferences { IsStayLoggedIn = false };
                }
            });
        }

        public async Task SaveAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    var filePath = GetFilePath(UserId);
                    var directory = System.IO.Path.GetDirectoryName(filePath);
                    if (!System.IO.Directory.Exists(directory))
                        System.IO.Directory.CreateDirectory(directory);

                    var serializer = new XmlSerializer(typeof(Preferences));
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        serializer.Serialize(stream, this);
                    }
                }
                catch (Exception exc)
                {
                    Debug.WriteLine(exc.Message);
                    throw exc;
                }
            });
        }
    }
}
