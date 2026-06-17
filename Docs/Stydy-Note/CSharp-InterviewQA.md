# C# Interview Questions & Answers

> Most frequently asked C# and .NET interview questions, from beginner to advanced level.

## Table of Contents

- [BASICS](#basics)
- [OBJECT ORIENTED PROGRAMMING](#object-oriented-programming)
- [INTERMEDIATE](#intermediate)
- [ADVANCED](#advanced)
- [QUICK-FIRE QUESTIONS](#quick-fire-questions)

---

<a id="basics"></a>
## 📘 BASICS

### Q1. What is C#? What are its key features?
**Answer:**  
C# is a strongly-typed, object-oriented programming language developed by Microsoft as part of the .NET platform.

**Key features:**
- Object-Oriented (encapsulation, inheritance, polymorphism, abstraction)
- Type-safe and strongly typed
- Garbage collection (automatic memory management)
- Async/await for asynchronous programming
- LINQ for querying data
- Cross-platform via .NET Core / .NET 5+
- Rich standard library (BCL)

---

### Q2. What is the difference between `value types` and `reference types`?

| Feature        | Value Type                        | Reference Type                    |
|----------------|-----------------------------------|-----------------------------------|
| Stored in      | Stack                             | Heap                              |
| Examples       | `int`, `double`, `bool`, `struct` | `class`, `string`, `array`        |
| Default value  | Zero/false                        | `null`                            |
| Assignment     | Copies the value                  | Copies the reference (pointer)    |
| Nullable       | Only with `?` (e.g., `int?`)      | Inherently nullable               |

```csharp
int a = 5;
int b = a;  // b is a copy
b = 10;
Console.WriteLine(a);  // still 5

class Box { public int Value; }
Box x = new Box { Value = 5 };
Box y = x;  // y points to same object
y.Value = 10;
Console.WriteLine(x.Value);  // 10
```

---

### Q3. What is boxing and unboxing?

**Boxing** — Converting a value type to `object` (heap allocation).  
**Unboxing** — Extracting the value type back from `object`.

```csharp
int num = 42;
object boxed = num;       // Boxing
int unboxed = (int)boxed; // Unboxing
```

> ⚠️ Excessive boxing/unboxing hurts performance. Use generics to avoid it.

---

### Q4. What is the difference between `==` and `.Equals()`?

```csharp
string a = new string("hello".ToCharArray());
string b = new string("hello".ToCharArray());

Console.WriteLine(a == b);         // True  (string overloads ==)
Console.WriteLine(a.Equals(b));    // True

object x = 42;
object y = 42;
Console.WriteLine(x == y);         // False — two separate boxed objects, so == compares references
Console.WriteLine(x.Equals(y));    // True  — Equals compares the underlying int values
```

- `==` for strings compares **content** (overloaded).
- For custom classes, `==` compares **reference** by default.
- When two value types are **boxed** into `object`, `==` compares references (usually `false`), while `.Equals()` compares values.
- `.Equals()` can be **overridden** for value equality.
- `ReferenceEquals()` always compares memory addresses.

---

### Q5. What is `string` vs `StringBuilder`?

| Feature     | `string`                    | `StringBuilder`              |
|-------------|-----------------------------|------------------------------|
| Mutability  | Immutable                   | Mutable                      |
| Performance | Poor for many concatenations| Efficient for many operations|
| Thread-safe | Yes (immutable)             | Not thread-safe              |

```csharp
// Bad - creates many string objects
string result = "";
for (int i = 0; i < 1000; i++)
    result += i;

// Good
var sb = new System.Text.StringBuilder();
for (int i = 0; i < 1000; i++)
    sb.Append(i);
string result2 = sb.ToString();
```

---

<a id="object-oriented-programming"></a>
## 🔷 OBJECT ORIENTED PROGRAMMING

### Q6. Explain the four pillars of OOP with examples.

#### Encapsulation
Hiding internal details and exposing only what's necessary.
```csharp
class Account
{
    private decimal _balance;
    public decimal Balance => _balance;
    public void Deposit(decimal amount)
    {
        if (amount > 0) _balance += amount;
    }
}
```

#### Inheritance
A class inherits fields and methods from a parent class.
```csharp
class Animal { public virtual void Speak() => Console.WriteLine("..."); }
class Dog : Animal { public override void Speak() => Console.WriteLine("Woof!"); }
```

#### Polymorphism
The same method behaves differently based on the object type.
```csharp
Animal a = new Dog();
a.Speak();  // "Woof!" — runtime polymorphism
```

#### Abstraction
Hiding implementation complexity behind a clean interface.
```csharp
abstract class Shape { public abstract double Area(); }
class Circle : Shape
{
    double r;
    public Circle(double r) { this.r = r; }
    public override double Area() => Math.PI * r * r;
}
```

---

### Q7. What is the difference between `abstract class` and `interface`?

| Feature                  | Abstract Class               | Interface                          |
|--------------------------|------------------------------|------------------------------------|
| Instantiation            | ❌ Cannot                    | ❌ Cannot                          |
| Multiple inheritance     | ❌ Single only               | ✅ Multiple                        |
| Constructors             | ✅ Yes                       | ❌ No                              |
| Fields                   | ✅ Yes                       | ❌ No (only properties)            |
| Access modifiers         | ✅ Yes                       | ✅ (C# 8+ default implementations) |
| Default implementation   | ✅ Yes                       | ✅ (C# 8+)                         |
| "is-a" relationship      | ✅                           | "can-do" relationship              |

```csharp
abstract class Animal
{
    public string Name { get; set; } = "";
    public abstract void Speak();         // must override
    public void Breathe() => Console.WriteLine("Breathing..."); // shared
}

interface ISwimmable { void Swim(); }
interface IFlyable   { void Fly();  }

class Duck : Animal, ISwimmable, IFlyable
{
    public override void Speak() => Console.WriteLine("Quack!");
    public void Swim() => Console.WriteLine("Swimming...");
    public void Fly()  => Console.WriteLine("Flying...");
}
```

---

### Q8. What is the difference between `override` and `new` keyword?

```csharp
class Base
{
    public virtual void Show()  => Console.WriteLine("Base.Show (virtual)");
    public void Display()       => Console.WriteLine("Base.Display");
}

class Derived : Base
{
    public override void Show() => Console.WriteLine("Derived.Show (override)");
    public new void Display()   => Console.WriteLine("Derived.Display (new)");
}

Base obj = new Derived();
obj.Show();     // "Derived.Show (override)" — runtime dispatch
obj.Display();  // "Base.Display"            — compile-time, new hides it
```

- `override` → **runtime polymorphism** (virtual dispatch).
- `new` → **hides** the base member, no polymorphism.

---

### Q9. What is a `sealed` class?

A `sealed` class **cannot be inherited**. Use it when you want to prevent further extension.

```csharp
sealed class FinalClass
{
    public void DoWork() => Console.WriteLine("Working...");
}

// class Child : FinalClass { }  ← Compile error!
```

`sealed` can also be applied to overriding methods to prevent further overriding.

---

### Q10. What are access modifiers in C#?

| Modifier             | Accessible From                                  |
|----------------------|--------------------------------------------------|
| `public`             | Everywhere                                       |
| `private`            | Same class only                                  |
| `protected`          | Same class + derived classes                     |
| `internal`           | Same assembly                                    |
| `protected internal` | Same assembly OR derived classes                 |
| `private protected`  | Same class OR derived classes within assembly    |

---

<a id="intermediate"></a>
## 🔶 INTERMEDIATE

### Q11. What is the difference between `struct` and `class`?

| Feature         | `struct`          | `class`           |
|-----------------|-------------------|-------------------|
| Type            | Value type        | Reference type    |
| Memory          | Stack             | Heap              |
| Inheritance     | Cannot inherit    | Supports          |
| Nullability     | Non-nullable      | Nullable          |
| Default ctor    | Always has one    | Can define custom |
| Performance     | Better for small  | Better for large  |

```csharp
struct Point { public int X, Y; }
class Rectangle { public int Width, Height; }
```

---

### Q12. What is `readonly` vs `const`?

| Feature      | `const`                        | `readonly`                      |
|--------------|--------------------------------|---------------------------------|
| Set when     | Compile time                   | Runtime (constructor or inline) |
| Instance/Static | Static only                 | Can be instance or static       |
| Can be object | No (primitive/string only)   | Yes                             |

```csharp
class Config
{
    public const string AppName = "MyApp";          // compile-time
    public readonly DateTime StartedAt;             // runtime

    public Config()
    {
        StartedAt = DateTime.Now;
    }
}
```

---

### Q13. What is a `delegate`? How is it different from an interface?

A **delegate** is a type-safe function pointer. An **interface** defines a contract.

```csharp
// Delegate
delegate void Notify(string message);

Notify n = Console.WriteLine;
n("Hello delegate!");

// Same behavior with interface
interface INotifier { void Notify(string message); }
```

Delegates support multicast, anonymous methods, and lambdas — making them ideal for callbacks and events.

---

### Q14. What are the differences between `IEnumerable`, `ICollection`, and `IList`?

| Interface       | Read | Count | Add/Remove | Index Access |
|-----------------|------|-------|------------|--------------|
| `IEnumerable<T>`| ✅   | ❌    | ❌         | ❌           |
| `ICollection<T>`| ✅   | ✅    | ✅         | ❌           |
| `IList<T>`      | ✅   | ✅    | ✅         | ✅           |

---

### Q15. What is the difference between `throw` and `throw ex`?

```csharp
try { SomeMethod(); }
catch (Exception ex)
{
    throw;     // ✅ Preserves original stack trace
    // throw ex;  ❌ Resets stack trace — loses where it originated
}
```

Always use `throw;` to rethrow exceptions to preserve the full stack trace.

---

### Q16. What is LINQ? Give examples.

**LINQ** (Language Integrated Query) allows querying collections using C# syntax.

```csharp
var numbers = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

// Method syntax
var result = numbers
    .Where(n => n % 2 == 0)
    .Select(n => n * n)
    .OrderByDescending(n => n)
    .Take(3);
// Output: 100, 64, 36

// Query syntax
var query = from n in numbers
            where n > 5
            select n;
```

---

### Q17. What is the difference between `First()`, `FirstOrDefault()`, `Single()`, `SingleOrDefault()`?

| Method              | No match        | Multiple matches |
|---------------------|-----------------|------------------|
| `First()`           | Exception       | Returns first    |
| `FirstOrDefault()`  | Returns default | Returns first    |
| `Single()`          | Exception       | Exception        |
| `SingleOrDefault()` | Returns default | Exception        |

---

### Q18. Explain `async`/`await`. What problem does it solve?

Async/await prevents **blocking threads** during I/O operations (network, file, database), making apps more scalable.

```csharp
// Without async — thread is blocked
string data = new HttpClient().GetStringAsync(url).Result;  // ❌ blocks thread

// With async — thread is released while waiting
async Task<string> GetDataAsync(string url)
{
    using var client = new HttpClient();
    return await client.GetStringAsync(url);  // ✅ non-blocking
}
```

> ⚠️ Avoid `.Result` or `.Wait()` on tasks — it can cause deadlocks in ASP.NET.

---

### Q19. What is the difference between `Task` and `Thread`?

| Feature      | `Thread`                         | `Task`                                  |
|--------------|----------------------------------|-----------------------------------------|
| Level        | Low-level OS thread              | High-level abstraction over ThreadPool |
| Return value | None                             | `Task<T>` has a result                 |
| Exception    | Hard to propagate                | Easy with `await`                       |
| Cancellation | Manual                           | `CancellationToken`                     |
| Recommended  | Rarely                           | ✅ Preferred                            |

---

### Q20. What is Dependency Injection (DI)?

DI is a design pattern where dependencies are **provided (injected)** rather than created inside a class.

```csharp
// Without DI — tightly coupled
class OrderService
{
    private EmailService _email = new EmailService();  // hard-coded
}

// With DI — loosely coupled
class OrderService
{
    private readonly IEmailService _email;
    public OrderService(IEmailService email) { _email = email; }
}

// Registration in ASP.NET Core
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
```

---

<a id="advanced"></a>
## 🔴 ADVANCED

### Q21. What is the difference between `Scoped`, `Transient`, and `Singleton` DI lifetimes?

| Lifetime    | Instance Created         | Use Case                           |
|-------------|--------------------------|-------------------------------------|
| `Singleton` | Once per app lifetime    | Configuration, caching, logging     |
| `Scoped`    | Once per HTTP request    | DbContext, unit-of-work services    |
| `Transient` | Every time requested     | Lightweight, stateless services     |

> ⚠️ Never inject a Scoped service into a Singleton — it causes a **captive dependency** bug.

---

### Q22. What is garbage collection in C#?

The **Garbage Collector (GC)** automatically reclaims memory that is no longer reachable.

- Objects are allocated on the **managed heap**.
- GC runs in **3 generations**: Gen 0, Gen 1, Gen 2.
  - **Gen 0**: Short-lived objects (most frequent GC)
  - **Gen 1**: Survived Gen 0
  - **Gen 2**: Long-lived objects (least frequent GC)
- `IDisposable` is used for **unmanaged resources** (files, DB connections).

```csharp
// Force GC (avoid in production)
GC.Collect();
GC.WaitForPendingFinalizers();

// Proper resource cleanup
using var conn = new SqlConnection(connectionString);
// Automatically calls conn.Dispose() at end of scope
```

---

### Q23. What is the difference between `Finalize` and `Dispose`?

| Feature     | `Finalize` (`~Destructor`)  | `Dispose` (IDisposable)     |
|-------------|-----------------------------|-----------------------------|
| Called by   | GC (non-deterministic)      | Developer / `using` block   |
| Timing      | Unpredictable               | Immediate                   |
| Performance | Slower (2 GC cycles needed) | Faster                      |
| Use case    | Last resort for unmanaged   | Preferred cleanup pattern   |

---

### Q24. What is reflection and when would you use it?

**Reflection** allows inspecting and invoking type metadata at runtime.

```csharp
Type type = typeof(MyClass);
var props = type.GetProperties();
var methods = type.GetMethods();
object instance = Activator.CreateInstance(type);
type.GetMethod("Run")?.Invoke(instance, null);
```

**Use cases:** Serialization frameworks, ORMs (Entity Framework), DI containers, test frameworks.

> ⚠️ Reflection is slow — cache reflected types when used repeatedly.

---

### Q25. What is `IQueryable` vs `IEnumerable` in Entity Framework?

| Feature         | `IEnumerable<T>`                 | `IQueryable<T>`                        |
|-----------------|----------------------------------|----------------------------------------|
| Execution       | In-memory (client-side)          | Translated to SQL (server-side)        |
| LINQ filtering  | After data is loaded             | Before data is loaded                  |
| Performance     | Poor for large datasets          | Efficient — only fetches needed rows   |

```csharp
// IEnumerable — loads ALL users, then filters in memory
var users = context.Users.ToList().Where(u => u.Age > 18);

// IQueryable — filters in the SQL query
var users = context.Users.Where(u => u.Age > 18).ToList();
```

---

### Q26. What are expression trees?

Expression trees represent code as data structures that can be **inspected and compiled at runtime**.

```csharp
Expression<Func<int, bool>> isEven = n => n % 2 == 0;

// Inspect the tree
Console.WriteLine(isEven.Body);        // (n % 2) == 0
Console.WriteLine(isEven.Parameters[0].Name);  // n

// Compile and execute
Func<int, bool> func = isEven.Compile();
Console.WriteLine(func(4));  // True
```

Used heavily by ORMs like Entity Framework to translate lambda expressions to SQL.

---

### Q27. What are covariance and contravariance in generics?

**Covariance (`out`)** — use a more derived type than specified.  
**Contravariance (`in`)** — use a less derived type than specified.

```csharp
// Covariance — IEnumerable<Dog> can be used as IEnumerable<Animal>
IEnumerable<Dog> dogs = new List<Dog>();
IEnumerable<Animal> animals = dogs;  // ✅ covariant (out T)

// Contravariance — Action<Animal> can be used as Action<Dog>
Action<Animal> actOnAnimal = a => Console.WriteLine(a.Name);
Action<Dog> actOnDog = actOnAnimal;  // ✅ contravariant (in T)
```

---

### Q28. What is `Span<T>` and why is it useful?

`Span<T>` provides a **zero-allocation, type-safe view** over a contiguous region of memory.

```csharp
int[] arr = { 1, 2, 3, 4, 5 };
Span<int> span = arr.AsSpan(1, 3);  // { 2, 3, 4 }
span[0] = 99;
Console.WriteLine(arr[1]);  // 99 (modifies original)

// Parse without allocating a new string
ReadOnlySpan<char> text = "Hello World".AsSpan();
ReadOnlySpan<char> hello = text[..5];
```

---

### Q29. How does `lock` work and what is a deadlock?

```csharp
private static readonly object _sync = new object();
private int _counter = 0;

void Increment()
{
    lock (_sync)
    {
        _counter++;  // thread-safe
    }
}
```

**Deadlock** — two threads each holding a lock and waiting for the other's lock.

```csharp
// Deadlock example
lock (lockA) { lock (lockB) { ... } }  // Thread 1
lock (lockB) { lock (lockA) { ... } }  // Thread 2 → DEADLOCK
```

**Prevention:** Always acquire locks in the same order, use `SemaphoreSlim` for async, or use `Monitor.TryEnter` with timeout.

---

### Q30. What is the difference between `ConcurrentDictionary` and `Dictionary` + lock?

```csharp
// Not thread-safe
Dictionary<string, int> dict = new();
// Multiple threads writing → race conditions

// Thread-safe
ConcurrentDictionary<string, int> safe = new();
safe.AddOrUpdate("key", 1, (k, v) => v + 1);
safe.GetOrAdd("key", 0);
```

`ConcurrentDictionary` uses fine-grained locking internally — better performance than a single lock on the whole dictionary.

---

### Q31. What is middleware in ASP.NET Core?

Middleware is software assembled into the request pipeline to handle requests and responses.

```csharp
// Custom middleware
app.Use(async (context, next) =>
{
    Console.WriteLine($"Before: {context.Request.Path}");
    await next.Invoke();  // call next middleware
    Console.WriteLine($"After: {context.Response.StatusCode}");
});

// Custom middleware class
public class TimingMiddleware
{
    private readonly RequestDelegate _next;

    public TimingMiddleware(RequestDelegate next) { _next = next; }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await _next(context);
        sw.Stop();
        Console.WriteLine($"Request took {sw.ElapsedMilliseconds}ms");
    }
}

app.UseMiddleware<TimingMiddleware>();
```

---

### Q32. What is the Repository Pattern?

The Repository Pattern abstracts data access behind an interface, decoupling business logic from persistence.

```csharp
interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<IEnumerable<User>> GetAllAsync();
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int id);
}

class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    public UserRepository(AppDbContext context) { _context = context; }

    public async Task<User?> GetByIdAsync(int id) =>
        await _context.Users.FindAsync(id);

    public async Task<IEnumerable<User>> GetAllAsync() =>
        await _context.Users.ToListAsync();

    public async Task AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }
    // ...
}
```

---

### Q33. What is the difference between `Select` and `SelectMany`?

```csharp
var data = new List<List<int>> { new() { 1, 2 }, new() { 3, 4 } };

// Select — returns IEnumerable<IEnumerable<int>> (nested)
var nested = data.Select(x => x);

// SelectMany — flattens the result into IEnumerable<int>
var flat = data.SelectMany(x => x);  // { 1, 2, 3, 4 }
```

---

### Q34. What are nullable reference types (C# 8+)?

Enable with `<Nullable>enable</Nullable>` in `.csproj`.

```csharp
string  nonNullable = "hello";    // cannot be null
string? nullable    = null;       // can be null

void Process(string? name)
{
    // Compiler warns you to check
    if (name is null) return;
    Console.WriteLine(name.Length); // safe now
}
```

---

### Q35. What is the difference between `async Task`, `async Task<T>`, and `async void`?

| Return Type    | Use Case                                  | Can be awaited |
|----------------|-------------------------------------------|----------------|
| `async Task`   | Async method with no return value         | ✅             |
| `async Task<T>`| Async method returning a value            | ✅             |
| `async void`   | Event handlers only                       | ❌             |

```csharp
async Task DoWorkAsync() { await Task.Delay(100); }
async Task<int> GetValueAsync() { await Task.Delay(100); return 42; }
async void Button_Click(object s, EventArgs e) { await DoWorkAsync(); }
```

> ⚠️ Avoid `async void` outside of event handlers — exceptions cannot be caught with `try/catch`.

---

<a id="quick-fire-questions"></a>
## 💡 QUICK-FIRE QUESTIONS

| Question | Answer |
|----------|--------|
| What does `yield return` do? | Creates a lazy iterator — values generated on demand |
| What is `dynamic` type? | Type checked at runtime, not compile time |
| What is `partial` class? | Class split across multiple files |
| What is `nameof` operator? | Returns the string name of a variable/type safely |
| What is `??=` operator? | Null-coalescing assignment: `x ??= defaultValue` |
| What is method hiding? | Using `new` keyword to hide a base class member |
| What is an indexer? | Allows object to be indexed like an array `obj[0]` |
| What is `params`? | Allows variable number of arguments to a method |
| What is `object` in C#? | Base type of all types (`System.Object`) |
| Is `string` a value or reference type? | Reference type, but acts like value type due to immutability |

---

> ✅ **Tip:** Review these questions out loud, draw diagrams for OOP concepts, and practice with LeetCode / HackerRank challenges using C# to reinforce your knowledge before interviews.
