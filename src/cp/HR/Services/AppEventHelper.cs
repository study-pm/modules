using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static HR.Services.AppEventHelper;

namespace HR.Services
{
    /// <summary>
    /// Provides helper types and methods for application event handling and logging.
    /// </summary>
    public static class AppEventHelper
    {
        /// <summary>
        /// Represents application-level operations or states.
        /// </summary>
        public enum AppOp
        {
            Startup = 0,    // The application start up.
            Shutdown = 1,    // The application shut down.
            Login = 2,      // A user login operation.
            Logout = 3,     // A user logout operation.
        }
        /// <summary>
        /// Represents basic data operations
        /// </summary>
        public enum DataOp
        {
            Create = 0,     // Create operation.    // Blue
            Read = 1,       // Read operation.      // Orange
            Update = 2,     // Update operation.    // Green
            Delete = 3      // Delete operation.    // Red
        }
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
            public int Op { get; set; }                             // Gets or sets the event operation code.
            public string Name { get; set; }                        // Gets or sets the name for the event.
            public string Scope { get; set; }                       // Gets or sets the scope for the event.
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
    /// <summary>
    /// Extensions for the <see cref="EventCategory"/> enumeration.
    /// </summary>
    public static class EventCategoryExtensions
    {
        /// <summary>
        /// Returns the string representation of the event category in English.
        /// </summary>
        /// <param name="category">The event category.</param>
        /// <returns>The English title of the event category.</returns>
        public static string ToTitle(this EventCategory category)
        {
            switch (category)
            {
                case EventCategory.Auth:
                    return "Аутентификация";
                case EventCategory.Data:
                    return "Данные";
                case EventCategory.Navigation:
                    return "Навигация";
                case EventCategory.Service:
                    return "Сервис";
                default:
                    return category.ToString();
            }
        }
    }
    /// <summary>
    /// Extensions for the <see cref="AppEventHelper.EventType"/> enumeration.
    /// </summary>
    public static class EventTypeExtensions
    {
        /// <summary>
        /// Returns the string representation of the event type in English.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <returns>The English title of the event type.</returns>
        public static string ToTitle(this AppEventHelper.EventType eventType)
        {
            switch (eventType)
            {
                case AppEventHelper.EventType.Progress:
                    return "Прогресс";
                case AppEventHelper.EventType.Success:
                    return "Успех";
                case AppEventHelper.EventType.Error:
                    return "Ошибка";
                case AppEventHelper.EventType.Info:
                    return "Информация";
                case AppEventHelper.EventType.Warning:
                    return "Предупреждение";
                default:
                    return eventType.ToString();
            }
        }
    }
}
