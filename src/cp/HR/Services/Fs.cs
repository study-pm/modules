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
        /// Executable file directory path
        /// </summary>
        public static readonly string appDir = AppDomain.CurrentDomain.BaseDirectory;
        /// <summary>
        /// Project/solution root directory path
        /// </summary>
        public static readonly string rootDir = System.IO.Path.GetFullPath(Path.Combine(appDir, "..", ".."));
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
        /// Combines the root directory path with a relative URI to produce a full file system path.
        /// </summary>
        /// <param name="relativeUri">The relative path or URI to combine with the root directory.</param>
        /// <returns>A full path string that combines the root directory and the relative URI.</returns>
        public static string GetFullRootPath(string relativeUri) => Path.Combine(rootDir, relativeUri);
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
        /// <summary>
        /// Asynchronously reads all bytes from the specified file path.
        /// </summary>
        /// <param name="path">The full file path to read from.</param>
        /// <returns>A task that represents the asynchronous read operation. The task result contains a byte array with the file's contents.</returns>
        /// <remarks>
        /// The method opens the file with asynchronous read enabled and reads its entire content into a byte array.
        /// It reads in a loop until all bytes are read or the end of the file is reached.
        /// </remarks>
        public static async Task<byte[]> ReadAllBytesAsync(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
            {
                byte[] result = new byte[fs.Length];
                int bytesRead = 0;
                while (bytesRead < fs.Length)
                {
                    int read = await fs.ReadAsync(result, bytesRead, (int)(fs.Length - bytesRead));
                    if (read == 0)
                        break;
                    bytesRead += read;
                }
                return result;
            }
        }
    }
}
