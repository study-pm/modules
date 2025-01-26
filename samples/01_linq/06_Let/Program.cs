namespace Let
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

            Console.WriteLine("/* Список имен с приставкой Mr. */");
            var people = from u in users
                          let name = "Mr." + u.Name
                          select new
                          {
                              Name = name,
                              u.Age
                          };

            foreach (var p in people) Console.WriteLine($"{p.Name}");
        }
    }
}
