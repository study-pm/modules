namespace Basics
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Без LINQ
            string[] teams = { "Бавария", "Боруссия", "Реал Мадрид", "Манчестер Сити", "ПСЖ", "Барселона" };

            var selectedTeams = new List<string>();

            foreach (string s in teams)
            {
                if (s.ToUpper().StartsWith("Б"))
                    selectedTeams.Add(s);
            }
            selectedTeams.Sort();

            Console.WriteLine("No LINQ:");
            foreach (string s in selectedTeams)
                Console.WriteLine(s);

            Console.WriteLine();

            // С LINQ (синтакс запросов):
            var selectedTeams1 = from t in teams // определяем каждый объект из teams как t
                                 where t.ToUpper().StartsWith("Б") // фильтрация по критерию
                                 orderby t // упорядочиваем по возрастанию
                                 select t; // выбираем объект


            Console.WriteLine("LINQ/query syntax:");
            foreach (string s in selectedTeams1)
                Console.WriteLine(s);

            Console.WriteLine();

            // С лямбда-выражением и методом расширения (лямбда-синтакс):
            var selectedTeams2 = teams.Where(t => t.ToUpper().StartsWith("Б")).OrderBy(t => t);

            Console.WriteLine("LINQ/lambda expression syntax with extension method:");
            foreach (string s in selectedTeams2)
                Console.WriteLine(s);

            Console.WriteLine();
        }
    }
}
