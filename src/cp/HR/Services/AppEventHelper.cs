using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HR.Services
{
    public static class AppEventHelper
    {
        public enum EventCategory
        {
            Auth = 0,
            Data = 1,
            Navigation = 2,
            Service = 3,
        }
        public enum EventType
        {
            Progress = 0,
            Success = 1,
            Error = 2,
            Info = 3,
            Warning = 4,
        }
        public class AppEventArgs : EventArgs
        {
            public Guid Id { get; set; }
            public DateTime Timestamp { get; set; }
            public EventCategory Category { get; set; }
            public EventType Type { get; set; }
            public string Message { get; set; }
            public string Details { get; set; }
        }

        public static event EventHandler<AppEventArgs> AppEvent;
        public static void RaiseAppEvent(AppEventArgs evt) => AppEvent?.Invoke(null, evt);

        public static void LogAppEvent(AppEventArgs evt)
        {
            try
            {
                Request.MockAsync(2000);
            }
            catch (Exception exc)
            {
                AppEventArgs appEvt = new AppEventArgs
                {
                    Id = new Guid(),
                    Timestamp = DateTime.Now,
                    Category = EventCategory.Service,
                    Type = EventType.Error,
                    Message = "Ошибка при сохранении записи в журнал событий",
                    Details = exc.Message
                };
                RaiseEvent(appEvt, false);
            }
        }

        public static void RaiseEvent(AppEventArgs evt, bool shouldLog = true, bool shouldNotify = true)
        {
            if (shouldNotify) RaiseAppEvent(evt);
            if (shouldLog) LogAppEvent(evt);
        }
    }
}
