using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Utilities
{
    public enum StatusType
    {
        Info,
        Progress,
        Success,
        Warning,
        Error
    }
    public class StatusEventArgs : EventArgs
    {
        public StatusType Type { get; }
        public string Message { get; }

        public StatusEventArgs(StatusType type, string message)
        {
            Type = type;
            Message = message;
        }
    }
    public static class StatusInformer
    {
        public static event EventHandler<StatusEventArgs> StatusChanged;

        public static void RaiseStatus(StatusType type, string message)
        {
            StatusChanged?.Invoke(null, new StatusEventArgs(type, message));
        }
        public static void ReportFailure(string message) => RaiseStatus(StatusType.Error, message);
        public static void ReportInfo(string message) => RaiseStatus(StatusType.Info, message);
        public static void ReportProgress(string message) => RaiseStatus(StatusType.Progress, message);
        public static void ReportSuccess(string message) => RaiseStatus(StatusType.Success, message);
        public static void ReportWarning(string message) => RaiseStatus(StatusType.Warning, message);
    }
}
