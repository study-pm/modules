using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HR.Models
{
    public class EmployeeX
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
        public static List<EmployeeX> LoadFromXml(string xml)
        {
            XDocument doc = XDocument.Parse(xml);
            var employees = doc.Descendants("row")
                .Select(e => new EmployeeX
                {
                    // Если id пустой, можно присвоить 0 или сгенерировать
                    Id = int.TryParse((string)e.Element("id"), out int id) ? id : 0,
                    Surname = (string)e.Element("surname"),
                    GivenName = (string)e.Element("given_name"),
                    Patronymic = (string)e.Element("patronymic"),
                    Gender = ((string)e.Element("sex")) == "m" ? false : true, // false = муж, true = жен
                    CareerStart = ParseYear((string)e.Element("career_start"))
                })
                .ToList();
            return employees;
        }

        // Парсинг года в DateTime
        private static DateTime ParseYear(string year)
        {
            if (int.TryParse(year, out int y))
                return new DateTime(y, 1, 1);
            return DateTime.MinValue;
        }
    }
}
