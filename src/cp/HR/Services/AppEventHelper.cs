using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HR.Services
{
    /// <summary>
    /// Provides helper types and methods for application event handling and logging.
    /// </summary>
    public static class AppEventHelper
    {
        /// <summary>
        /// Defines categories for application events to classify their origin or purpose.
        /// </summary>
        public enum EventCategory
        {
            Auth = 0,       // Events related to authentication processes.
            Data = 1,       // Events related to data operations.
            Navigation = 2, // Events related to navigation within the application.
            Service = 3,    // Events related to internal service or system operations.
        }
        /// <summary>
        /// Defines types of application events indicating their status or severity.
        /// </summary>
        public enum EventType
        {
            Progress = 0,   // Event indicating progress or ongoing operation.
            Success = 1,    // Event indicating successful completion.
            Error = 2,      // Event indicating an error occurred.
            Info = 3,       // Informational event.
            Warning = 4,    // Event indicating a warning condition.
            Fatal = 5,      // Event indicating a fatal error or critical failure.
        }
        /// <summary>
        /// Provides data for application events, including identification, timing, classification, and descriptive messages.
        /// </summary>
        public class AppEventArgs : EventArgs
        {
            public Guid Id { get; set; } = Guid.NewGuid();          // Gets or sets the unique identifier of the event.
            public DateTime Timestamp { get; set; } = DateTime.Now; // Gets or sets the timestamp when the event occurred.
            public EventCategory Category { get; set; }             // Gets or sets the category of the event.
            public EventType Type { get; set; }                     // Gets or sets the type of the event.
            public string Message { get; set; }                     // Gets or sets the main message or description of the event.
            public string Details { get; set; }                     // Gets or sets additional details about the event.
        }

        /// <summary>
        /// Occurs when an application event is raised.
        /// </summary>
        public static event EventHandler<AppEventArgs> AppEvent;
        /// <summary>
        /// Raises the <see cref="AppEvent"/> with the specified event arguments.
        /// </summary>
        /// <param name="evt">The event data to raise.</param>
        public static void RaiseAppEvent(AppEventArgs evt) => AppEvent?.Invoke(null, evt);
    }
}
