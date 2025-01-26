namespace MultiSrc
{
    internal class Program
    {
        class Phone
        {
            public string Name { get; set; }
            public string Company { get; set; }
        }

        class User
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        static void Main(string[] args)
        {
            List<User> users = new List<User>()
            {
                new User { Name = "Sam", Age = 43 },
                new User { Name = "Tom", Age = 33 }
            };

            List<Phone> phones = new List<Phone>()
            {
                new Phone { Name = "Lumia 630", Company = "Microsoft" },
                new Phone { Name = "iPhone 6", Company = "Apple" }
            };

            Console.WriteLine("// Using query syntax");
            var people1 = from u in users
                         from p in phones
                         select new { u.Name, Phone = p.Name };


            foreach (var p in people1) Console.WriteLine($"{p.Name} = {p.Phone}");

            Console.WriteLine();

            Console.WriteLine("// Using lambda extension syntax");
            var people2 = users.SelectMany(u => phones, (u, p) => new
            {
                u.Name,
                Phone = p.Name
            });
            foreach (var p in people2) Console.WriteLine($"{p.Name} = {p.Phone}");
        }
    }
}
