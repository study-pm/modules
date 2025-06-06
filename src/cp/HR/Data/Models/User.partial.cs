﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Data.Models
{
    public enum UserStatus : byte
    {
        Created = 0,    // Default status set right after user registration
        Activated = 1,  // Confirmed/Verified by admin
        Locked = 2      // Blocked by administrator
    }
    public partial class User
    {
        public static string basePath = AppDomain.CurrentDomain.BaseDirectory;
        public static string uidFilePath = System.IO.Path.Combine(basePath, "user.uid");
        public string StatusName
        {
            get
            {
                switch (Status)
                {
                    case 0:
                        return "новый";
                    case 1:
                        return "активный";
                    case 2:
                        return "заблокирован";
                    default:
                        return "удален";
                }
            }
        }
        UserStatus State {
            get => (UserStatus)Status;
            set => Status = (byte)value;
        }
    }
}
