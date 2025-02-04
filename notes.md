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
  - [Лекция Подключение БД](#лекция-подключение-бд)
    - [Строка подключения для MS SQL Server](#строка-подключения-для-ms-sql-server)
    - [Получение информации о подключении](#получение-информации-о-подключении)
  - [Пример подключения](#пример-подключения)
  - [Работа с бд](#работа-с-бд)
    - [Руководство по стилю](#руководство-по-стилю)
      - [Общие требования](#общие-требования)
      - [Использование логотипа](#использование-логотипа)
      - [Шрифт](#шрифт)
      - [Цветовая схема](#цветовая-схема)
    - [Ресурсы](#ресурсы)
      - [Данные](#данные)
  - [Авторизация WPF / Реализация авторизации и регистрации пользователя в WPF-приложении (C#) к базе данных MS SQL Server](#авторизация-wpf--реализация-авторизации-и-регистрации-пользователя-в-wpf-приложении-c-к-базе-данных-ms-sql-server)
    - [Создание базы данных и подключение ее к проекту](#создание-базы-данных-и-подключение-ее-к-проекту)
      - [Создание БД](#создание-бд)
      - [Создание модели БД](#создание-модели-бд)
      - [Подключение к БД](#подключение-к-бд)
      - [Интерфейс приложения (главное окно)](#интерфейс-приложения-главное-окно)
      - [Добавление страниц](#добавление-страниц)
        - [Страница авторизации](#страница-авторизации)
      - [Функционал авторизации](#функционал-авторизации)
  - [Задание на авторизацию и создание бд](#задание-на-авторизацию-и-создание-бд)
  - [Регистрация WPF](#регистрация-wpf)
    - [Реализация регистрации пользователя в WPF-приложении (C#) к базе данных MS SQL Server (практическая работа)](#реализация-регистрации-пользователя-в-wpf-приложении-c-к-базе-данных-ms-sql-server-практическая-работа)
      - [Страница регистрации](#страница-регистрации)
      - [Функционал прохождения регистрации ученика](#функционал-прохождения-регистрации-ученика)
  - [Задание Регистрация WPF](#задание-регистрация-wpf)
  - [Данные](#данные-1)
  - [Вывод данных](#вывод-данных)
  - [Фильтрация, поиск, сортировка](#фильтрация-поиск-сортировка)

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

[Code Example](./samples/01_LINQ/01_Basics/Program.cs)

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

*[Преобразовать массив строк в массив типа `int` и отсортировать по возрастанию](./samples/01_LINQ/02_LambdaExp/Program.cs)*:
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

[Example Code](./samples/01_LINQ/03_Where/Program.cs)

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
  new User { Name="Том", Age=23, Languages = new List<string> {"английский", "немецкий"} },
  new User { Name="Боб", Age=27, Languages = new List<string> {"английский", "французский"} },
  new User { Name="Джон", Age=29, Languages = new List<string> {"английский", "испанский"} },
  new User { Name="Элис", Age=24, Languages = new List<string> {"испанский", "немецкий"} }
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
                    .Where(u => u.Lang == "английский" && u.User.Age < 28)
                    .Select(u => u.User);

```

**`SelectMany`** принимает последовательность, которую надо проецировать, и функцию преобразования, которая применяется к каждому элементу; возвращает 8 пар "пользователь - язык" (`new { User = u, Lang = l }`), к которым потом применяется фильтр `Where`.

[Example Code](./samples/01_LINQ/04_WhereComplex/Program.cs)

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

[Example Code](./samples/01_LINQ/05_Select/Program.cs)

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

[Example Code](./samples/01_LINQ/06_Let/Program.cs)

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
              select new { Name = user.Name, Phone = phone.Name };

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

[Example Code](./samples/01_LINQ/07_MultiSrc/Program.cs)

#### LINQ: сортировка orderby
Оператор `orderby` принимает критерий сортировки (по умолчанию — по возрастанию):

- **`ascending`** — по возрастанию;
- **`descending`** — по убыванию.

*Отсортировать по возрастанию*:
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

[Example Code](./samples/01_LINQ/08_Orderby/Program.cs)

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

[Example Code](./samples/01_LINQ/09_MultiCriteria/Program.cs)

### Лекция Подключение БД
[6720d6c15040133e8429e59c](https://e-learn.petrocollege.ru/mod/lesson/view.php?id=302825)

SQL Server — одна из наиболее популярных систем управления базами данных, а при работе с фреймворком .NET, возможно, наиболее часто выбираемая СУБД. И в этой части руководства рассмотрим подключение к MS SQL Server.

#### Строка подключения для MS SQL Server
Для работы с MS SQL Server естественно нам потребуется MS SQL Server. Можно выбрать как полноценный MS SQL Server (в весиях Enterprise, Developer), так и MS SQL Server Express.

Про установку MS SQL Server в выпусках Developer или Express можно почитать в статье [Установка MS SQL Server 2019](https://metanit.com/sql/sqlserver/1.2.php).

Также можно использовать специально предназначенный для целей разработки и тестирования легковесный движок MS SQL Server Express LocalDB, про установку которого можно почитать в статье [Установка SQL Server Express LocalDB](https://metanit.com/sql/sqlserver/1.4.php).

Вначале необходимо определить строку подключения, которая содержит набор параметров сервера MS SQL Server. Строка подключения представляет набор параметров в виде пар `ключ=значение`, которые отделяются друг от друга точкой с запятой.

Прежде всего, определение строки подключения зависит от типа подключения: либо мы подлючаемся по логину и паролю, либо мы используем доверенное подключение (trusted connection), где не требуются логин и пароль (например, при подключении к локальному серверу SQL Server).

Если подключение производится по логину и паролю, то общий вид строки подключения выглядит следующим образом:

```cs
Server=адрес_сервера;Database=имя_базы_данных;User Id=логин;Password=пароль;
```

В данном случае строка подключения состоит из четырех параметров:

- `Server`: указывает на название сервера

- `Database`: указывает на название базы данных на сервере

- `User Id`: логин

- `Password`: пароль

Если мы используем так называемое доверенное подключение (trusted connection) и применяем аутентификацию Windwows, например, при подключении к локальному серверу, который запущен на том же компьютере, то строка подключения в общем виде выглядит следующим образом:

```cs
Server=адрес_сервера;Database=имя_базы_данных;Trusted_Connection=True;
```

Вместо параметров `User Id` и `Password`, здесь применяется параметр `Trusted_Connection=True`. Значение `True` указывает, что будет применяться аутентификация на основе учетных записей Windows.

Список основных параметров строки подключения, которые могут использоваться:

- `Application Name`: название приложения. Может принимать в качестве значения любую строку. Значение по умолчанию: ".Net SqlClient Data Provide"

- `AttachDBFileName`: хранит полный путь к прикрепляемой базе данных

- `Connect Timeout`: временной период в секундах, через который ожидается установка подключения. Принимает одно из значений из интервала 0–32767. По умолчанию равно 15.

  В качестве альтернативного названия параметра может использоваться `Connection Timeout`

- `Server`: название экземпляра SQL Servera, с которым будет идти взаимодействие. Это может быть название локального сервера, например, "./SQLEXPRESS", "localhost", либо сетевой адрес.

  В качестве альтернативного названия параметра можно использовать `Data Source`, `Address`, `Addr` и `NetworkAddress`

- `Encrypt`: устанавливает шифрование SSL при подключении. Может принимать значения `true`, `false`, `yes` и `no`. По умолчанию значение `false`

- `Database`: хранит имя базы данных

  В качестве альтернативного названия параметра можно использовать `Initial Catalog`

- `Trusted_Connection`: задает режим аутентификации. Может принимать значения `true`, `false`, `yes`, `no` и `sspi`. По умолчанию значение `false`

  Если значение `true`, то для аутентификации будет использоваться текущая учетная запись Windows. Подходит для подключения к локальному серверу.

  В качестве альтернативного названия параметра может использоваться `Integrated Security`

- `Packet Size`: размер сетевого пакета в байтах. Может принимать значение, которое кратно 512. По умолчанию равно 8192

- `Persist Security Info`: указывает, должна ли конфиденциальная информация передаваться обратно при подключении. Может принимать значения `true`, `false`, `yes` и `no`. По умолчанию значение `false`

- `Pooling`: если значение равно `true`, любое новое подключение при его закрытии добавляется в пул подключений. В следующий раз при создании такого же подключения (которое имеет ту же самую строку подключения) оно будет извлекаться из пула. Может принимать значения `true`, `false`, `yes` и `no`. По умолчанию значение `true`

- `Workstation ID`: указывает на рабочую станцию — имя локального компьютера, на котором запущен SQL Server

- `Password`: пароль пользователя

- `User ID`: логин пользователя

В данном случае мы будем использовать к локальному серверу. Если мы подключаемся к полноценному серверу MS SQL Server (например, версия Developer Edition), то в качестве адреса сервера, как правило, выступает `localhost`:

```cs
string connectionString = "Server=localhost;Database=master;Trusted_Connection=True;";
// альтернатива
// string connectionString = "Server=.;Database=master;Trusted_Connection=True;";
```

Если установлен MS SQL Server Express, то адрес сервера — ".\SQLEXPRESS"

```cs
string connectionString = "Server=.\SQLEXPRESS;Database=master;Trusted_Connection=True;";
```

Для подключения к localdb применяется адрес `(localdb)\mssqllocaldb`:

```cs
string connectionString = "Server=(localdb)\\mssqllocaldb;Database=master;Trusted_Connection=True"
```

Для работы с базой данных MS SQL Server в .NET 5 и выше (а также .NET Core 3.0/3.1) необходимо установить в проект через nuget пакет Microsoft.Data.SqlClient:

![Picture 1.1](./img/671399405040133e8429e521-1.1.png)

<details>
<summary><em>How to install SQLClient</em></summary>

1. "Open Manage NuGet Packages..." from the context menu of Solution Explorer.

    ![Manage NuGet Packages](./img/manage-nuget-packages.png)

2. Go "Browse" tab and put the "SqlClient" string into the search box.

    ![Search for SQL Client](./img/search-sql-client.png)

3. Choose between the "Microsoft.Data.SqlClient" (preferred) or "System.Data.SqlClient" packages, click the download arrow and proceed to the installation process accepting all the agreements.

Пространство имен `Microsoft.Data.SqlClient` по сути является новой версией пространства имен `System.Data.SqlClient`. `Microsoft.Data.SqlClient` обычно поддерживает те же API и обратную совместимость, что и `System.Data.SqlClient`. Для большинства приложений переход с `System.Data.SqlClient` на `Microsoft.Data.SqlClient` не составляет проблем. Добавьте зависимость NuGet в `Microsoft.Data.SqlClient`, после чего обновите ссылки и инструкции using в `Microsoft.Data.SqlClient`.

В этой версии пространства имен есть несколько различий в менее используемых API по сравнению с `System.Data.SqlClient`, которые могут повлиять на некоторые приложения. Для этих различий обратитесь к полезному памятку по переносу.[^introduction-microsoft-data-sqlclient-namespace]

[^introduction-microsoft-data-sqlclient-namespace]: [Введение в пространство имен Microsoft.Data.SqlClient](https://learn.microsoft.com/ru-ru/sql/connect/ado-net/introduction-microsoft-data-sqlclient-namespace?view=sql-server-ver16)

</details>

Для создания подключения к MS SQL Server применяется класс `SqlConnection` из пространства имен `Microsoft.Data.SqlClient`.

Этот класс имеет три конструктора:

```cs
SqlConnection()
SqlConnection(String)
SqlConnection(String, SqlCredential)
```

Второй и третий конструкторы в качестве первого параметра принимают строку подключения. Третий конструктор также принимает объект `SqlCredential`, который фактически представляет логин и пароль.

Теперь проверим подключение на примере сервера `LocalDB`:
```cs
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Threading.Tasks;

namespace HelloApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string connectionString = "Server=(localdb)\\mssqllocaldb;Database=master;Trusted_Connection=True;";

            // Создание подключения
            SqlConnection connection = new SqlConnection(connectionString);
            try
            {
                // Открываем подключение
                await connection.OpenAsync();
                Console.WriteLine("Подключение открыто");
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                // если подключение открыто
                if (connection.State == ConnectionState.Open)
                {
                    // закрываем подключение
                    await connection.CloseAsync();
                    Console.WriteLine("Подключение закрыто...");
                }
            }
            Console.WriteLine("Программа завершила работу.");
            Console.Read();
        }
    }
}
```

В данном случае подключение осуществляется к серверу `LocalDB` и его базе данных `master` (по умолчанию база данных `master` уже должна быть на любом MS SQL Servere).

Для начала взаимодействия с базой данных нам надо открыть подключение с помощью методов `Open()` (синхронный) или `OpenAsync()` (асинхронный).

По окончании работы с `SqlConnection` необходимо закрыть подключение к серверу, вызвав метод `Close()`/`CloseAsync()` или `Dispose()`/`DisposeAsync()`. В данном случае вначале проверяем, что подключение открыто и, если оно открыто, вызываем асинхронный метод `OpenAsync()`.

В итоге, если указана валидная строка подключения, то мы должны увидеть на консоли следующие строки:
```
Подключение открыто
Подключение закрыто...
Программа завершила работу.
```

Вместо явного закрытия подключения также можно использовать конструкцию `using`, которая автоматически закрывает подключение:
```cs
using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

namespace HelloApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string connectionString = "Server=(localdb)\\mssqllocaldb;Database=master;Trusted_Connection=True;";
            using(SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                Console.WriteLine("Подключение открыто");
            }
            Console.WriteLine("Подключение закрыто...");
            Console.WriteLine("Программа завершила работу.");
            Console.Read();
        }
    }
}
```

#### Получение информации о подключении
Объект `SqlConnection` обладает рядом свойств, которые позволяют получить информацию о подключении:
```cs
using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

namespace HelloApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string connectionString = "Server=(localdb)\\mssqllocaldb;Database=master;Trusted_Connection=True;";

            using(SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                Console.WriteLine("Подключение открыто");
                // Вывод информации о подключении
                Console.WriteLine("Свойства подключения:");
                Console.WriteLine($"\tСтрока подключения: {connection.ConnectionString}");
                Console.WriteLine($"\tБаза данных: {connection.Database}");
                Console.WriteLine($"\tСервер: {connection.DataSource}");
                Console.WriteLine($"\tВерсия сервера: {connection.ServerVersion}");
                Console.WriteLine($"\tСостояние: {connection.State}");
                Console.WriteLine($"\tWorkstationld: {connection.WorkstationId}");
            }
            Console.WriteLine("Подключение закрыто...");
            Console.WriteLine("Программа завершила работу.");
            Console.Read();
        }
    }
}
```

```
Подключение открыто
Свойства подключения:
	Строка подключения:
	База данных: master
	Сервер: (localdb)\mssqllocaldb
	Версия сервера: 15.00.2000
	Состояние: Open
	WorkstationId: EUGENEPC
Подключение закрыто...
Программа завершила работу.
```

### Пример подключения
[679511615040133e8429ecc9](https://e-learn.petrocollege.ru/mod/page/view.php?id=302826)

```cs
SqlConnection myConnection = new SqlConnection("Server = pcsqlstud01;database = 10171655;  Integrated Security=True;");

try
{
    myConnection.Open();
    SqlCommand myCommand = new SqlCommand("Select * from Cars", myConnection);
    string selectquery = "Select * from Cars";
    SqlDataAdapter adpt = new SqlDataAdapter(selectquery, myConnection);

    DataTable table = new DataTable();
    adpt.Fill(table);
    dataGridView1.DataSource = table;
}
catch (Exception e)
{
    Console.WriteLine(e.ToString());
}

```

### Работа с бд
[679618625040133e8429eccd](https://e-learn.petrocollege.ru/mod/assign/view.php?id=302827)

ООО «Стройматериалы» — магазин по продаже строительных материалов в Сыктывкаре.

Даны данные о товарах. Необходимо разработать бд и приложение, которое будет выводить данные о товарах, изменять, удалять и добавлять новые товары.

Визуальные компоненты должны соответствовать руководству по стилю, предоставленному в качестве ресурсов к заданию в соответствующем файле. Обеспечьте соблюдение требований всех компонентов в следующих областях:

- цветовая схема,

- размещение логотипа,

- использование шрифтов,

- установка иконки приложения.

Все компоненты системы должны иметь единый согласованный внешний вид, соответствующий руководству по стилю, а также следующим требованиям:

- разметка и дизайн (предпочтение отдается масштабируемой компоновке;

- должно присутствовать ограничение на минимальный размер окна;

- должна присутствовать возможность изменения размеров окна, где это необходимо;

- увеличение размеров окна должно увеличивать размер контентной части, например, таблицы с данными из БД);

- группировка элементов (в логические категории);

- использование соответствующих элементов управления (например, выпадающих            списков для отображения подстановочных значений из базы данных);

- расположение и выравнивание элементов (метки, поля для ввода и т.д.);

- последовательный переход фокуса по элементам интерфейса (по нажатию клавиши <kbd>TAB</kbd>);

- общая компоновка логична, понятна и проста в использовании;

- последовательный пользовательский интерфейс, позволяющий перемещаться между существующими окнами в приложении (в том числе обратно, например, с помощью кнопки «Назад»);

- соответствующий заголовок на каждом окне приложения (не должно быть значений по умолчанию типа `MainWindow`, `Form1` и тп).

Уведомляйте пользователя о совершаемых им ошибках или о запрещенных в рамках задания действиях, запрашивайте подтверждение перед удалением, предупреждайте о неотвратимых операциях, информируйте об отсутствии результатов поиска и т.п. Окна сообщений соответствующих типов (например, ошибка, предупреждение, информация) должны отображаться с соответствующим заголовком и пиктограммой. Текст сообщения должен быть полезным и информативным, содержать полную информацию о совершенных ошибках пользователя и порядок действий для их исправления. Также можно использовать визуальные подсказки для пользователя при вводе данных.

Не позволяйте пользователю вводить некорректные значения в текстовые поля сущностей. Например, в случае несоответствия типа данных или размера поля введенному значению. Оповестите пользователя о совершенной им ошибке.

Обратите внимание на использование абсолютных и относительных путей к изображениям. Приложение должно корректно работать, в том числе и при перемещении папки с исполняемым  файлом.

При возникновении непредвиденной ошибки приложение не должно аварийно завершать работу.

Идентификаторы переменных, методов и классов должны отражать суть и/или цель их использования, в том числе и наименования элементов управления (например, не должно быть значений по умолчанию типа `Form1`, `button3`).

Идентификаторы должны соответствовать соглашению об именовании (Code Convention) и стилю CamelCase (для C#).

Допустимо использование не более одной команды в строке.

#### Руководство по стилю

##### Общие требования
При создании приложения руководствуйтесь требованиями, описанными в документе «Требования и рекомендации.pdf». Не допускайте орфографические и грамматические ошибки.

##### Использование логотипа
Все экранные формы пользовательского интерфейса должны иметь заголовок с логотипом (в ресурсах). Не искажайте логотип (не изменяйте изображение, его пропорции, цвет).

Также для приложений должна быть установлена иконка.

##### Шрифт
Используйте шрифт *Comic Sans MS*.

##### Цветовая схема
В	качестве основного фона используется белый цвет; в качестве дополнительного: RGB (118, 227, 131).

Для акцентирования внимания пользователя на целевое действие интерфейса используйте цвет RGB (73, 140,81).

<table>
<thead>
  <tr>
    <th>Основной фон</th><th>Дополнительный фон</th><th>Акцентирование внимания</th>
  </tr>
<thead>
<tbody>
  <tr>
    <td>RGB</td><td>RGB</td><td>RGB</td>
  </tr>
  <tr>
    <td>(255, 255, 255)</td><td>(118, 227, 131)</td><td>(73, 140,81)</td>
  </tr>
</tbody>
<tfoot>
  <tr>
    <td style="height: 20px; background: rgb(255, 255, 255)"></td>
    <td style="height: 20px; background: rgb(118, 227, 131)"></td>
    <td style="height: 20px; background: rgb(73, 140,81)"></td>
  </tr>
</tfoot>
</table>

#### Ресурсы
Все необходимые ресурсы находятся по следующей ссылке:
- [Ресурсы](./res/00%2053-02в%2053-03в/03%20Работа%20с%20бд%20-%20Задание/)

##### Данные

| Артикул | Наименование                          | Единица измерения | Стоимость | Размер максимально возможной скидки | Производитель     | Поставщик         | Категория товара                         | Действующая скидка | Кол-во на складе | Описание                                                                                                    | Изображение |
| ------- | -------------------------------------- | ----------------- | --------- | ----------------------------------- | ----------------- | ----------------- | ---------------------------------------- | ------------------ | ---------------- | ----------------------------------------------------------------------------------------------------------- | ----------- |
| PMEZMH  | Цемент                                 | шт.               | 440       | 10                                  | М500              | М500              | Общестроительные материалы               | 8                  | 34               | Цемент Евроцемент М500 Д0 ЦЕМ I 42,5 50 кг                                                                  | PMEZMH.jpg  |
| BPV4MM  | Пленка техническая                     | шт.               | 8         | 13                                  | Изостронг         | Изостронг         | Общестроительные материалы               | 8                  | 2                | Пленка техническая полиэтиленовая Изостронг 60 мк 3 м рукав 1,5 м, пог.м                                    | BPV4MM.jpg  |
| JVL42J  | Пленка техническая                     | шт.               | 13        | 1                                   | Изостронг         | Изостронг         | Общестроительные материалы               | 4                  | 34               | Пленка техническая полиэтиленовая Изостронг 100 мк 3 м рукав 1,5 м, пог.м                                   | JVL42J.jpg  |
| F895RB  | Песок строительный                     | шт.               | 102       | 17                                  | Knauf             | Knauf             | Общестроительные материалы               | 6                  | 7                | Песок строительный 50 кг                                                                                    | F895RB.jpg  |
| 3XBOTN  | Керамзит фракция                       | шт.               | 110       | 14                                  | MixMaster         | MixMaster         | Общестроительные материалы               | 5                  | 21               | Керамзит фракция 10-20 мм 0,05 куб.м                                                                        | 3XBOTN.jpg  |
| 3L7RCZ  | Газобетон                              | шт.               | 7400      | 7                                   | ЛСР               | ЛСР               | Стеновые и фасадные материалы            | 2                  | 20               | Газобетон ЛСР 100х250х625 мм D400                                                                           | 3L7RCZ.jpg  |
| S72AM3  | Пазогребневая плита                   | шт.               | 500       | 9                                   | ВОЛМА             | ВОЛМА             | Стеновые и фасадные материалы            | 5                  | 35               | Пазогребневая плита ВОЛМА Гидро 667х500х80 мм полнотелая                                                    | S72AM3.jpg  |
| 2G3280  | Угол наружный                          | шт.               | 795       | 16                                  | Vinylon           | Vinylon           | Стеновые и фасадные материалы            | 9                  | 20               | Угол наружный Vinylon 3050 мм серо-голубой                                                                  | 2G3280.jpg  |
| MIO8YV  | Кирпич                                 | шт.               | 30        | 9                                   | ВОЛМА             | ВОЛМА             | Стеновые и фасадные материалы            | 9                  | 31               | Кирпич рядовой Боровичи полнотелый М150 250х120х65 мм 1NF                                                   | MIO8YV.jpg  |
| UER2QD  | Скоба для пазогребневой плиты          | шт.               | 25        | 20                                  | Knauf             | Knauf             | Стеновые и фасадные материалы            | 8                  | 27               | Скоба для пазогребневой плиты Knauf С1 120х100 мм                                                           | UER2QD.jpg  |
| ZR70B4  | Кирпич                                 | шт.               | 16        | 3                                   | Павловский завод | Павловский завод | Стеновые и фасадные материалы            | 3                  | 26               | Кирпич рядовой силикатный Павловский завод полнотелый М200 250х120х65 мм 1NF                                |             |
| LPDDM4  | Штукатурка гипсовая                    | шт.               | 500       | 17                                  | Knauf             | Knauf             | Сухие строительные смеси и гидроизоляция | 6                  | 38               | Штукатурка гипсовая Knauf Ротбанд 30 кг                                                                     |             |
| LQ48MW  | Штукатурка гипсовая                    | шт.               | 462       | 16                                  | Weber             | Weber             | Сухие строительные смеси и гидроизоляция | 6                  | 33               | Штукатурка гипсовая Knauf МП-75 машинная 30 кг                                                              |             |
| O43COU  | Шпаклевка                              | шт.               | 750       | 9                                   | ВОЛМА             | ВОЛМА             | Сухие строительные смеси и гидроизоляция | 1                  | 16               | Шпаклевка полимерная Weber.vetonit LR + для сухих помещений белая 20 кг                                     |             |
| M26EXW  | Клей для плитки, керамогранита и камня | шт.               | 340       | 8                                   | Knauf             | Knauf             | Сухие строительные смеси и гидроизоляция | 8                  | 2                | Клей для плитки, керамогранита и камня Крепс Усиленный серый (класс С1) 25 кг                               |             |
| K0YACK  | Смесь цементно-песчаная                | шт.               | 160       | 9                                   | MixMaster         | MixMaster         | Сухие строительные смеси и гидроизоляция | 8                  | 19               | Смесь цементно-песчаная (ЦПС) 300 по ТУ MixMaster Универсал 25 кг                                           |             |
| ASPXSG  | Ровнитель                              | шт.               | 711       | 17                                  | Weber             | Weber             | Сухие строительные смеси и гидроизоляция | 10                 | 20               | Ровнитель (наливной пол) финишный Weber.vetonit 4100 самовыравнивающийся высокопрочный 20 кг                |             |
| ZKQ5FF  | Лезвие для ножа                       | шт.               | 65        | 13                                  | Hesler            | Hesler            | Ручной инструмент                        | 6                  | 6                | Лезвие для ножа Hesler 18 мм прямое (10 шт.)                                                                |             |
| 4WZEOT  | Лезвие для ножа                       | шт.               | 110       | 2                                   | Armero            | Armero            | Ручной инструмент                        | 6                  | 17               | Лезвие для ножа Armero 18 мм прямое (10 шт.)                                                                |             |
| 4JR1HN  | Шпатель                                | шт.               | 26        | 3                                   | Hesler            | Hesler            | Ручной инструмент                        | 6                  | 7                | Шпатель малярный 100 мм с пластиковой ручкой                                                                |             |
| Z3XFSP  | Нож строительный                      | шт.               | 63        | 19                                  | Hesler            | Hesler            | Ручной инструмент                        | 8                  | 5                | Нож строительный Hesler 18 мм с ломающимся лезвием пластиковый корпус                                       |             |
| I6MH89  | Валик                                  | шт.               | 326       | 12                                  | Wenzo Roma        | Wenzo Roma        | Ручной инструмент                        | 6                  | 3                | Валик Wenzo Roma полиакрил 250 мм ворс 18 мм для красок грунтов и антисептиков на водной основе с рукояткой |             |
| 83M5ME  | Кисть                                  | шт.               | 122       | 16                                  | Armero            | Armero            | Ручной инструмент                        | 9                  | 26               | Кисть плоская смешанная щетина 100х12 мм для красок и антисептиков на водной основе                         |             |
| 61PGH3  | Очки защитные                          | шт.               | 184       | 2                                   | KILIMGRIN         | KILIMGRIN         | Защита лица, глаз, головы                | 6                  | 25               | Очки защитные Delta Plus KILIMANDJARO (KILIMGRIN) открытые с прозрачными линзами                            |             |
| GN6ICZ  | Каска защитная                        | шт.               | 154       | 10                                  | Исток             | Исток             | Защита лица, глаз, головы                | 6                  | 8                | Каска защитная Исток (КАС001О) оранжевая                                                                    |             |
| Z3LO0U  | Очки защитные                         | шт.               | 228       | 19                                  | RUIZ              | RUIZ              | Защита лица, глаз, головы                | 9                  | 11               | Очки защитные Delta Plus RUIZ (RUIZ1VI) закрытые с прозрачными линзами                                      |             |
| QHNOKR  | Маска защитная                         | шт.               | 251       | 6                                   | Исток             | Исток             | Защита лица, глаз, головы                | 2                  | 22               | Маска защитная Исток (ЩИТ001) ударопрочная и термостойкая                                                   |             |
| EQ6RKO  | Подшлемник                             | шт.               | 36        | 17                                  | Husqvarna         | Husqvarna         | Защита лица, глаз, головы                | 3                  | 22               | Подшлемник для каски одноразовый                                                                            |             |
| 81F1WG  | Каска защитная                         | шт.               | 1500      | 1                                   | Delta             | Delta             | Защита лица, глаз, головы                | 2                  | 13               | Каска защитная Delta Plus BASEBALL DIAMOND V UP (DIAM5UPBCFLBS) белая                                       |             |
| 0YGHZ7  | Очки защитные                         | шт.               | 700       | 9                                   | Husqvarna         | Husqvarna         | Защита лица, глаз, головы                | 9                  | 36               | Очки защитные Husqvarna Clear (5449638-01) открытые с прозрачными линзами                                   |             |

### Авторизация WPF / Реализация авторизации и регистрации пользователя в WPF-приложении (C#) к базе данных MS SQL Server
[679831c85040133e8429ecfa](https://e-learn.petrocollege.ru/mod/resource/view.php?id=302828)

**Используемые среды**:
- MS SQL Server Management Studio
- Microsoft Visual Studio

#### Создание базы данных и подключение ее к проекту

##### Создание БД
1. Запустите MS SQL Server Management Studio.
2. Подключитесь к серверу. Например для подключения к локальному надо ввести

   ```
   (localdb)\mssqllocaldb
   ```

3. Создайте базу данных. В поле **Имя базы данных**: задайте имя БД.

    ![Create DB](./img/679831c85040133e8429ecfa-1.png)

4. Создайте в БД две таблицы в соответствии с диаграммой (`User` и `Role`), необходимые для реализации в приложении авторизации. Задайте для них ключевое поле в виде счетчика (`Спецификация идентификатора - Да`). Все поля должны быть обязательными для заполнения.

    ![Create DB](./img/679831c85040133e8429ecfa-2.png)

5. Заполните таблицы данными. Предполагаем, что у нас будет два пользователя, один из них Администратор (Степан), другой – Ученик (Наталья). Других пользователей будем добавлять через приложение.

    ![Create DB](./img/679831c85040133e8429ecfa-3.png)

    ![Create DB](./img/679831c85040133e8429ecfa-4.png)

##### Создание модели БД
1. Создайте приложение **WPF** в MS Visual Studio.
2. Добавьте в проект в окне *Обозреватель решений* папку *ApplicationData* для размещения в ней модели базы данных.

    ![Create DB Model](./img/679831c85040133e8429ecfa-5.png)

3. В папке *ApplicationData* создайте элемент **Модель ADO.NET.EDM** (имя модели можно не менять) `Model1`. Выберите нужную модель.

    ![Create DB Model](./img/679831c85040133e8429ecfa-6.png)

4. Далее ***Создать соединение***:

    ![Create DB Model](./img/679831c85040133e8429ecfa-7.png)

5. В появившемся окне написать ***Имя сервера*** (имя сервера можно взять в MS SQL Server Management Studio по команде *Свойства* в контекстном меню сервера)

    ![Create DB Model](./img/679831c85040133e8429ecfa-8.png)

6. Выбрать вариант входа на сервер. Выбрать имя базы данных.

    ![Create DB Model](./img/679831c85040133e8429ecfa-9.png)

7. Выбрать версию Framework и на последнем шаге выбрать таблицы БД.

Добавленная модель имеет вид:

![Create DB Model](./img/679831c85040133e8429ecfa-10.png)

При изменении структуры базы данных в VS необходимо удалить таблицы модели, затем на свободном месте щелкнуть правой кнопкой мыши и выбрать команду *Обновить модель из базы данных*.

В обозревателе решений проекта отображается файл созданной модели ***Model1.edmx***.

##### Подключение к БД
1. В папку ***ApplicationData*** добавить класс **`AppConnect`**, написать в нем подключение к нашей БД.

    ![DB Connect](./img/679831c85040133e8429ecfa-11.png)

    ```cs
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace AcademyApp.ApplicationData
    {
        internal class AppConnect
        {
            public static AcademyEntities modelOdb;
        }
    }
    ```

2. Подключить ту же самую модель как объект к главному окну **`MainWindow`**.

    ![DB Connect](./img/679831c85040133e8429ecfa-12.png)

    ```cs
    using AcademyApp.ApplicationData;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;

    namespace AcademyApp
    {
        /// <summary>
        /// Interaction logic for MainWindow.xaml
        /// </summary>
        public partial class MainWindow : Window
        {
            public MainWindow()
            {
                InitializeComponent();
                AppConnect.modelOdb = new AcademyEntities();
            }
        }
    }
    ```

**На этом Подключение БД к проекту завершено!**

##### Интерфейс приложения (главное окно)
1. Организуйте разметку главного окна приложения в соответствии с рисунком:

    ![App Interface](./img/679831c85040133e8429ecfa-13.png)

    Ширину окна установите 800, высоту 350, измените текст в строке заголовка.

    Основной контейнер компоновки в окне — `Grid`, состоящий из двух строк:
    ```xml
    <Grid.RowDefinitions>
      <RowDefinition Height="100*" />
      <RowDefinition Height="350*" />
    </Grid.RowDefinitions>
    ```

    В **верхней строке сетки `Grid`** размещен контейнер компоновки `StackPanel` с горизонтальным размещением в нем двух элементов: `Image` и `TextBlock`. Изображение логотипа поместить в папку проекта ***Resources***. Стиль оформления надписи организовать в ресурсах файла ***App.xaml***, например, в таком виде (можно изменить стиль на свое усмотрение):
    ```xml
    <Style TargetType="TextBlock" x:Key="Title">
      <Setter Property="VerticalAlignment" Value="Center"/>
      <Setter Property="FontSize" Value="13pt"/>
      <Setter Property="FontWeight" Value="Bold"/>
      <Setter Property="Margin" Value="15"/>
    </Style>
    ```

    и применить к элементу **`TextBlock`**: `Style="{StaticResource Title}"`.

    В нижней строке сетки `Grid` разместить **`Frame`**. Дать ему имя ***FrmMain***, скрыть в нем панель навигации: `NavigationUIVisibility="Hidden"`.

2. Чтобы внутри данного фрейма выполнять действия (хранить и передавать информацию из страницы к странице), создадим специальный класс, и объект этого класса с именем FrmMain.

    В папку ***ApplicationData*** добавьте класс `AppFrame`:

    ![App Interface](./img/679831c85040133e8429ecfa-14.png)

    ```cs
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Controls;

    namespace AcademyApp.ApplicationData
    {
        internal class AppFrame
        {
            public static Frame frameMain;
        }
    }
    ```

    Пропишите фрейм в коде главного окна:

    ![App Interface](./img/679831c85040133e8429ecfa-15.png)

    ```cs
    using AcademyApp.ApplicationData;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;

    namespace AcademyApp
    {
        /// <summary>
        /// Interaction logic for MainWindow.xaml
        /// </summary>
        public partial class MainWindow : Window
        {
            public MainWindow()
            {
                InitializeComponent();
                AppConnect.modelOdb = new AcademyEntities();
                AppFrame.frameMain = FrmMain;
            }
        }
    }
    ```

##### Добавление страниц
Для каждой вновь добавляемой страницы приложения будем организовывать в проекте отдельную папку.

###### Страница авторизации
1. Создайте в проекте папку *PagesMain*** (***Pages***). В ней создайте страницу ***PageLogin***. Установите высоту 350, ширину 800.

   - Организуйте разметку страницы в соответствии с рисунком.
   - Используйте контейнер `StackPanel`, расположите его в центре страницы.
   - Для ввода пароля используйте компонент `PasswordBox`.
   - Для оформления кнопок добавьте еще два шаблона в ресурсы файла ***APP.xaml***.
   - Между всеми компонентами страницы создайте отступы в 5 пикселей:

   ![Auth Page](./img/679831c85040133e8429ecfa-16.png)

   ![Auth Page](./img/679831c85040133e8429ecfa-17.png)

2. Данная страница авторизации должна загружаться при запуске приложения. Для этого нужно в главном окне внутри фрейма инициализировать нашу страницу:

    ![Auth Page](./img/679831c85040133e8429ecfa-18.png)

    ```cs
    using AcademyApp.ApplicationData;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;

    namespace AcademyApp
    {
        /// <summary>
        /// Interaction logic for MainWindow.xaml
        /// </summary>
        public partial class MainWindow : Window
        {
            public MainWindow()
            {
                InitializeComponent();
                AppConnect.modelOdb = new AcademyEntities();
                AppFrame.frameMain = FrmMain;

                FrmMain.Navigate(new Pages.PageLogin());
            }
        }
    }
    ```

**Запустите проект, убедитесь, что страница авторизации загружается в окне приложения!**

![Auth Page](./img/679831c85040133e8429ecfa-19.png)

##### Функционал авторизации
Создайте обработчик события `Click` для кнопки ***Войти***.

```cs
try
{
  var userObj = AppConnect.modelOdb.User.FirstOrDefault(x => x.Login == txbLogin.Text && x.Password == psbPassword.Password);
  if (userObj==null)
  {
    MessageBox.Show("Такого пользователя нет!", "Ошибка при авторизации!",
      MessageBoxButton.OK, MessageBoxImage.Error);
  }
  else
  {
    switch (userObj.IdRole)
    {
      case 1: MessageBox.Show("Здравствуйте, Администратор " + userObj.Name + "!",
        "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
        break;
      case 2:
        MessageBox.Show("Здравствуйте, Ученик " + userObj.Name + "!",
          "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
        break;
      default: MessageBox.Show("Данные не обнаружены!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Warning);
        break;
    }
  }
}
catch (Exception Ex)
{
  MessageBox.Show("Ошибка " + Ex.Message.ToString() + "Критическая ошибка приложения!",
  "Уведомление", MessageBoxButton.OK, MessageBoxImage.Warning);
}
```

_Пояснения к коду_:

Когда идет работа с данными рекомендуют использовать `try..catch` (обработку исключений).

Внутри оператора `try` нужно написать процесс подключения к базе данных и работы с ней.
```cs
var userObj = AppConnect.modelOdb.User.FirstOrDefault(x =>
x.Login  == txbLogin.Text && x.Password == psbPassword.Password);
```

Создадим динамическую переменную **`userObj`**. Будем внутри нее хранить данные, получаемые из полей для ввода логина и пароля.

Обращаться к БД будем посредством созданного нами ранее класса **`AppConnect`** и работать непосредственно с ним.

![Auth Page](./img/679831c85040133e8429ecfa-20.png)

Модель `modelOdb` видит входящие в БД таблицы в качестве атрибутов (`User`).

При наборе строки
```cs
var userObj = AppConnect.modelOdb.User.
```

после очередной точки перечисляются методы работы с LINQ-запросами и EntityFramework.

Нам нужен метод `FirstOrDefault` (он возвращает первый элемент последовательности…).

Создаем анонимную переменную `x`, используемую внутри LINQ-запроса (аналогично параметру цикла `for`).

Возвращаем в переменную `userObj` все значения из таблицы `User`, которые совпали по введенным в текстовые поля логину и паролю.

Если в БД пользователя нет, то нужно вывести об этом сообщение (`if`…), иначе – нужно вывести приветствие для соответствующего пользователя.

**Запустите проект, убедитесь, что на экран выводится каждое из организованных в коде приветствий и уведомлений!**

### Задание на авторизацию и создание бд
[679858275040133e8429ecfc](https://e-learn.petrocollege.ru/mod/assign/view.php?id=358549)

1. Разработать базу данных согласно схеме:

    ![DB Schema](./img/679858275040133e8429ecfc-1.png)

2. Заполнить базу данными, минимум 10 записей в каждой таблице.

3. Разработать страницу авторизации в VS 2022.

    ![Auth page](./img/679858275040133e8429ecfc-2.png)

4. Подключить бд к приложению

    ![Auth page](./img/679858275040133e8429ecfc-3.png)

5. Запрограммировать кнопку Войти

    ![Login page](./img/679858275040133e8429ecfc-4.png)

### Регистрация WPF
[6799fb9c5040133e8429ed29](https://e-learn.petrocollege.ru/mod/assign/view.php?id=358550)

#### Реализация регистрации пользователя в WPF-приложении (C#) к базе данных MS SQL Server (практическая работа)
https://e-learn.petrocollege.ru/pluginfile.php/542787/mod_assign/introattachment/0/%D0%9F%D1%80%D0%B0%D0%BA%D1%82%D0%B8%D1%87%D0%B5%D1%81%D0%BA%D0%B0%D1%8F%20%D1%80%D0%B0%D0%B1%D0%BE%D1%82%D0%B0%20%D0%A0%D0%B5%D0%B3%D0%B8%D1%81%D1%82%D1%80%D0%B0%D1%86%D0%B8%D1%8F%20%D0%BF%D0%BE%D0%BB%D1%8C%D0%B7%D0%BE%D0%B2%D0%B0%D1%82%D0%B5%D0%BB%D1%8F%20%D0%B2%20WPF-%D0%BF%D1%80%D0%B8%D0%BB%D0%BE%D0%B6%D0%B5%D0%BD%D0%B8%D0%B8%20%D0%BA%20%D0%B1%D0%B0%D0%B7%D0%B5%20%D0%B4%D0%B0%D0%BD%D0%BD%D1%8B%D1%85%20MS%20SQL%20Server_%20%281%29.docx?forcedownload=1

**Используемые среды**:
- MS SQL Server Management Studio
- Microsoft Visual Studio

##### Страница регистрации

1. В папке *PageMain* создайте страницу *PageCreateAcc*. Установите высоту страницы 350, ширину – 800.

    Создадим интерфейс для возможности отправки в БД введенных значений нового пользователя.

    ![Picture 1](./img/6799fb9c5040133e8429ed29-1.png)

    - Организуйте разметку страницы в соответствии с рисунком.
    - Используйте контейнер `StackPanel` с вертикальной ориентацией, разместите его в центре страницы.
    - В нем организуйте еще 4 контейнера `StackPanelс` горизонтальной ориентацией.
    - Для ввода пароля используйте компонент `TextBox`, для подтверждения пароля – `PasswordBox`.
    - Оформление компонентов организуйте с помощью стилей, шаблоны которых пропишите в файле *App.xaml*.
    - По нажатию на кнопку **Регистрация** страницы **Авторизация** должна открываться страница для создания аккаунта ученика. При нажатии на кнопку **Назад** страницы **Регистрация** мы должны вернуться на страницу **Авторизация**.

2. Создайте обработчик события `Click` для кнопки Регистрация:

    ```cs
    private void btnRegIn_Click(object sender, RoutedEventArgs e)
    {
      AppFrame.frameMain.Navigate(new PageCreateAcc());
    }
    ```

3. Создайте обработчик события `Click` для кнопки Назад:

    ```cs
    private void btnRegIn_Click(object sender, RoutedEventArgs e)
    {
      AppFrame.frameMain.GoBack();
    }
    ```

_Пояснения к коду_:
- Все действия на главном окне у нас выполняются во фрейме.
- Этот фрейм хранится в объекте `frameMain`.
- Мы загрузили наш фрейм с главной страницы.

**Запустите проект, убедитесь, что осуществляются соответствующие переходы по кнопкам Регистрация и Назад!**

##### Функционал прохождения регистрации ученика

1. Кнопка **Создать** должна быть недоступна, пока в поле подтверждения пароля вводимое значение не совпадет с первоначальным. А поле подтверждения пароля окрашивается красным пока нет совпадения и зеленым в момент совпадения значений в полях.

    Для этого:
    -	Кнопку Создать сделать изначально неактивной `IsEnabled="False"`
    -	Создайте обработчик события `PasswordChanged` компонента `PasswordBox`:
    ```cs
    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
      if (psbPass.Password!=txbPass.Text)
      {
        btnCreate.IsEnabled = false;
        psbPass.Background = Brushes.LightCoral;
        psbPass.BorderBrush = Brushes.Red;
      }
      else
      {
        btnCreate.IsEnbaled = true;
        psbPass.Background = Brushes.LightGreen;
        psbPass.BorderBrush = Brushes.Green;
      }
    }
    ```

    **Запустите проект. Убедитесь, что кнопка Создать и поле подтверждения пароля ведут себя как было запланировано!**

    ![Picture 2](./img/6799fb9c5040133e8429ed29-2.png)

    ![Picture 3](./img/6799fb9c5040133e8429ed29-3.png)

2. Внутри обработчика события `Click` кнопки **Создать** нужно организовать добавление в базу данных значений, вводимых пользователем в соответствующие текстовые поля. Для этого необходимо обратиться к таблице `User` базы данных, и эта таблица должна получить значения из компонентов страницы **Регистрация** (Имя, Логин и Пароль).

    ```cs
    private void btnCreate_Click(object sender, RoutedEventArgs e)
    {
      if (AppConnect.modelOdb.User.Count(x => x.Login==txbLogin.Text) > 0)
      {
        MessageBox.Show("Пользователь с таким логином есть!",
          "Уведомление",MessageBoxButton.OK,MessageBoxImage.Information);
        return;
      }
      try
      {
        User userObj = new User()
        {
          Login = txbLogin.Text,
          Name = txbName.Text,
          Password = txbPass.Text,
          IdRole = 2
        };
        AppConnect.modelOdb.User.Add(userObj);
        AppConnect.modelOdb.SaveChanges();
        MessageBox.Show("Данные успешно добавлены!",
          "Уведомление",MessageBoxButton.OK,MessageBoxImage.Information);
      }
      catch
      {
        MessageBox.Show("Ошибка при добавлении данных!",
          "Уведомление",MessageBoxButton.OK,MessageBoxImage.Error);
      }
    }
    ```

    _Пояснения к коду_:
    - Сначала нужно проверить, есть ли пользователь с таким логином в системе.
    - Если значения, возвращаемые из таблицы `User` по полю `Login` имеются (применяем для этой цели метод `Count` LINQ-запроса), то вывести уведомление о том, что пользователь с таким логином существует в БД.
    - Для добавления данных нового пользователя нужно использовать конструкцию `try..catch`.
    - Создаем новый объект `UserObj` класса `User`, присваиваем его свойствам значения из компонентов страницы **Регистрация**. Регистрировать можно только учеников, поэтому присваиваем значение 2 полю `idRole`. Записываем созданный объект в таблицу БД `User` (метод `Add`) и сохраняем изменения (метод `SaveChanges`). По окончании выводим уведомление о проделанной операции.

    **Запустите проект, зарегистрируйте нового ученика, осуществите вход под его аккаунтом!**

3. Измените интерфейс таким образом, чтобы название проекта отображалось в строке заголовка главного окна, а в верхней части окна каждый раз располагалось название открытой страницы (**Окно авторизации**, **Окно регистрации**).

    Изменения для этого вносим в разметку: добавляем элемент `TextBlock` и в его свойстве `Text` указываем источник текста – `Title` открытой страницы (содержимого фрейма).

    ```xml
    Text="{Binding ElementName=FrmMain},
          Path=Content.Title}"
    ```

    **Запустите проект, убедитесь в отображении заголовков открытых страниц!**

    ![Picture 4](./img/6799fb9c5040133e8429ed29-4.png)

    ![Picture 5](./img/6799fb9c5040133e8429ed29-5.png)

### Задание Регистрация WPF
[679b50185040133e8429ed46](https://e-learn.petrocollege.ru/mod/assign/view.php?id=358551)

https://e-learn.petrocollege.ru/pluginfile.php/542788/mod_assign/introattachment/0/%D0%97%D0%B0%D0%B4%D0%B0%D0%BD%D0%B8%D0%B5%20%D0%BD%D0%B0%20%D1%80%D0%B5%D0%B3%D0%B8%D1%81%D1%82%D1%80%D0%B0%D1%86%D0%B8%D1%8E.docx?forcedownload=1

1. Разработать страницу регистрации в VS 2022.

    ![Registration Form](./img/679b50185040133e8429ed46-1.png)

2. Добавить в базу соответствующие поля

3. Запрограммировать кнопки **Зарегистрироваться** и **Назад**

4. Подключить страницу к авторизации

### Данные
[679c880a5040133e8429ed59](https://e-learn.petrocollege.ru/mod/resource/view.php?id=309444)

Сделать интернет-магазин.

### Вывод данных
[679e34c15040133e8429ed70](https://e-learn.petrocollege.ru/mod/resource/view.php?id=324518)

После разработки авторизации и регистрации добавить возможность вывода списка студентов.

Пример таблицы с информацией о студентах представлен на рисунке 1.

![Picture 1](./img/679e34c15040133e8429ed70-1.png)

Для вывода списка информации о студентах необходимо создать `ListView`. Пример оформления представлен на Рисунке 2.

```xml
<Grid>
  <Grid.ColumnDefinitions>
    <ColumnDefinition/>
  </Grid.ColumnDefinitions>
  <Grid.RowDefinitions>
    <RowDefinition Height="33"/>
    <RowDefinition Height="497"/>
    <RowDefinition Height="50"/>
  </Grid.RowDefinitions>
  <ListView x:Name="listProducts" Grid.Row="1" ScrollViewer.CanContentScroll="False">
    <ListView.ItemTemplate>
      <DataTemplate>
        <Grid>
          <Grid.ColumnDefinitions>
            <ColumnDefinitions Width="100"/>
            <ColumnDefinitions Width="700"/>
            <ColumnDefinitions Width="200"/>
          </Grid.ColumnDefinitions>
          <Image Source="{Binding CurrentPhoto}" Grid.Column="0"/>
          <StackPanel Grid.Column="1" Width="auto" Orientation="Vertical" HorizontalAlignment="Left">
            <TextBlock Width="auto" TextWrapping="Wrap" Height="auto">
              <Run Text="ФИО: "/>
              <Run Text="{Binding FIO}"/>
            </TextBlock>
            <TextBlock Width="auto" TextWrapping="Wrap" Height="auto">
              <Run Text="Курс: "/>
              <Run Text="{Binding Course}"/>
            </TextBlock>
            <TextBlock Width="auto" TextWrapping="Wrap" Height="auto">
              <Run Text="Специальность: "/>
              <Run Text="{Binding Specialty}"/>
            </TextBlock>
          </StackPanel>
          <StackPanel Grid.Column="2" Width="auto" Orientation="Vertical" HorizontalAlignment="Left">
            <TextBlock Width="auto" TextWrapping="Wrap" Height="auto">
              <Run Text="Пол: "/>
              <Run Text="{Binding Gender}"/>
            </TextBlock>
          </StackPanel>
        </Grid>
      </DataTemplate>
    </ListView.ItemTemplate>
  </ListView>
  <TextBlock x:Name="tbCounter" Text="Не найдено" Grid.Row="2" Width="95" HorizontalAlignment="Left" Height="22" Margin="695,0,0,0">
</Grid>
</Page>
```

Для присоединения базы к `ListView` необходимо добавить следующий код
```cs
public PageTask()
{
  InitializeComponent();
  List<Student> products = AppConnect.model101.Students.ToList();

  if (products.Count > 0)
  {
    tbCounter.Text = "Найдено " + products.Count + " товаров";
  }
  else
  {
    tbCounter.Text = "Не найдено";
  }
  listProducts.ItemSource = products;
}
```

Для вывода изображений в классе `Student` необходимо прописать свойство
```cs
public string Specialty { get; set; }
public string Image { get; set; }
public string CurrentPhoto {
  get
  {
    if (String.IsNullOrEmpty(Image) || String.IsNullOrWhiteSpace(Image))
    {
      return "/Images/picture.png";
    }
    else
    {
      return "/Images/" + Image;
    }
  }
}
```

### Фильтрация, поиск, сортировка
[679f415a5040133e8429ed7c](https://e-learn.petrocollege.ru/mod/resource/view.php?id=325841)

```cs
Product[] FindProduct()
{
  // В переменную product записываем список из таблицы Product
  var product = AppConnect.modelOdb.Product.ToList();
  var productall = product;
  // Поиск по названию продуктов
  if (TextSearch != null)
  {
    // ProductName1 (первое значение) - таблица с названиями
    // ProductName1 (второе значение) - столбик с названиями в таблице
    product = product.Where(x => x.ProductName1.ProductName1.ToLower().Contains(TextSearch.Text.ToLower())).ToList();
  }
  // Фильтрация по скидке
  if (ComboFilter.SelectedIndex > 0)
  {
    switch (ComboFilter.SelectedIndex)
    {
      case 1:
        product = product.Where(x => x.ProductDiscountAmount > 0 && x.ProductDiscountAmount < 10).ToList();
        break;
      case 2:
        product = product.Where(x => x.ProductDiscountAmount >= 10 && x.ProductDiscountAmount < 15).ToList();
        break;
      case 3:
        product = product.Where(x => x.ProductDiscountAmount >= 15 && x.ProductDiscountAmount >= 15).ToList();
        break;
    }
  }
  // Сортировка по возрастанию и убыванию цены
  if (ComboSort.SelectedIndex > 0)
  {
    switch (ComboSort.SelectedIndex)
    {
      case 1:
        product = product.OrderBy(x => x.ProductCost).ToList();
        break;
      case 2:
        product = product.OrderByDescending(x => x.ProductCost).ToList();
        break;
    }
  }
  // Количество элементов найденных
  if (product.Count > 0)
  {
    LabelCount.Content = "Найдено" + product.Count + " из " + productall.Count;
  }
  else
  {
    LabelCount.Content = "Ничего не найдено ";
  }
  return product.ToArray();
}
```
