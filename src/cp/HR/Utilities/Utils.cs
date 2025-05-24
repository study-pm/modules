using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace HR.Utilities
{
    internal static class Utils
    {
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
        public static Task<bool> MockAsync(int delay, bool isSuccess = true)
        {
            var tcs = new TaskCompletionSource<bool>();
            var timer = new System.Timers.Timer(delay);

            timer.Elapsed += (sender, args) =>
            {
                timer.Stop();
                timer.Dispose();
                if (isSuccess) tcs.SetResult(true);
                else tcs.SetException(new Exception("Mock failure"));
            };

            timer.AutoReset = false; // To make it one-time only
            timer.Start();

            return tcs.Task;
        }
    }
}
