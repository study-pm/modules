namespace Where
{
    internal class Program
    {
        static void Main(string[] args)
        {
            /* Выбрать все четные элементы, которые больше 10 */
            int[] numbers = { 1, 2, 3, 4, 10, 34, 55, 66, 77, 88 };

            // Query syntax
            IEnumerable<int> evens1 = from i in numbers
                                     where i % 2 == 0 && i > 10
                                     select i;

            Console.WriteLine("Evens by query syntax:");
            foreach (int i in evens1) Console.WriteLine(i);

            Console.WriteLine();

            // Lambda/Extension syntax
            IEnumerable<int> evens2 = numbers.Where(i => i % 2 == 0 && i > 10);

            Console.WriteLine("Evens by lambda/ext syntax:");
            foreach (int i in evens2) Console.WriteLine(i);

            Console.WriteLine(Environment.NewLine);

            /* Выбрать все имена, длина которых меньше 6 */
            string[] names = {
                "Adams", "Arthur", "Buchanan", "Bush", "Carter", "Cleveland", "Clinton", "Coolidge", "Eisenhower",
                "Fillmore", "Ford", "Garfield", "Grant", "Harding", "Harrison", "Hayes", "Hoover", "Jackson", "Jefferson", "Johnson",
                "Kennedy", "Lincoln", "Madison", "McKinley", "Monroe", "Nixon", "Obama", "Pierce", "Polk", "Reagan", "Roosevelt",
                "Taft", "Taylor", "Truman", "Tyler", "Van Buren", "Washington", "Wilson"
            };

            // Query syntax
            IEnumerable<string> seq1 = from n in names
                                       where n.Length < 6
                                       select n;

            Console.WriteLine("Short names by query syntax:");
            Console.WriteLine(string.Join(", ", seq1));

            Console.WriteLine();

            // Extension/Lambda syntax
            IEnumerable<string> seq2 = names.Where(n => n.Length < 6).Select(i => i);
            // The same as:
            seq2 = names.Where(n => n.Length < 6);
            Console.WriteLine("Short names by lambda syntax:");
            Console.WriteLine(string.Join(", ", seq1));
        }
    }
}
