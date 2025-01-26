namespace Select
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

            Console.WriteLine("/* Получить список только имён */");
            Console.WriteLine("// Using query syntax");
            var names1 = from u in users select u.Name;
            Console.WriteLine(string.Join(", ", names1));

            Console.WriteLine();

            Console.WriteLine("// Using extension syntax");
            var names2 = users.Select(u => u.Name);
            Console.WriteLine(string.Join(", ", names2));

            Console.WriteLine(Environment.NewLine);

            Console.WriteLine("/* Получить список из имени и года рождения */");
            Console.WriteLine("// Using query syntax");
            var items1 = from u in users select new {
                FirstName = u.Name,
                DateOfBirth = DateTime.Now.Year - u.Age
            };
            foreach (var i in items1) Console.WriteLine($"{i.FirstName} - {i.DateOfBirth}");

            Console.WriteLine();

            Console.WriteLine("// Using extension syntax");
            var items2 = users.Select(u => new
            {
                FirstName = u.Name,
                DateOfBirth = DateTime.Now.Year - u.Age
            });
            foreach (var i in items2) Console.WriteLine($"{i.FirstName} - {i.DateOfBirth}");
        }
    }
}
