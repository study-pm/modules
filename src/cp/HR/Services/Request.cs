using HR.Models;
using HR.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Services
{
    internal static class Request
    {
        public static async Task<List<Employee>> GetEmployees()
        {
            // Mock checking user existences from DB
            await Utils.MockAsync(1000);
            List<Employee> employees = new List<Employee>
            {
                new Employee { Id = 1, Surname = "Карягин", GivenName = "Сергей", Patronymic = "Николаевич", Gender = false, CareerStart = new DateTime(2012) }
            };
            return employees;
        }
    }
}
