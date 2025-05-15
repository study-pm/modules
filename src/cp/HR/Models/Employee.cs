using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string Surname { get; set; }
        public string GivenName { get; set; }
        public string Patronymic { get; set; }
        public bool Gender { get; set; }
        public DateTime CareerStart { get; set; }
        // Вычисляемый стаж в годах
        public int Experience
        {
            get
            {
                var today = DateTime.Today;
                int years = today.Year - CareerStart.Year;
                if (CareerStart.Date > today.AddYears(-years)) years--;
                return years;
            }
        }
    }
}
