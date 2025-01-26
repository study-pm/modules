namespace _02_LambdaExp
{
    internal class Program
    {
        static void Main(string[] args)
        {

            // Преобразовать массив строк в массив типа int и отсортировать по возрастанию
            string[] numberStrings = { "40", "2012", "176", "5" };

            int[] nums = numberStrings.Select(s => Int32.Parse(s)).OrderBy(s => s).ToArray();

            foreach (int s in nums) Console.WriteLine(s);
        }
    }
}
