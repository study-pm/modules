# МДК 01.01 Разработка программных модулей
[671399405040133e8429e521](https://e-learn.petrocollege.ru/course/view.php?id=6620)

- [Общее](#общее)
- [53-02в 53-03в / Прикладное программирование: технологии C#.Net](#53-02в-53-03в--прикладное-программирование-технологии-cnet)
  - [Основы LINQ](#основы-linq)
    - [Методы расширения LINQ](#методы-расширения-linq)
    - [Лямбда-выражения](#лямбда-выражения)
    - [LINQ: Фильтрация where](#linq-фильтрация-where)
    - [LINQ: Сложные фильтры where](#linq-сложные-фильтры-where)
    - [LINQ: проекция select](#linq-проекция-select)
    - [LINQ: переменные и `let`](#linq-переменные-и-let)
    - [LINQ: выборка из нескольких источников](#linq-выборка-из-нескольких-источников)
    - [LINQ: сортировка orderby](#linq-сортировка-orderby)
    - [LINQ: сортировка сложных объектов](#linq-сортировка-сложных-объектов)
    - [LINQ: множественные критерии сортировки](#linq-множественные-критерии-сортировки)

## Общее
[6720ad8d5040133e8429e595](https://e-learn.petrocollege.ru/course/view.php?id=6620#section-0)

## 53-02в 53-03в / Прикладное программирование: технологии C#.Net
[6720ae035040133e8429e596](https://e-learn.petrocollege.ru/course/view.php?id=6620#section-2)

### Основы LINQ
[6720af0b5040133e8429e597](https://e-learn.petrocollege.ru/mod/resource/view.php?id=302824)

*[LINQ]: Language-Integrated Query
<dfn title="LINQ">LINQ</dfn> (Language-Integrated Query) — технология Microsoft, предназначенная для поддержки запросов к данным всех типов на уровне языка.

Источник данных реализует интерфейс `IEnumerable` (коллекции, массивы, набор данных `DataSet`, документ XML и т.д.).

Вне зависимости от типа источника LINQ позволяет применить ко всем один и тот же подход для выборки данных.

Простейшее определение запроса LINQ:
```cs
from переменная in набор_объектов select переменная;
```

*Без LINQ*:
```cs
string[ ] teams = { "Бавария", "Боруссия", "Реал Мадрид", "Манчестер Сити", "ПСЖ", "Барселона" };

var selectedTeams = new List<string>( );
foreach(string s in teams)
{
    if (s.ToUpper( ).StartsWith("Б"))
        selectedTeasm.Add(s);
}
selectedTeams.Sort( );

foreach (string s in selectedTeams)
    Console.WriteLine(s);
```

*С LINQ*:
```cs
string[ ] teams = { "Бавария", "Боруссия", "Реал Мадрид", "Манчестер Сити", "ПСЖ", "Барселона" };

var selectedTeams = from t in teams // определяем каждый объект из teams как t
                    where t.ToUpper( ).StartsWith("Б") // фильтрация по критерию
                    orderby t // упорядочиваем по возрастанию
                    select t; // выбираем объект

foreach (string s in selectedTeams)
    Console.WriteLine(s);
```

*С лямбда-выражением и методом расширения*:
```cs
var selectedTeams = teams.Where(t => t.ToUpper( ).StartsWith("Б")).OrderBy(t => t);
```

#### Методы расширения LINQ
- **`Select`**: определяет проекцию выбранных значений

- **`Where`**: определяет фильтр выборки

- **`OrderBy`**: упорядочивает элементы по возрастанию

- **`OrderByDescending`**: упорядочивает элементы по убыванию

- **`ThenBy`**: задает дополнительные критерии для упорядочивания элементов возрастанию

- **`ThenByDescending`**: задает дополнительные критерии для упорядочивания элементов по убыванию

- **`Join`**: соединяет две коллекции по определенному признаку

- **`Aggregate`**: применяет к элементам последовательности агрегатную функцию, которая сводит их к одному объекту

- **`GroupBy`**: группирует элементы по ключу

- **`ToLookup`**: группирует элементы по ключу, при этом все элементы добавляются в словарь

- **`GroupJoin`**: выполняет одновременно соединение коллекций и группировку элементов по ключу

- **`Reverse`**: располагает элементы в обратном порядке

- **`All`**: определяет, все ли элементы коллекции удовлетворяют определенному условию

- **`Any`**: определяет, удовлетворяет хотя бы один элемент коллекции определенному условию

- **`Contains`**: определяет, содержит ли коллекция определенный элемент

- **`Distinct`**: удаляет дублирующиеся элементы из коллекции

- **`Except`**: возвращает разность двух коллекцию, то есть те элементы, которые создаются только в одной коллекции

- **`Union`**: объединяет две однородные коллекции

- **`Intersect`**: возвращает пересечение двух коллекций, то есть те элементы, которые встречаются в обоих коллекциях

- **`Count`**: подсчитывает количество элементов коллекции, которые удовлетворяют определенному условию

- **`Sum`**: подсчитывает сумму числовых значений в коллекции

- **`Average`**: подсчитывает среднее значение числовых значений в коллекции

- **`Min`**: находит минимальное значение

- **`Max`**: находит максимальное значение

- **`Take`**: выбирает определенное количество элементов

- **`Skip`**: пропускает определенное количество элементов

- **`TakeWhile`**: возвращает цепочку элементов последовательности, до тех пор, пока условие истинно

- **`SkipWhile`**: пропускает элементы в последовательности, пока они удовлетворяют заданному условию, и затем возвращает оставшиеся элементы

- **`Concat`**: объединяет две коллекции

- **`Zip`**: объединяет две коллекции в соответствии с определенным условием

- **`First`**: выбирает первый элемент коллекции

- **`FirstOrDefault`**: выбирает первый элемент коллекции или возвращает значение по умолчанию

- **`Single`**: выбирает единственный элемент коллекции, если коллекция содержит больше или меньше одного элемента, то генерируется исключение

- **`SingleOrDefault`**: выбирает единственный элемент коллекции. Если коллекция пуста, возвращает значение по умолчанию. Если в коллекции больше одного элемента, генерирует исключение

- **`ElementAt`**: выбирает элемент последовательности по определенному индексу

- **`ElementAtOrDefault`**: выбирает элемент коллекции по определенному индексу или возвращает значение по умолчанию, если индекс вне допустимого диапазона

- **`Last`**: выбирает последний элемент коллекции

- **`LastOrDefault`**: выбирает последний элемент коллекции или возвращает значение по умолчанию

#### Лямбда-выражения
<dfn title="лямбда-выражение">Лямбда-выражения</dfn> — разделенный запятыми список параметров, за которым следует лямбда-операция (в C# `=>`), а за ней — выражение или блок операторов:
```
(параметр1, параметр2, параметр3) => выражение
```

```cs
x => x // ввод x, возвращает x

(x, y) => x == y // ввод x, y
// возвращает результат x == y
```

Может применяться блок операторов:
```cs
(параметр1, параметр2, параметр3) =>
{
  оператор1;
  оператор2;
  оператор3;
  return(тип_возврата_лямбда_выражения);
}
```

```cs
(x, y) =>
{
  if (x > y)
    return (x);
  else
    return (y);
}
```

*Преобразовать массив строк в массив типа `int` и отсортировать по возрастанию*:
```cs
string[ ] numbers = { "40", "2012", "176", "5" };

int[ ] nums = numbers.Select(s => Int32.Parse(s)).OrderBy(s => s).ToArray( );

foreach (int i in nums)
  Console.WriteLine(i);
```

<details>
<summary><em>Output</em></summary>

```
5
40
176
2012
```

</details>

#### LINQ: Фильтрация where
*Выбрать все четные элементы, которые больше 10*:
```cs
int[ ] numbers = { 1, 2, 3, 4, 10, 34, 55, 66, 77, 88 };

IEnumerable<int> evens =  from i in numbers
                          where i % 2 == 0 && i > 10
                          select i;

foreach (int i in evens)
  Console.WriteLine(i);

```

<details>
<summary><em>Output</em></summary>

```
34
66
88
```

</details>

*С помощью метода расширения*:
```cs
int[ ] numbers = { 1, 2, 3, 4, 10, 34, 55, 66, 77, 88 };

IEnumerable<int> evens =  numbers.Where(i => i % 2 == 0 && i > 10);

foreach (int i in evens)
  Console.WriteLine(i);

```

*Выбрать все имена, длина которых меньше 6*:
```cs
string[ ] names = { "Adams", "Arthur", "Buchanan", "Bush", "Carter", "Cleveland", "Clinton", "Coolidge", "Eisenhower", "Fillmore", "Ford", "Garfield", "Grant", "Harding", "Harrison", "Hayes", "Hoover", "Jackson", "Jefferson", "Johnson", "Kennedy", "Lincoln", "Madison", "McKinley", "Monroe", "Nixon", "Obama", "Pierce", "Polk", "Reagan", "Roosevelt", "Taft", "Taylor", "Truman", "Tyler", "Van Buren", "Washington", "Wilson" };

IEnumerable<string> sequence =  from n in names
                                where n.Length < 6
                                select n;
```

<details>
<summary><em>Output</em></summary>

```
Adams
Bush
Ford
Grant
Hayes
Nixon
Obama
Polk
Taft
Tyler
```

</details>

*С помощью метода расширения*:
```cs
string[ ] names = { "Adams", "Arthur", "Buchanan", "Bush", "Carter", "Cleveland", "Clinton", "Coolidge", "Eisenhower", "Fillmore", "Ford", "Garfield", "Grant", "Harding", "Harrison", "Hayes", "Hoover", "Jackson", "Jefferson", "Johnson", "Kennedy", "Lincoln", "Madison", "McKinley", "Monroe", "Nixon", "Obama", "Pierce", "Polk", "Reagan", "Roosevelt", "Taft", "Taylor", "Truman", "Tyler", "Van Buren", "Washington", "Wilson" };

IEnumerable<string> sequence =  names
                                .Where(n => n.Length < 6)
                                .Select(n => n);
```

#### LINQ: Сложные фильтры where
*Выбрать всех пользователей старше 25 лет*
```cs
class User
{
  public string Name { get; set; }
  public int Age { get; set; }
  public List<string> Languages { get; set; }
  public User()
  {
    Languages = new List<string>( );
  }
}

List<User> users = new List<User>
{
  new User { Name="Том", Age=23, Lanugages = new List<string> {"английский", "немецкий"} },
  new User { Name="Боб", Age=27, Lanugages = new List<string> {"английский", "французский"} },
  new User { Name="Джон", Age=29, Lanugages = new List<string> {"английский", "испанский"} },
  new User { Name="Элис", Age=24, Lanugages = new List<string> {"испанский", "немецкий"} }
}

var selectedUsers = from user in users
                    where user.Age > 25
                    select user;

foreach (User user in selectedUsers)
  Console.WriteLine($"{user.Name} - {user.Age}");
```

<details>
<summary><em>Output</em></summary>

```
Боб - 27
Джон - 29
```

</details>

*С помощью метода расширения*:
```cs
var selectedUsers = users.Where(u => u.Age > 25);
```

*Отфильтровать пользователей по языку и возрасту*:
```cs
var selectedUsers = from user in users
                    from lang in user.Languages
                    where user.Age < 28
                    where lang == "английский"
                    select user;
```

<details>
<summary><em>Output</em></summary>

```
Том - 23
Боб - 27
```

</details>

*С помощью метода расширения*:
```cs
var selectedUsers = users.SelectMany(u => u.Languages,
                    (u, l) => new { User = u, Lang = l })
                    .Where(u => u.Lang = "английский" && u.User.Age < 28)
                    .Select(u => u.User);

```

**`SelectMany`** принимает последовательность, которую надо проецировать, и функцию преобразования, которая применяется к каждому элементу; возвращает 8 пар "пользователь - язык" (`new { User = u, Lang = l }`), к которым потом применяется фильтр `Where`.

#### LINQ: проекция select
Проекция позволяет получить из текущего типа выборки какой-то другой тип.

*Получить список только имён*:
```cs
var names = from u in users select u.Name;

foreach (string n in names)
  Console.WriteLine(n);
```

<details>
<summary><em>Output</em></summary>

```
Том
Боб
Джон
Элис
```

</details>

*Получить список из имени и года рождения*:
```cs
var items = from u in users
            select new
            {
              FirstName = u.Name,
              DateOfBirth = DateTime.Now.Year - u.Age
            };

foreach (var n in items)
  Console.WriteLine($"{n.FirstName} - {n.DateOfBirth}");
```

<details>
<summary><em>Output</em></summary>

```
Том - 1996
Боб - 1992
Джон - 1990
Элис - 1995
```

</details>

*С помощью метода расширения*:
```cs
var names = users.Select(u => u.Name);

var items = users.Select(u => new
{
  FirstName = u.Name,
  DateOfBirth = DateTime.Now.Year - u.Age
});
```

#### LINQ: переменные и `let`
Переменные для промежуточных вычислений.

*Список имен с приставкой Mr.*:
```cs
var people =  from u in users
              let name = "Mr." + u.Name
              select new
              {
                Name = name,
                Age = u.Age
              };

foreach (var p in people)
  Console.WriteLine(p.Name);
```

<details>
<summary><em>Output</em></summary>

```
Mr. Том
Mr. Боб
Mr. Джон
Mr. Элис
```

</details>

С помощью метода расширения нельзя :(

#### LINQ: выборка из нескольких источников
```cs
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

List<User> users = new List<User>( )
{
  new User { Name = "Sam", Age = 43 },
  new User { Name = "Tom", Age = 33 }
};
List<Phone> phones = new List<Phone>( )
{
  new Phone {Name="Lumia 630", Company="Microsoft"},
  new Phone {Name="iPhone 6", Company="Apple"}
};

var people =  from user in users
              from phone in phones
              select new { Name = user.Name, Phone = phone. Name };

foreach (var p in people)
  Console.WriteLine($"{p.Name} = {p.Phone}");
```

<details>
<summary><em>Output</em></summary>

```
Sam - Lumia 630
Sam - iPhone 6
Tom - Lumia 630
Tom - iPhone 6
```

</details>

#### LINQ: сортировка orderby
Оператор `orderby` принимает критерий сортировки (по умолчанию — по возрастанию):

- **`ascending`** — по возрастанию;
- **`descending`** — по убыванию.

Отсортировать по возрастанию:
```cs
int [] numbers = { 3, 12, 4, 10, 34, 20, 55, -66, 77, 88, 4 };
var orderedNumbers =  from i in numbers
                      orderby i
                      select i;

foreach (in i in orderedNumbers)
  Console.WriteLine(i);
```

<details>
<summary><em>Output</em></summary>

```
-66
3
4
4
10
12
20
34
55
77
88
```

</details>

*С помощью метода расширения*:
```cs
IEnumerable<int> sortedNumbers = numbers.OrderBy(i => i);
```

*По убыванию*:
```cs
var orderedNumbers =  from i in numbers
                      orderby i descending
                      select i;
```

#### LINQ: сортировка сложных объектов
*Отсортировать по возрастанию*:
```cs
List<User> users = new List<User>()
{
  new User { Name = "Tom", Age = 33 },
  new User { Name = "Bob", Age = 30 },
  new User { Name = "Tom", Age = 21 },
  new User { Name = "Sam", Age = 43 }
};

var sortedUsers = from u in users
                  orderby u.Name
                  select u;

foreach (User u in sortedUsers)
  Console.WriteLine("{0} - {1}", u.Name, u.Age);
```

<details>
<summary><em>Output</em></summary>

```
Bob - 30
Sam - 43
Tom - 33
Tom - 21
```

</details>

*С помощью метода расширения*:
```cs
var sortedUsers = users.OrderBy(u => u.Name);
```

#### LINQ: множественные критерии сортировки
*Отсортировать по возрастанию*:
```cs
List<User> users = new List<User>()
{
  new User { Name = "Tom", Age = 33 },
  new User { Name = "Bob", Age = 30 },
  new User { Name = "Tom", Age = 21 },
  new User { Name = "Sam", Age = 43 }
};

var sortedUsers = from user in users
                  orderby user.Name, user.Age
                  select user;

foreach (User u in sortedUsers)
  Console.WriteLine("{0} - {1}", u.Name, u.Age);
```

<details>
<summary><em>Output</em></summary>

```
Bob - 30
Sam - 43
Tom - 21
Tom - 33
```

</details>

*С помощью метода расширения*:
```cs
var sortedUsers = users.OrderBy(u => u.Name).ThenBy(u => u.Age);
```
