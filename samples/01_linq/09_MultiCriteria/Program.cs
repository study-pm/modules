namespace MultiCriteria
{
    internal class Program
    {
        class User
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
        static void Main(string[] args)
        {
            List<User> users = new List<User>()
            {
                new User { Name = "Tom", Age = 33 },
                new User { Name = "Bob", Age = 30 },
                new User { Name = "Tom", Age = 21 },
                new User { Name = "Sam", Age = 43 }
            };

            Console.WriteLine("// Using query syntax");
            var sorted = from u in users
                         orderby u.Name, u.Age
                         select u;

            foreach (var u in sorted) Console.WriteLine("{0} - {1}", u.Name, u.Age);

            Console.WriteLine();

            Console.WriteLine("// Using lambda extension syntax");
            var ordered = users.OrderBy(u => u.Name).ThenBy(u => u.Age);

            foreach (var u in ordered) Console.WriteLine("{0}: {1}", u.Name, u.Age);
        }
    }
}
