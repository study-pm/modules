using HR.Services;
using QRCoder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace HR.Utilities
{
    internal static class Utils
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
        /// <summary>
        /// Generates a QR code image as a byte array representing the OTP Auth URI for a given secret and user login.
        /// </summary>
        /// <param name="secret">The secret key used for generating the OTP Auth URI.</param>
        /// <param name="login">The user login or identifier to include in the OTP Auth URI.</param>
        /// <returns>A byte array containing the PNG image data of the generated QR code.</returns>
        /// <remarks>
        /// The generated QR code encodes the OTP Auth URI, which can be scanned by authenticator apps to set up time-based one-time password (TOTP) authentication.
        /// </remarks>
        public static byte[] GetQrCode(byte[] secret, string login)
        {
            // Generate OTP Auth URI
            string pathUrl = Crypto.GetOtpAuthUri(secret, login);
            // Generate QR code
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(pathUrl, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(20);
        }
    }
}
