using System.Diagnostics.SymbolStore;

namespace WhereComplex
{
    internal class Program
    {
        class User
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public List<string> Languages { get; set; }
            public User()
            {
                Languages = new List<string>();
            }
        }
        static void Main(string[] args)
        {
            List<User> users = new List<User>
            {
                new User { Name="Том", Age=23, Languages = new List<string> {"английский", "немецкий"} },
                new User { Name="Боб", Age=27, Languages = new List<string> {"английский", "французский"} },
                new User { Name="Джон", Age=29, Languages = new List<string> {"английский", "испанский"} },
                new User { Name="Элис", Age=24, Languages = new List<string> {"испанский", "немецкий"} }
            };

            /* Выбрать всех пользователей старше 25 лет */
            // Query syntax
            var selectedUsers1 = from user in users
                                 where user.Age > 25
                                 orderby user.Name
                                 select user;

            Console.WriteLine("Selected by query syntax:");
            foreach (User user in selectedUsers1) Console.WriteLine($"{user.Name} - {user.Age}");

            Console.WriteLine();

            var selectedUsers2 = users.Where(u => u.Age > 25).OrderBy(u => u.Name);
            Console.WriteLine("Selected by extension syntax:");
            foreach (User user in selectedUsers1) Console.WriteLine($"{user.Name}: {user.Age}");

            Console.WriteLine(Environment.NewLine);

            /* Отфильтровать пользователей по языку и возрасту */
            // Query syntax
            var selected1 = from user in users
                            from lang in user.Languages
                            where user.Age < 28
                            where lang == "английский"
                            select user;
            Console.WriteLine("Selected by query syntax:");
            foreach (User user in selected1) Console.WriteLine($"{user.Name} - {user.Age}");

            Console.WriteLine();

            // Extension syntax
            var selected2 = users.SelectMany(u => u.Languages, (u, l) => new { User = u, Lang = l })
                .Where(u => u.Lang == "английский" && u.User.Age < 28)
                .Select(u => u.User);
            Console.WriteLine("Selected by extension syntax:");
            foreach (User user in selected2) Console.WriteLine($"{user.Name} - {user.Age}");
        }
    }
}
