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
        [DataContract]
        public enum EventCategory
        {
            [EnumMember] Auth = 0,
            [EnumMember] Data = 1,
            [EnumMember] Navigation = 2,
            [EnumMember] Service = 3,
        }
        [DataContract]
        public enum EventType
        {
            [EnumMember] Progress = 0,
            [EnumMember] Success = 1,
            [EnumMember] Error = 2,
            [EnumMember] Info = 3,
            [EnumMember] Warning = 4,
        }
        public class AppEventArgs : EventArgs
        {
            [DataMember] public Guid Id { get; set; }
            [DataMember] public DateTime Timestamp { get; set; }
            [DataMember] public EventCategory Category { get; set; }
            [DataMember] public EventType Type { get; set; }
            [DataMember] public string Message { get; set; }
            [DataMember] public string Details { get; set; }
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
