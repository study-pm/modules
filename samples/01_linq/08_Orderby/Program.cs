namespace Orderby
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int[] numbers = { 3, 12, 4, 10, 34, 20, 55, -66, 77, 88, 4 };

            /* Отсортировать по возрастанию */
            Console.WriteLine("// Using query syntax");
            var ordered = from n in numbers
                         orderby n
                         select n;
            Console.WriteLine(string.Join(", ", ordered));

            Console.WriteLine();

            Console.WriteLine("// Using lambda extension syntax");
            IEnumerable<int> sorted = numbers.OrderBy(n => n);
            Console.WriteLine(string.Join(", ", sorted));

            Console.WriteLine();

            Console.WriteLine("// Sort descending using query syntax");
            var orderedDesc = from n in numbers
                          orderby n descending
                          select n;
            Console.WriteLine(string.Join(", ", orderedDesc));

            Console.WriteLine();

            Console.WriteLine("// Sort desc using lambda extension syntax");
            IEnumerable<int> sortedDesc = numbers.OrderByDescending(n => n);
            Console.WriteLine(string.Join(", ", sortedDesc));
        }
    }
}
