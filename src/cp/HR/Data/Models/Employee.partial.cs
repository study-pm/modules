using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Data.Models
{
    public partial class Employee
    {
        /// <summary>
        /// Returns work experience in full years calculated from CareerStart until the current date
        /// Returns 0 if CareerStart is unset
        /// </summary>
        public int Experience
        {
            get
            {
                if (!CareerStart.HasValue)
                    return 0;

                DateTime start = CareerStart.Value;
                DateTime now = DateTime.Today;

                int years = now.Year - start.Year;

                // Если текущая дата раньше даты начала карьеры в этом году, уменьшаем на 1 год
                if (now.Month < start.Month || (now.Month == start.Month && now.Day < start.Day))
                    years--;

                return years >= 0 ? years : 0;
            }
        }
        /// <summary>
        /// Gets full name consisting of Surname, GivenName and Patronymic separated by spaces
        /// Omitting empty and null values to avoid extra spaces
        /// </summary>
        public string FullName
        {
            get => string.Join(" ", new[] { Surname, GivenName, Patronymic }.Where(s => !string.IsNullOrWhiteSpace(s)));
        }
    }
}
