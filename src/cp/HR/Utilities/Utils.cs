using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Utilities
{
    internal static class Utils
    {
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
