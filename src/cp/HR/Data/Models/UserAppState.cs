using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Data.Models
{
    [Serializable]
    public class UserAppState
    {
        public static readonly string basePath = "Cache";
        public static readonly string cacheFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, basePath);
        public string LastState { get; set; }
    }
}
