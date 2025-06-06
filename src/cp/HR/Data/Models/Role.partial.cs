using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Data.Models
{
    public partial class Role
    {
        public string Name
        {
            get
            {
                switch (Id)
                {
                    case 1:
                        return "администратор";
                    case 2:
                        return "сопровождающий";
                    case 3:
                        return "редактор";
                    case 4:
                        return "пользователь";
                    default:
                        return Title;
                }
            }
        }
    }
}
