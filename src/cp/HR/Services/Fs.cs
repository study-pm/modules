using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace HR.Services
{
    internal static class Fs
    {
        /// <summary>
        /// Creates bitmap image
        /// </summary>
        /// <param name="path">Fil path</param>
        /// <returns>Bitmap image</returns>
        public static BitmapImage CreateBitmapImage(string uriString)
        {
            try
            {
                var uri = new Uri(uriString, UriKind.Absolute);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = uri;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                // Return null or a default image if loading fails
                return null;
            }
        }
        /// <summary>
        /// Creates bitmap image from file
        /// </summary>
        /// <param name="path">Fil path</param>
        /// <returns>Bitmap image</returns>
        public static BitmapImage CreateBitmapImageFromFile(string path)
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(path, UriKind.Absolute);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }
        /// <summary>
        /// Search for an image file inside the parent folders (up till the disk root)
        /// </summary>
        /// <param name="startDir"></param>
        /// <param name="imagesFolder"></param>
        /// <param name="imageName"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string FindInParentDirectories(string startDir, string imagesFolder, string imageName, string extension)
        {
            DirectoryInfo dir = new DirectoryInfo(startDir);
            while (dir != null)
            {
                string imagesPath = Path.Combine(dir.FullName, imagesFolder, imageName);
                if (File.Exists(imagesPath))
                    return imagesPath;

                if (!Path.HasExtension(imagesPath))
                {
                    string imagesPathWithExt = imagesPath + "." + extension;
                    if (File.Exists(imagesPathWithExt))
                        return imagesPathWithExt;
                }
                dir = dir.Parent;
            }
            return null;
        }
        /// <summary>
        /// Loads a <see cref="BitmapImage"/> from a byte array containing image data.
        /// </summary>
        /// <param name="imageData">The byte array representing the image data to load.</param>
        /// <returns>A <see cref="BitmapImage"/> created from the provided byte array.</returns>
        /// <remarks>
        /// The method creates a <see cref="MemoryStream"/> from the byte array and initializes
        /// the <see cref="BitmapImage"/> with caching enabled for immediate loading.
        /// The stream is disposed after the image is loaded.
        /// </remarks>
        public static BitmapImage LoadImage(byte[] imageData)
        {
            using (var ms = new MemoryStream(imageData))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }
    }
}
