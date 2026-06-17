# C# Beginner Concepts with Examples

## Table of Contents

- [1. Hello World](#1-hello-world)
- [2. Variables & Data Types](#2-variables--data-types)
- [3. Type Conversion](#3-type-conversion)
- [4. Operators](#4-operators)
- [5. String Methods](#5-string-methods)
- [6. Conditional Statements](#6-conditional-statements)
- [7. Loops](#7-loops)
- [8. Arrays](#8-arrays)
- [9. Methods (Functions)](#9-methods-functions)
- [10. Object Oriented Programming (OOP)](#10-object-oriented-programming-oop)
- [10.1 Classes & Objects](#101-classes--objects)
- [10.2 Encapsulation](#102-encapsulation)
- [10.3 Inheritance](#103-inheritance)
- [10.4 Polymorphism](#104-polymorphism)
- [10.5 Abstraction](#105-abstraction)
- [10.6 Interfaces](#106-interfaces)
- [11. Collections](#11-collections)
- [12. Exception Handling](#12-exception-handling)
- [13. Nullable Types](#13-nullable-types)
- [14. File I/O Basics](#14-file-io-basics)
- [15. var and Type Inference](#15-var-and-type-inference)

---

## 1. Hello World

**Definition:** Every C# program needs an entry point — the `Main` method — where execution begins. `Console.WriteLine` prints a line of text to the standard output. `using System;` imports the `System` namespace so types like `Console` can be used without their full name.

- `class Program` — code in C# lives inside classes.
- `static void Main(string[] args)` — the method the runtime calls first; `args` holds command-line arguments.
- Modern C# (top-level statements) lets you skip the boilerplate and write just `Console.WriteLine("Hello, World!");`.

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
    }
}
```

---

## 2. Variables & Data Types

**Definition:** A variable is a named storage location with a specific data type that determines what values it can hold and how much memory it uses. C# is *strongly typed*, so every variable's type is known at compile time.

- **Value types** (`int`, `double`, `bool`, `char`, `decimal`) store the data directly.
- **Reference types** (`string`, arrays, classes) store a reference to data on the heap.
- Use `decimal` for money (high precision), `double` for general floating-point math, and `float` when memory matters more than precision.

```csharp
int age = 25;
double salary = 55000.50;
float temperature = 36.6f;
decimal price = 199.99m;
char grade = 'A';
string name = "Alice";
bool isActive = true;

Console.WriteLine($"Name: {name}, Age: {age}, Active: {isActive}");
```

| Type      | Size    | Example           |
|-----------|---------|-------------------|
| `int`     | 4 bytes | `int x = 10;`     |
| `double`  | 8 bytes | `double x = 3.14;`|
| `float`   | 4 bytes | `float x = 3.14f;`|
| `decimal` | 16 bytes| `decimal x = 9.99m;`|
| `char`    | 2 bytes | `char c = 'A';`   |
| `string`  | varies  | `string s = "Hi";`|
| `bool`    | 1 byte  | `bool b = true;`  |

---

## 3. Type Conversion

**Definition:** Type conversion (casting) changes a value from one data type to another. C# offers several ways depending on safety and the types involved.

- **Implicit** — automatic and safe when there is no data loss (e.g., `int` → `double`).
- **Explicit (cast)** — required when data loss is possible (e.g., `double` → `int`); you write `(int)value`.
- **`Convert` class** — converts between types and throws if the input is invalid.
- **`TryParse`** — the safe way to parse strings; returns `false` instead of throwing on bad input.

```csharp
// Implicit conversion (safe, no data loss)
int num = 100;
double d = num;

// Explicit conversion (casting)
double pi = 3.14;
int rounded = (int)pi;  // 3

// Using Convert class
string strNum = "42";
int converted = Convert.ToInt32(strNum);

// TryParse (safe parsing)
bool success = int.TryParse("123abc", out int result);
Console.WriteLine(success);  // False
```

---

## 4. Operators

**Definition:** Operators are symbols that perform operations on values (operands). C# groups them into categories:

- **Arithmetic** (`+ - * / %`) — math; note `/` on two `int`s does integer division and `%` gives the remainder.
- **Comparison** (`> < >= <= == !=`) — return a `bool`.
- **Logical** (`&& || !`) — combine boolean conditions (`&&`/`||` short-circuit).
- **Assignment** (`= += -= *= /=`) — store/update values.
- **Ternary** (`condition ? a : b`) — a compact inline if/else.

```csharp
// Arithmetic
int a = 10, b = 3;
Console.WriteLine(a + b);   // 13
Console.WriteLine(a - b);   // 7
Console.WriteLine(a * b);   // 30
Console.WriteLine(a / b);   // 3
Console.WriteLine(a % b);   // 1

// Comparison
Console.WriteLine(a > b);   // True
Console.WriteLine(a == b);  // False
Console.WriteLine(a != b);  // True

// Logical
bool x = true, y = false;
Console.WriteLine(x && y);  // False
Console.WriteLine(x || y);  // True
Console.WriteLine(!x);      // False

// Assignment
int c = 5;
c += 3;   // c = 8
c *= 2;   // c = 16

// Ternary
string status = (a > b) ? "Greater" : "Smaller";
```

---

## 5. String Methods

**Definition:** A `string` is an immutable sequence of characters. Every method that "changes" a string actually returns a **new** string, leaving the original unchanged. The `System.String` type provides many helpers for searching, trimming, replacing, and slicing text.

- **Immutable** — useful for safety and thread-safety; for heavy concatenation use `StringBuilder`.
- **Interpolation** (`$"...{value}..."`) — embeds expressions directly inside a string.
- **Verbatim** (`@"..."`) — treats backslashes literally (great for file paths).

```csharp
string text = "  Hello, C# World!  ";

Console.WriteLine(text.Trim());              // "Hello, C# World!"
Console.WriteLine(text.ToUpper());           // "  HELLO, C# WORLD!  "
Console.WriteLine(text.ToLower());           // "  hello, c# world!  "
Console.WriteLine(text.Contains("C#"));      // True
Console.WriteLine(text.Replace("World", "Universe"));
Console.WriteLine(text.Length);              // 20
Console.WriteLine(text.Substring(9, 2));     // "C#"  (index 9 = 'C', index 10 = '#')
Console.WriteLine(text.StartsWith("  He")); // True

// String interpolation
string firstName = "John";
int userAge = 30;
string message = $"My name is {firstName} and I am {userAge} years old.";

// String concatenation
string full = "Hello" + " " + "World";

// Verbatim string
string path = @"C:\Users\Alice\Documents";
```

---

## 6. Conditional Statements

**Definition:** Conditional statements let a program choose different paths based on whether conditions are `true` or `false`.

- **`if` / `else if` / `else`** — evaluate conditions top to bottom; the first match runs.
- **`switch`** — compares one value against multiple `case` labels (cleaner than long if-chains).
- **`switch` expression (C# 8+)** — a concise form that *returns* a value using `=>` arms and `_` as the default.

```csharp
// if-else
int score = 75;

if (score >= 90)
    Console.WriteLine("Grade: A");
else if (score >= 80)
    Console.WriteLine("Grade: B");
else if (score >= 70)
    Console.WriteLine("Grade: C");
else
    Console.WriteLine("Grade: F");

// switch
string day = "Monday";
switch (day)
{
    case "Monday":
    case "Tuesday":
    case "Wednesday":
    case "Thursday":
    case "Friday":
        Console.WriteLine("Weekday");
        break;
    case "Saturday":
    case "Sunday":
        Console.WriteLine("Weekend");
        break;
    default:
        Console.WriteLine("Unknown");
        break;
}

// switch expression (C# 8+)
string result = score switch
{
    >= 90 => "A",
    >= 80 => "B",
    >= 70 => "C",
    _     => "F"
};
```

---

## 7. Loops

**Definition:** Loops repeat a block of code multiple times, which is essential for processing collections or repeating work.

- **`for`** — when you know the number of iterations (uses a counter).
- **`while`** — repeats *while* a condition stays true (checked **before** each pass; may run zero times).
- **`do-while`** — like `while`, but the condition is checked **after**, so the body runs at least once.
- **`foreach`** — iterates over every item in a collection without an index.
- **`break`** exits the loop entirely; **`continue`** skips to the next iteration.

```csharp
// for loop
for (int i = 0; i < 5; i++)
    Console.WriteLine($"i = {i}");

// while loop
int count = 0;
while (count < 3)
{
    Console.WriteLine($"count = {count}");
    count++;
}

// do-while loop
int n = 0;
do
{
    Console.WriteLine($"n = {n}");
    n++;
} while (n < 3);

// foreach loop
string[] fruits = { "Apple", "Banana", "Cherry" };
foreach (string fruit in fruits)
    Console.WriteLine(fruit);

// break and continue
for (int i = 0; i < 10; i++)
{
    if (i == 5) break;      // exit loop
    if (i % 2 == 0) continue; // skip even
    Console.WriteLine(i);   // prints 1, 3
}
```

---

## 8. Arrays

**Definition:** An array is a fixed-size, ordered collection of elements of the **same type**, stored in contiguous memory and accessed by a zero-based index.

- **Single-dimensional** — a simple list (`int[]`).
- **Multi-dimensional** (`int[,]`) — a rectangular grid (table) where every row has the same length.
- **Jagged** (`int[][]`) — an "array of arrays" where rows can have different lengths.
- The size is fixed once created; for a resizable list use `List<T>` (see Collections).

```csharp
// Single-dimensional array
int[] numbers = { 10, 20, 30, 40, 50 };
Console.WriteLine(numbers[0]);       // 10
Console.WriteLine(numbers.Length);   // 5

// Initialize with size
string[] names = new string[3];
names[0] = "Alice";
names[1] = "Bob";
names[2] = "Charlie";

// Multi-dimensional array
int[,] matrix = {
    { 1, 2, 3 },
    { 4, 5, 6 }
};
Console.WriteLine(matrix[1, 2]); // 6

// Jagged array
int[][] jagged = new int[3][];
jagged[0] = new int[] { 1, 2 };
jagged[1] = new int[] { 3, 4, 5 };
jagged[2] = new int[] { 6 };

// Array methods
Array.Sort(numbers);
Array.Reverse(numbers);
int idx = Array.IndexOf(numbers, 30);
```

---

## 9. Methods (Functions)

**Definition:** A method is a named, reusable block of code that performs a task. It can take **parameters** (inputs) and optionally **return** a value, helping you avoid repetition and organize logic.

- **Return type** — `void` means no value is returned; otherwise specify the type (e.g., `int`).
- **Default parameters** — give a parameter a fallback value so callers can omit it.
- **Named arguments** — pass arguments by name, in any order, for clarity.
- **`out`** — lets a method return extra values through parameters.
- **`params`** — accepts a variable number of arguments as an array.

```csharp
// Basic method
static void Greet(string name)
{
    Console.WriteLine($"Hello, {name}!");
}

// Method with return value
static int Add(int a, int b)
{
    return a + b;
}

// Default parameters
static void PrintInfo(string name, int age = 18)
{
    Console.WriteLine($"{name} is {age} years old.");
}

// Named arguments
PrintInfo(age: 25, name: "Alice");

// Out parameter
static bool TryDivide(int a, int b, out double result)
{
    if (b == 0) { result = 0; return false; }
    result = (double)a / b;
    return true;
}

// Params keyword (variable arguments)
static int Sum(params int[] numbers)
{
    int total = 0;
    foreach (int n in numbers) total += n;
    return total;
}

Console.WriteLine(Sum(1, 2, 3, 4, 5)); // 15
```

---

## 10. Object Oriented Programming (OOP)

**Definition:** OOP is a programming paradigm that models software as **objects** — self-contained units that bundle data (fields/properties) with behavior (methods). It is built on four pillars: **Encapsulation**, **Inheritance**, **Polymorphism**, and **Abstraction**.

### 10.1 Classes & Objects

**Definition:** A **class** is a blueprint that defines the fields, properties, and methods an object will have. An **object** is a concrete instance of a class created with `new`.

- **Fields** — variables that hold an object's state (often `private`).
- **Properties** — controlled access to fields via `get`/`set`; auto-properties (`{ get; set; }`) generate the backing field for you.
- **Constructor** — a special method that runs when an object is created, used to initialize it.

```csharp
// Class definition
class Car
{
    // Fields
    private string _brand;
    private int _year;

    // Properties
    public string Brand
    {
        get { return _brand; }
        set { _brand = value; }
    }

    public int Year { get; set; }  // Auto-property

    // Constructor
    public Car(string brand, int year)
    {
        _brand = brand;
        Year = year;
    }

    // Method
    public void DisplayInfo()
    {
        Console.WriteLine($"Brand: {_brand}, Year: {Year}");
    }
}

// Creating objects
Car myCar = new Car("Toyota", 2022);
myCar.DisplayInfo();  // Brand: Toyota, Year: 2022
```

### 10.2 Encapsulation

**Definition:** Encapsulation hides an object's internal state and exposes only a controlled, public interface. By keeping fields `private` and validating changes through methods/properties, you protect data from invalid modifications.

- The `_balance` field can't be set directly from outside — only through `Deposit`/`Withdraw`, which enforce rules.
- This makes code safer and easier to maintain because the class controls how its data changes.

```csharp
class BankAccount
{
    private double _balance;  // private field

    public double Balance
    {
        get { return _balance; }
    }

    public void Deposit(double amount)
    {
        if (amount > 0)
            _balance += amount;
    }

    public bool Withdraw(double amount)
    {
        if (amount > 0 && amount <= _balance)
        {
            _balance -= amount;
            return true;
        }
        return false;
    }
}

BankAccount account = new BankAccount();
account.Deposit(500);
account.Withdraw(100);
Console.WriteLine(account.Balance);  // 400
```

### 10.3 Inheritance

**Definition:** Inheritance lets a class (the *derived*/child class) reuse and extend the fields and methods of another class (the *base*/parent class), modeling an "is-a" relationship (a `Dog` *is an* `Animal`).

- `: base(name)` calls the parent constructor.
- `virtual` marks a base method as overridable; `override` provides the child's version.
- Promotes code reuse — shared behavior lives in the base class.

```csharp
// Base class
class Animal
{
    public string Name { get; set; }

    public Animal(string name)
    {
        Name = name;
    }

    public virtual void Speak()
    {
        Console.WriteLine($"{Name} makes a sound.");
    }
}

// Derived class
class Dog : Animal
{
    public string Breed { get; set; }

    public Dog(string name, string breed) : base(name)
    {
        Breed = breed;
    }

    public override void Speak()
    {
        Console.WriteLine($"{Name} barks!");
    }
}

class Cat : Animal
{
    public Cat(string name) : base(name) { }

    public override void Speak()
    {
        Console.WriteLine($"{Name} meows!");
    }
}

Animal dog = new Dog("Rex", "Labrador");
Animal cat = new Cat("Whiskers");
dog.Speak();  // Rex barks!
cat.Speak();  // Whiskers meows!
```

### 10.4 Polymorphism

**Definition:** Polymorphism ("many forms") lets the same operation behave differently depending on the type involved. C# supports two kinds:

- **Compile-time (overloading)** — multiple methods share a name but differ in parameters; the compiler picks the right one.
- **Runtime (overriding)** — a base-type reference (`Shape`) calls the derived type's overridden method (`Circle.Area`) decided at runtime.

```csharp
// Method Overloading (compile-time polymorphism)
class Calculator
{
    public int Add(int a, int b) => a + b;
    public double Add(double a, double b) => a + b;
    public int Add(int a, int b, int c) => a + b + c;
}

// Method Overriding (runtime polymorphism)
class Shape
{
    public virtual double Area() => 0;
}

class Circle : Shape
{
    public double Radius { get; set; }
    public Circle(double radius) { Radius = radius; }
    public override double Area() => Math.PI * Radius * Radius;
}

class Rectangle : Shape
{
    public double Width { get; set; }
    public double Height { get; set; }
    public Rectangle(double w, double h) { Width = w; Height = h; }
    public override double Area() => Width * Height;
}

List<Shape> shapes = new List<Shape>
{
    new Circle(5),
    new Rectangle(4, 6)
};

foreach (Shape s in shapes)
    Console.WriteLine($"Area: {s.Area():F2}");
```

### 10.5 Abstraction

**Definition:** Abstraction hides complex implementation details and exposes only the essential features. An **abstract class** cannot be instantiated and may define `abstract` methods (no body) that derived classes **must** implement, alongside concrete shared methods.

- `abstract` members force every subclass to provide their own implementation.
- Concrete members (like `DisplayModel`) provide shared behavior all subclasses inherit.

```csharp
// Abstract class
abstract class Vehicle
{
    public string Model { get; set; }

    public Vehicle(string model) { Model = model; }

    // Abstract method - must be implemented by derived class
    public abstract void StartEngine();

    // Concrete method - shared behavior
    public void DisplayModel()
    {
        Console.WriteLine($"Model: {Model}");
    }
}

class ElectricCar : Vehicle
{
    public ElectricCar(string model) : base(model) { }

    public override void StartEngine()
    {
        Console.WriteLine($"{Model}: Silent electric engine started.");
    }
}

class GasCar : Vehicle
{
    public GasCar(string model) : base(model) { }

    public override void StartEngine()
    {
        Console.WriteLine($"{Model}: Vroom! Gas engine started.");
    }
}
```

### 10.6 Interfaces

**Definition:** An interface is a contract that defines *what* a class can do (method/property signatures) without specifying *how*. A class can implement **multiple** interfaces, enabling a "can-do" relationship and flexible design.

- Members have no implementation (prior to C# 8 default methods) — implementing classes must provide the bodies.
- Unlike classes, a type can implement many interfaces, working around C#'s single-inheritance limit.
- Naming convention: interfaces start with `I` (e.g., `IPayable`).

```csharp
// Interface definition
interface IPayable
{
    decimal CalculatePay();
    void ProcessPayment();
}

interface IReportable
{
    string GenerateReport();
}

// A class can implement multiple interfaces
class Employee : IPayable, IReportable
{
    public string Name { get; set; }
    public decimal HourlyRate { get; set; }
    public int HoursWorked { get; set; }

    public Employee(string name, decimal rate, int hours)
    {
        Name = name;
        HourlyRate = rate;
        HoursWorked = hours;
    }

    public decimal CalculatePay() => HourlyRate * HoursWorked;

    public void ProcessPayment()
    {
        Console.WriteLine($"Processing payment of {CalculatePay():C} for {Name}");
    }

    public string GenerateReport()
    {
        return $"Employee: {Name}, Pay: {CalculatePay():C}";
    }
}
```

---

## 11. Collections

**Definition:** Collections are dynamic data structures (in `System.Collections.Generic`) that, unlike arrays, can grow and shrink at runtime. Each is optimized for different access patterns.

- **`List<T>`** — a resizable, index-accessible list (the everyday workhorse).
- **`Dictionary<TKey,TValue>`** — fast key→value lookups; keys are unique.
- **`HashSet<T>`** — stores **unique** items; ignores duplicates.
- **`Queue<T>`** — FIFO (first-in, first-out): `Enqueue`/`Dequeue`.
- **`Stack<T>`** — LIFO (last-in, first-out): `Push`/`Pop`.

```csharp
using System.Collections.Generic;

// List<T>
List<string> cities = new List<string> { "London", "Paris", "Tokyo" };
cities.Add("New York");
cities.Remove("Paris");
Console.WriteLine(cities.Count);  // 3

// Dictionary<TKey, TValue>
Dictionary<string, int> ages = new Dictionary<string, int>
{
    { "Alice", 30 },
    { "Bob", 25 }
};
ages["Charlie"] = 28;
Console.WriteLine(ages["Alice"]);  // 30

if (ages.TryGetValue("Bob", out int bobAge))
    Console.WriteLine(bobAge);

// HashSet<T> - unique elements
HashSet<int> uniqueNumbers = new HashSet<int> { 1, 2, 3, 2, 1 };
Console.WriteLine(uniqueNumbers.Count);  // 3

// Queue<T>
Queue<string> queue = new Queue<string>();
queue.Enqueue("First");
queue.Enqueue("Second");
string first = queue.Dequeue();  // "First"

// Stack<T>
Stack<int> stack = new Stack<int>();
stack.Push(1);
stack.Push(2);
int top = stack.Pop();  // 2
```

---

## 12. Exception Handling

**Definition:** Exceptions represent runtime errors. Exception handling lets a program detect and respond to these errors gracefully instead of crashing, using `try`/`catch`/`finally`.

- **`try`** — wraps code that might fail.
- **`catch`** — handles a specific exception type; order from most to least specific.
- **`finally`** — always runs (cleanup), whether or not an exception occurred.
- **`throw`** — raises an exception; you can define **custom** exceptions by inheriting from `Exception`.

```csharp
// try-catch-finally
try
{
    int[] arr = new int[5];
    arr[10] = 1;  // IndexOutOfRangeException
}
catch (IndexOutOfRangeException ex)
{
    Console.WriteLine($"Index error: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"General error: {ex.Message}");
}
finally
{
    Console.WriteLine("This always executes.");
}

// Throwing exceptions
static void Divide(int a, int b)
{
    if (b == 0)
        throw new ArgumentException("Divisor cannot be zero.", nameof(b));
    Console.WriteLine(a / b);
}

// Custom exception
class InsufficientFundsException : Exception
{
    public decimal Amount { get; }

    public InsufficientFundsException(decimal amount)
        : base($"Insufficient funds. Needed: {amount:C}")
    {
        Amount = amount;
    }
}
```

---

## 13. Nullable Types

**Definition:** By default, value types (like `int`) cannot be `null`. Nullable types let them represent "no value," and C# provides operators to work with nulls safely.

- **`int?`** — a nullable value type that can hold a number *or* `null`.
- **`??`** (null-coalescing) — supplies a fallback when a value is `null`.
- **`?.`** (null-conditional) — safely accesses members; returns `null` instead of throwing.
- **`!`** (null-forgiving) — tells the compiler "trust me, this isn't null."
- **Nullable reference types (C# 8+)** — `string?` vs `string` help the compiler catch potential null bugs.

```csharp
// Nullable value types
int? nullableInt = null;
double? nullableDouble = 3.14;

// Null coalescing operator (??)
int value = nullableInt ?? 0;  // 0

// Null conditional operator (?.)
string? name = null;
int? length = name?.Length;  // null (no exception)

// Null forgiving operator (!)
string definitelyNotNull = name!;  // tells compiler it's not null

// is null check
if (nullableInt is null)
    Console.WriteLine("It is null");

// Nullable reference types (C# 8+)
string? maybeNull = null;
string notNull = "Hello";
```

---

## 14. File I/O Basics

**Definition:** File I/O (Input/Output) reads from and writes to files on disk, via the `System.IO` namespace.

- **`File` helpers** (`WriteAllText`, `ReadAllText`, `ReadAllLines`, `AppendAllText`, `Exists`) — quick one-shot operations.
- **`StreamWriter` / `StreamReader`** — efficient for reading/writing large files line by line.
- Wrap streams in `using` so the file handle is released automatically when done.

```csharp
using System.IO;

// Write to file
File.WriteAllText("output.txt", "Hello, File!");

// Append to file
File.AppendAllText("output.txt", "\nSecond line.");

// Read from file
string content = File.ReadAllText("output.txt");
string[] lines = File.ReadAllLines("output.txt");

// Check if file exists
if (File.Exists("output.txt"))
    Console.WriteLine("File exists!");

// Using StreamWriter
using StreamWriter writer = new StreamWriter("log.txt");
writer.WriteLine("Log entry 1");
writer.WriteLine("Log entry 2");

// Using StreamReader
using StreamReader reader = new StreamReader("log.txt");
string line;
while ((line = reader.ReadLine()) != null)
    Console.WriteLine(line);
```

---

## 15. var and Type Inference

**Definition:** The `var` keyword lets the compiler **infer** a variable's type from the value assigned to it. The variable is still strongly typed — `var` is not `dynamic` and the type is fixed at compile time.

- Use it to reduce repetition, especially with long generic types.
- The variable **must** be initialized so the compiler can determine the type.
- Prefer explicit types when the inferred type isn't obvious to a reader.

```csharp
var name = "Alice";         // string
var age = 30;               // int
var price = 9.99m;          // decimal
var numbers = new[] { 1, 2, 3 }; // int[]

// var with collections
var list = new List<string> { "a", "b", "c" };
var dict = new Dictionary<string, int>();
```

---

> 💡 **Tip:** Practice each concept by writing small programs. Build projects like a calculator, student grade tracker, or a simple bank account simulator to reinforce these fundamentals.
