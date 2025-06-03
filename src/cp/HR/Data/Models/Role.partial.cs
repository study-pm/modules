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
                        return "Администратор";
                    case 2:
                        return "Сопровождающий";
                    case 3:
                        return "Редактор";
                    case 4:
                        return "Пользователь";
                    default:
                        return Title;
                }
            }
        }
    }
}
