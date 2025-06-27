using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;

namespace HR.Services
{
    /// <summary>
    /// Provides helper methods for image handling, including displaying image popups,
    /// saving images to a designated folder, and setting preview images in UI controls.
    /// </summary>
    public static class ImgHelper
    {
        /// <summary>
        /// The base folder name where images are stored.
        /// </summary>
        public static readonly string imgBase = "Images";
        /// <summary>
        /// Opens a popup displaying the full-size image associated with the specified button.
        /// Assumes the button contains a thumbnail image and that a popup exists in the visual tree hierarchy.
        /// </summary>
        /// <param name="btn">The button containing the thumbnail image.</param>
        public static void OpenImgPopup(Button btn)
        {
            // Search for the thumb Image inside the Button
            var image = btn.Content as Image;
            if (image == null) return;

            // Search for the parent Grid
            DependencyObject grid = VisualTreeHelper.GetParent(btn);
            while (grid != null && !(grid is Grid))
            {
                grid = VisualTreeHelper.GetParent(grid);
            }
            if (grid == null) return;

            // Find a Popup inside the Grid
            Popup popup = null;
            int childrenCount = VisualTreeHelper.GetChildrenCount(grid);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(grid, i);
                if (child is Popup p)
                {
                    popup = p;
                    break;
                }
            }
            if (popup == null) return;

            // Search for a full-scale Image inside the Popup
            if (!(popup.Child is Border border)) return;
            if (!(border.Child is ScrollViewer scrollViewer)) return;
            if (!(scrollViewer.Content is Image popupImage)) return;

            // Set image source and open Popup
            popupImage.Source = image.Source;
            popup.IsOpen = true;
        }
        /// <summary>
        /// Saves an image file from the specified source path to the images folder within the project,
        /// ensuring a unique file name by appending a numeric suffix if needed.
        /// </summary>
        /// <param name="srcPath">The full path to the source image file.</param>
        /// <param name="maxNameLength">
        /// Optional. The maximum allowed length for the file name (without extension).
        /// If the original file name exceeds this length, it will be truncated.
        /// Default value is 256.
        /// </param>
        /// <returns>The file name under which the image was saved in the images folder.</returns>
        public static string SaveImg(string srcPath, int maxNameLength = 256)
        {
            // Source image name
            string srcName = System.IO.Path.GetFileName(srcPath);
            // Пути
            string projectRoot = System.IO.Path.Combine(Fs.appDir, "..", "..");
            string fullPath = System.IO.Path.GetFullPath(Fs.rootDir);
            // Folder for storing images
            string destFolder = System.IO.Path.Combine(fullPath, imgBase);
            // Create folder if it does not exist
            if (!Directory.Exists(destFolder)) Directory.CreateDirectory(destFolder);

            // Target name of the file
            string destName = System.IO.Path.GetFileName(srcName);
            // Target full path
            string destPath = System.IO.Path.Combine(destFolder, destName);
            // Get file name without extension and extension
            string fileName = System.IO.Path.GetFileNameWithoutExtension(srcName);
            string fileExt = System.IO.Path.GetExtension(srcName);
            // Restrict file name up to maxNameLength symbols
            if (fileName.Length > maxNameLength)
            {
                fileName = fileName.Substring(0, maxNameLength);
            }
            // File name suffix
            int count = 1;
            // Generate new file name with suffix (_1, _2, etc.) if file already exists
            while (File.Exists(destPath))
            {
                destName = $"{fileName}_{count}{fileExt}";
                destPath = System.IO.Path.Combine(destFolder, destName);
                count++;
            }
            // Copy file by unique path
            File.Copy(srcPath, destPath, true);
            return destName;
        }
        /// <summary>
        /// Sets the source of the specified <see cref="Image"/> control to the image located at the given path,
        /// loading the image immediately to avoid locking the file.
        /// </summary>
        /// <param name="imgPath">The file path of the image to display.</param>
        /// <param name="ctl">The <see cref="Image"/> control where the image will be displayed.</param>
        public static void SetPreviewImage(string imgPath, Image ctl)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(imgPath);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            ctl.Source = image;
        }
    }
}
