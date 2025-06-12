using HR.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HR.Services.AppEventHelper;
using System.Windows;
using System.Diagnostics;

namespace HR.Utilities
{
    /// <summary>
    /// Represents a logger that writes events to a JSON log file with filtering support by event categories and types.
    /// Implements <see cref="IDisposable"/> to properly unsubscribe from events and release resources.
    /// </summary>
    public class Logger : IDisposable
    {
        /// <summary>
        /// Directory where log files are stored.
        /// </summary>
        public static readonly string logsDir = "Logs";
        /// <summary>
        /// The file path of the log file for this logger instance.
        /// </summary>
        public string filePath;
        /// <summary>
        /// List of event categories that this logger processes.
        /// </summary>
        public List<EventCategory> categories;
        /// <summary>
        /// List of event types that this logger processes.
        /// </summary>
        public List<EventType> types;

        private bool _disposed = false; // Flag to prevent multiple calls to Dispose
        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class for a specified user,
        /// creates the log directory if it does not exist, and subscribes to application events.
        /// </summary>
        /// <param name="uid">The unique user identifier used as the log file name.</param>
        /// <param name="categories">The event categories to filter.</param>
        /// <param name="types">The event types to filter.</param>
        public Logger(int uid, List<EventCategory> categories, List<EventType> types)
        {
            filePath = System.IO.Path.Combine(logsDir, uid + ".json");
            string logDirectory = System.IO.Path.GetDirectoryName(filePath);

            if (!System.IO.Directory.Exists(logDirectory))
            {
                System.IO.Directory.CreateDirectory(logDirectory);
            }
            AppEvent += AppEventHelper_AppEvent;
            this.categories = categories;
            this.types = types;
        }
        /// <summary>
        /// Releases all resources used by the <see cref="Logger"/>, including unsubscribing from events.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Releases unmanaged and optionally managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources;
        /// <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Release managed resources
                AppEvent -= AppEventHelper_AppEvent;
            }

            // Release unmanage resources (if any)
            _disposed = true;
        }
        /// <summary>
        /// Finalizer that releases resources if Dispose was not called.
        /// </summary>
        ~Logger()
        {
            Dispose(false);
        }
        /// <summary>
        /// Application event handler that logs the event.
        /// </summary>
        /// <param name="sender">The event source.</param>
        /// <param name="e">The event arguments.</param>
        private void AppEventHelper_AppEvent(object sender, AppEventArgs e)
        {
            LogEvent(e);
        }
        /// <summary>
        /// Logs the specified event to the log file if it matches the configured categories and types.
        /// If an error occurs during logging, raises an error event.
        /// </summary>
        /// <param name="evt">The event to log.</param>
        /// <param name="fileName">The log file name.</param>
        public void LogEvent(AppEventArgs evt)
        {
            try
            {
                if (categories.Any(x => x == evt.Category) && types.Any(x => x == evt.Type))
                {
                    JsonHelper.AddItem(filePath, evt);
                }
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"Error logging event: {exc.ToString()}");
            }
        }
    }
}
