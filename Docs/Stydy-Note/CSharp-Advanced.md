# C# Advanced Concepts with Examples

## Table of Contents

- [1. Generics](#1-generics)
- [2. Delegates & Events](#2-delegates--events)
- [Delegates](#delegates)
- [Events](#events)
- [3. Lambda Expressions & LINQ](#3-lambda-expressions--linq)
- [Lambda Expressions](#lambda-expressions)
- [LINQ (Language Integrated Query)](#linq-language-integrated-query)
- [4. Async / Await & Task Parallel Library](#4-async--await--task-parallel-library)
- [5. Extension Methods](#5-extension-methods)
- [6. Pattern Matching](#6-pattern-matching)
- [7. Records](#7-records)
- [8. Dependency Injection (DI)](#8-dependency-injection-di)
- [9. Reflection](#9-reflection)
- [10. Span\<T\> and Memory\<T\>](#10-spant-and-memoryt)
- [11. Expression Trees](#11-expression-trees)
- [12. Channels (Producer-Consumer)](#12-channels-producer-consumer)
- [13. IDisposable & using Statement](#13-idisposable--using-statement)
- [14. Source Generators & Attributes (Overview)](#14-source-generators--attributes-overview)
- [15. Advanced OOP: Interfaces (Advanced Patterns)](#15-advanced-oop-interfaces-advanced-patterns)
- [Interface Segregation & Composition](#interface-segregation--composition)
- [Abstract Factory Pattern](#abstract-factory-pattern)
- [Strategy Pattern](#strategy-pattern)
- [16. C# 12 / .NET 8 Modern Features](#16-c-12--net-8-modern-features)

---

## 1. Generics

Generics allow you to write type-safe, reusable code without committing to a specific data type.

```csharp
// Generic method
static T Max<T>(T a, T b) where T : IComparable<T>
{
    return a.CompareTo(b) > 0 ? a : b;
}

Console.WriteLine(Max(3, 7));         // 7
Console.WriteLine(Max("apple", "banana")); // banana

// Generic class
class Repository<T> where T : class
{
    private List<T> _items = new();

    public void Add(T item) => _items.Add(item);
    public T? Get(int index) => _items.ElementAtOrDefault(index);
    public IEnumerable<T> GetAll() => _items;
    public int Count => _items.Count;
}

// Generic interface
interface IRepository<T>
{
    void Add(T item);
    T? GetById(int id);
    IEnumerable<T> GetAll();
}

// Generic constraints
class ServiceBase<T> where T : class, new()
{
    public T Create() => new T();
}
```

---

## 2. Delegates & Events

### Delegates

A delegate is a type-safe function pointer.

```csharp
// Declare delegate
delegate int MathOperation(int a, int b);

static int Add(int a, int b) => a + b;
static int Multiply(int a, int b) => a * b;

MathOperation op = Add;
Console.WriteLine(op(3, 4));   // 7

op = Multiply;
Console.WriteLine(op(3, 4));   // 12

// Multicast delegate
Action<string> notify = Console.WriteLine;
notify += s => Console.WriteLine($"Also: {s}");
notify("Hello");  // prints twice

// Built-in delegates
Func<int, int, int> add = (a, b) => a + b;
Action<string> print = Console.WriteLine;
Predicate<int> isEven = n => n % 2 == 0;
```

### Events

```csharp
class Button
{
    // Define event using EventHandler
    public event EventHandler? Clicked;

    public void Click()
    {
        Console.WriteLine("Button clicked!");
        Clicked?.Invoke(this, EventArgs.Empty);  // raise event
    }
}

class Logger
{
    public void OnClicked(object? sender, EventArgs e)
    {
        Console.WriteLine($"[LOG] Button was clicked at {DateTime.Now}");
    }
}

Button btn = new Button();
Logger logger = new Logger();

btn.Clicked += logger.OnClicked;
btn.Clicked += (s, e) => Console.WriteLine("Lambda handler fired!");
btn.Click();

// Custom EventArgs
class OrderEventArgs : EventArgs
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
}

class OrderService
{
    public event EventHandler<OrderEventArgs>? OrderPlaced;

    public void PlaceOrder(int id, decimal amount)
    {
        Console.WriteLine($"Order {id} placed.");
        OrderPlaced?.Invoke(this, new OrderEventArgs { OrderId = id, Amount = amount });
    }
}
```

---

## 3. Lambda Expressions & LINQ

### Lambda Expressions

```csharp
// Expression lambda
Func<int, int> square = x => x * x;

// Statement lambda
Func<int, int, string> compare = (a, b) =>
{
    if (a > b) return "greater";
    if (a < b) return "lesser";
    return "equal";
};

// Capturing variables (closure)
int multiplier = 3;
Func<int, int> triple = n => n * multiplier;
Console.WriteLine(triple(5));  // 15
```

### LINQ (Language Integrated Query)

```csharp
using System.Linq;

List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

// Method syntax
var evens      = numbers.Where(n => n % 2 == 0);
var squared    = numbers.Select(n => n * n);
var sum        = numbers.Sum();
var avg        = numbers.Average();
var max        = numbers.Max();
var first      = numbers.First(n => n > 5);
var ordered    = numbers.OrderByDescending(n => n);
var top3       = numbers.OrderByDescending(n => n).Take(3);

// Query syntax
var query = from n in numbers
            where n % 2 == 0
            orderby n descending
            select n * n;

// Complex LINQ on objects
class Product
{
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Price { get; set; }
}

var products = new List<Product>
{
    new() { Name = "Laptop",  Category = "Electronics", Price = 999.99m },
    new() { Name = "Phone",   Category = "Electronics", Price = 599.99m },
    new() { Name = "Desk",    Category = "Furniture",   Price = 250.00m },
    new() { Name = "Chair",   Category = "Furniture",   Price = 150.00m },
    new() { Name = "Monitor", Category = "Electronics", Price = 399.99m },
};

// GroupBy
var byCategory = products
    .GroupBy(p => p.Category)
    .Select(g => new
    {
        Category = g.Key,
        TotalPrice = g.Sum(p => p.Price),
        Count = g.Count()
    });

foreach (var group in byCategory)
    Console.WriteLine($"{group.Category}: {group.Count} items, Total: {group.TotalPrice:C}");

// Join
var categories = new List<(string Name, string Description)>
{
    ("Electronics", "Electronic gadgets"),
    ("Furniture",   "Home & office furniture")
};

var joined = from p in products
             join c in categories on p.Category equals c.Name
             select new { p.Name, p.Price, c.Description };
```

---

## 4. Async / Await & Task Parallel Library

```csharp
using System.Threading.Tasks;

// Basic async method
static async Task<string> FetchDataAsync(string url)
{
    using HttpClient client = new HttpClient();
    string data = await client.GetStringAsync(url);
    return data;
}

// Async void (only for event handlers)
static async void Button_Click(object? sender, EventArgs e)
{
    await Task.Delay(1000);
    Console.WriteLine("Done!");
}

// Task.WhenAll - run tasks in parallel
static async Task RunParallelAsync()
{
    Task<int> task1 = Task.Run(() => { Thread.Sleep(500); return 1; });
    Task<int> task2 = Task.Run(() => { Thread.Sleep(300); return 2; });
    Task<int> task3 = Task.Run(() => { Thread.Sleep(200); return 3; });

    int[] results = await Task.WhenAll(task1, task2, task3);
    Console.WriteLine(string.Join(", ", results));  // 1, 2, 3
}

// Task.WhenAny
static async Task FirstResponseAsync()
{
    var tasks = new[]
    {
        Task.Delay(300).ContinueWith(_ => "Fast"),
        Task.Delay(1000).ContinueWith(_ => "Slow"),
    };
    Task<string> winner = await Task.WhenAny(tasks);
    Console.WriteLine(await winner);  // "Fast"
}

// CancellationToken
static async Task LongOperationAsync(CancellationToken ct)
{
    for (int i = 0; i < 100; i++)
    {
        ct.ThrowIfCancellationRequested();
        await Task.Delay(100, ct);
        Console.WriteLine($"Step {i}");
    }
}

// Usage
CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
try
{
    await LongOperationAsync(cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation was cancelled.");
}
```

---

## 5. Extension Methods

```csharp
// Must be in a static class
static class StringExtensions
{
    public static bool IsPalindrome(this string s)
    {
        string clean = s.ToLower().Replace(" ", "");
        return clean == new string(clean.Reverse().ToArray());
    }

    public static string Truncate(this string s, int maxLength)
    {
        return s.Length <= maxLength ? s : s[..maxLength] + "...";
    }

    public static bool IsNullOrWhiteSpace(this string? s) =>
        string.IsNullOrWhiteSpace(s);
}

static class IEnumerableExtensions
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
        where T : class
    {
        return source.Where(x => x != null)!;
    }
}

// Usage
Console.WriteLine("racecar".IsPalindrome());         // True
Console.WriteLine("Hello World".Truncate(5));        // Hello...
```

---

## 6. Pattern Matching

```csharp
// Type pattern
object obj = "Hello";
if (obj is string str)
    Console.WriteLine(str.Length);  // 5

// switch expression with patterns
static string Describe(object obj) => obj switch
{
    int n when n < 0   => "Negative integer",
    int n              => $"Positive integer: {n}",
    string s           => $"String of length {s.Length}",
    null               => "null",
    _                  => "Unknown"
};

// Property pattern
class Point { public int X { get; set; } public int Y { get; set; } }

static string Quadrant(Point p) => p switch
{
    { X: > 0, Y: > 0 } => "Q1",
    { X: < 0, Y: > 0 } => "Q2",
    { X: < 0, Y: < 0 } => "Q3",
    { X: > 0, Y: < 0 } => "Q4",
    _                   => "Origin or Axis"
};

// Tuple pattern
static string RPS(string p1, string p2) => (p1, p2) switch
{
    ("rock",     "scissors") => "Player 1 wins",
    ("scissors", "paper")    => "Player 1 wins",
    ("paper",    "rock")     => "Player 1 wins",
    (var a, var b) when a == b => "Draw",
    _                          => "Player 2 wins"
};

// List pattern (C# 11)
int[] arr = { 1, 2, 3 };
if (arr is [1, .., 3])
    Console.WriteLine("Starts with 1, ends with 3");
```

---

## 7. Records

Records are immutable reference types with value-based equality.

```csharp
// Record declaration (positional)
record Person(string FirstName, string LastName, int Age);

var p1 = new Person("Alice", "Smith", 30);
var p2 = new Person("Alice", "Smith", 30);
Console.WriteLine(p1 == p2);        // True (value equality)
Console.WriteLine(p1);              // Person { FirstName = Alice, LastName = Smith, Age = 30 }

// With expression (non-destructive mutation)
var p3 = p1 with { Age = 31 };

// Record with additional members
record Employee(string Name, string Department, decimal Salary)
{
    public string FullTitle => $"{Name} - {Department}";

    public decimal AnnualSalary => Salary * 12;
}

// Record struct (value type, C# 10)
record struct Coordinate(double X, double Y);

// Mutable record
record MutablePerson
{
    public string Name { get; set; } = "";
    public int Age { get; set; }
}
```

---

## 8. Dependency Injection (DI)

```csharp
// Interfaces
interface IEmailService
{
    void Send(string to, string subject, string body);
}

interface ILogger
{
    void Log(string message);
}

// Implementations
class SmtpEmailService : IEmailService
{
    private readonly ILogger _logger;

    public SmtpEmailService(ILogger logger)
    {
        _logger = logger;
    }

    public void Send(string to, string subject, string body)
    {
        _logger.Log($"Sending email to {to}");
        // SMTP logic here
    }
}

class ConsoleLogger : ILogger
{
    public void Log(string message) =>
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
}

// In ASP.NET Core (Program.cs)
builder.Services.AddSingleton<ILogger, ConsoleLogger>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

// Service lifetimes:
// Singleton  - one instance for the whole app lifetime
// Scoped     - one instance per HTTP request
// Transient  - new instance every time it's requested
```

---

## 9. Reflection

```csharp
using System.Reflection;

class SampleClass
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    private string _secret = "hidden";

    public void Greet() => Console.WriteLine($"Hello from {Name}");
}

// Get type info
Type type = typeof(SampleClass);
Console.WriteLine(type.Name);           // SampleClass
Console.WriteLine(type.FullName);       // Namespace.SampleClass
Console.WriteLine(type.IsClass);        // True

// Get properties
foreach (var prop in type.GetProperties())
    Console.WriteLine($"{prop.Name}: {prop.PropertyType.Name}");

// Get methods
foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
    Console.WriteLine(method.Name);

// Create instance dynamically
object? instance = Activator.CreateInstance(type);

// Set property value
type.GetProperty("Name")?.SetValue(instance, "Dynamic Alice");

// Invoke method
type.GetMethod("Greet")?.Invoke(instance, null);

// Custom attributes
[AttributeUsage(AttributeTargets.Property)]
class RequiredAttribute : Attribute
{
    public string ErrorMessage { get; set; } = "Field is required.";
}

class UserModel
{
    [Required(ErrorMessage = "Name is required.")]
    public string Name { get; set; } = "";
}

// Read custom attribute
var prop2 = typeof(UserModel).GetProperty("Name");
var attr = prop2?.GetCustomAttribute<RequiredAttribute>();
Console.WriteLine(attr?.ErrorMessage);  // Name is required.
```

---

## 10. Span\<T\> and Memory\<T\>

High-performance, zero-allocation slicing of memory.

```csharp
using System;

// Span over array (stack-allocated)
int[] array = { 1, 2, 3, 4, 5, 6, 7, 8 };
Span<int> span = array.AsSpan();

Span<int> slice = span[2..5];  // { 3, 4, 5 }
slice[0] = 99;                 // modifies original array
Console.WriteLine(array[2]);   // 99

// stackalloc with Span
Span<byte> buffer = stackalloc byte[256];
buffer.Fill(0);

// Memory<T> (heap, can be async)
Memory<char> memory = "Hello World".AsMemory();
Memory<char> part = memory[6..];
Console.WriteLine(new string(part.Span));  // "World"

// String parsing without allocation
ReadOnlySpan<char> input = "name=Alice;age=30".AsSpan();
int semicolon = input.IndexOf(';');
ReadOnlySpan<char> namePart = input[..semicolon];
Console.WriteLine(namePart.ToString());  // "name=Alice"
```

---

## 11. Expression Trees

```csharp
using System.Linq.Expressions;

// Build an expression tree manually
// Represents: x => x * x + 1
ParameterExpression x = Expression.Parameter(typeof(int), "x");
Expression body = Expression.Add(
    Expression.Multiply(x, x),
    Expression.Constant(1)
);
Expression<Func<int, int>> lambda = Expression.Lambda<Func<int, int>>(body, x);

Func<int, int> compiled = lambda.Compile();
Console.WriteLine(compiled(5));  // 26

// Using expressions for dynamic filtering (common in ORM/EF Core)
static IQueryable<T> Filter<T>(IQueryable<T> source, string propName, object value)
{
    var param = Expression.Parameter(typeof(T), "x");
    var prop  = Expression.Property(param, propName);
    var val   = Expression.Constant(value);
    var equal = Expression.Equal(prop, val);
    var predicate = Expression.Lambda<Func<T, bool>>(equal, param);
    return source.Where(predicate);
}
```

---

## 12. Channels (Producer-Consumer)

```csharp
using System.Threading.Channels;

// Unbounded channel
Channel<int> channel = Channel.CreateUnbounded<int>();

// Producer
async Task ProduceAsync()
{
    for (int i = 0; i < 10; i++)
    {
        await channel.Writer.WriteAsync(i);
        await Task.Delay(100);
    }
    channel.Writer.Complete();
}

// Consumer
async Task ConsumeAsync()
{
    await foreach (int item in channel.Reader.ReadAllAsync())
        Console.WriteLine($"Consumed: {item}");
}

await Task.WhenAll(ProduceAsync(), ConsumeAsync());
```

---

## 13. IDisposable & using Statement

```csharp
// Implementing IDisposable
class DatabaseConnection : IDisposable
{
    private bool _disposed = false;

    public void Open() => Console.WriteLine("Connection opened.");

    public void Query(string sql) => Console.WriteLine($"Executing: {sql}");

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            // Release managed resources
            Console.WriteLine("Connection closed.");
        }
        _disposed = true;
    }

    ~DatabaseConnection() => Dispose(false);
}

// Usage
using (var conn = new DatabaseConnection())
{
    conn.Open();
    conn.Query("SELECT * FROM Users");
}  // Dispose called automatically

// Using declaration (C# 8+)
using var conn2 = new DatabaseConnection();
conn2.Open();
// Disposed at end of scope
```

---

## 14. Source Generators & Attributes (Overview)

```csharp
// Custom attribute for documentation/code gen purposes
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
class LogAttribute : Attribute
{
    public string Level { get; set; } = "Info";
}

[Log(Level = "Debug")]
class UserService
{
    [Log(Level = "Info")]
    public User GetUser(int id)
    {
        // Implementation
        return new User();
    }
}

// Caller information attributes
static void LogCall(
    string message,
    [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
    [System.Runtime.CompilerServices.CallerFilePath]   string filePath   = "",
    [System.Runtime.CompilerServices.CallerLineNumber] int    lineNumber  = 0)
{
    Console.WriteLine($"[{memberName}:{lineNumber}] {message}");
}
```

---

## 15. Advanced OOP: Interfaces (Advanced Patterns)

### Interface Segregation & Composition

```csharp
// Instead of one fat interface, use many small ones
interface IReadable<T>  { T Read(int id); }
interface IWritable<T>  { void Write(T item); }
interface IDeletable     { void Delete(int id); }
interface ICrudRepository<T> : IReadable<T>, IWritable<T>, IDeletable { }

// Default interface methods (C# 8+)
interface ILogger
{
    void Log(string message);
    void LogError(string message) => Log($"[ERROR] {message}");
    void LogInfo(string message)  => Log($"[INFO] {message}");
}

class FileLogger : ILogger
{
    public void Log(string message) => File.AppendAllText("app.log", message + "\n");
    // LogError and LogInfo are inherited with default implementation
}
```

### Abstract Factory Pattern

```csharp
interface IButton { void Render(); }
interface ICheckbox { void Check(); }

interface IUIFactory
{
    IButton CreateButton();
    ICheckbox CreateCheckbox();
}

class WindowsButton : IButton  { public void Render() => Console.WriteLine("Windows Button"); }
class MacButton    : IButton   { public void Render() => Console.WriteLine("Mac Button"); }
class WindowsCheckbox : ICheckbox { public void Check() => Console.WriteLine("Windows Checkbox"); }
class MacCheckbox : ICheckbox  { public void Check() => Console.WriteLine("Mac Checkbox"); }

class WindowsFactory : IUIFactory
{
    public IButton CreateButton()     => new WindowsButton();
    public ICheckbox CreateCheckbox() => new WindowsCheckbox();
}

class MacFactory : IUIFactory
{
    public IButton CreateButton()     => new MacButton();
    public ICheckbox CreateCheckbox() => new MacCheckbox();
}
```

### Strategy Pattern

```csharp
interface ISortStrategy
{
    void Sort(List<int> data);
}

class BubbleSort : ISortStrategy
{
    public void Sort(List<int> data)
    {
        // bubble sort implementation
        for (int i = 0; i < data.Count - 1; i++)
            for (int j = 0; j < data.Count - i - 1; j++)
                if (data[j] > data[j + 1])
                    (data[j], data[j + 1]) = (data[j + 1], data[j]);
    }
}

class LinqSort : ISortStrategy
{
    public void Sort(List<int> data)
    {
        data.Sort();
    }
}

class Sorter
{
    private ISortStrategy _strategy;

    public Sorter(ISortStrategy strategy) { _strategy = strategy; }
    public void SetStrategy(ISortStrategy strategy) { _strategy = strategy; }
    public void Sort(List<int> data) => _strategy.Sort(data);
}
```

---

## 16. C# 12 / .NET 8 Modern Features

```csharp
// Primary constructors (C# 12)
class PersonService(ILogger logger, string connectionString)
{
    public void DoWork() => logger.Log("Working...");
}

// Collection expressions (C# 12)
int[] arr1 = [1, 2, 3];
List<string> list = ["Alice", "Bob"];
Span<char> chars = ['a', 'b', 'c'];

// Spread operator in collections
int[] first  = [1, 2, 3];
int[] second = [4, 5, 6];
int[] all    = [..first, ..second, 7, 8];

// Required members (C# 11)
class Config
{
    public required string ConnectionString { get; init; }
    public required string ApiKey { get; init; }
    public int Timeout { get; init; } = 30;
}
var config = new Config { ConnectionString = "...", ApiKey = "abc" };

// Raw string literals (C# 11)
string json = """
    {
        "name": "Alice",
        "age": 30
    }
    """;

// Generic math (C# 11 / .NET 7+)
static T AddValues<T>(T a, T b) where T : System.Numerics.INumber<T>
    => a + b;

Console.WriteLine(AddValues(3, 4));        // 7
Console.WriteLine(AddValues(3.5, 1.5));    // 5.0
```

---

> 🚀 **Pro Tip:** Mastering these advanced concepts will significantly improve your ability to write performant, maintainable, and scalable C# applications. Focus on async patterns and LINQ as they appear in almost every real-world .NET project.
