# Database Concepts in DevConnect

---

## Status Summary

| Concept | Implemented? | Where |
|---------|-------------|-------|
| **DbContext** | ✅ Yes | `Data/FirstAPIContext.cs`, `Data/DevConnectDbContext.cs` |
| **DbSet** | ✅ Yes | Both contexts — `DbSet<Books>`, `DbSet<User>`, `DbSet<Post>`, `DbSet<Like>`, `DbSet<Comment>` |
| **Migrations** | ✅ Yes | `Migrations/` & `Migrations/DevConnectDb/` |
| **Relationships** | ✅ Yes | One-to-Many: `User→Posts`, `Post→Likes`, `Post→Comments`, `User→Likes`, `User→Comments` |
| **ACID** | ⚠️ Partial | EF Core handles it internally — not explicitly used |
| **CAP Theorem** | ❌ Not applicable | Applies to distributed systems — SQL Server is not distributed here |

---

## 1. DbContext ✅ Implemented

### What is DbContext?
`DbContext` is the **bridge between your C# code and the database**. It manages:
- Database connections
- Querying data (translates LINQ → SQL)
- Tracking changes to objects
- Saving changes back to the database

### In DevConnect

**`Data/FirstAPIContext.cs`** — for Books (learning context):
```csharp
public class FirstAPIContext : DbContext
{
    public FirstAPIContext(DbContextOptions<FirstAPIContext> options) 
        : base(options) { }

    // Seed data — pre-populate Books table on migration
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Books>().HasData(
            new Books { Id = 1, Title = "Title", Author = "Author", YearOfPublished = 2021 },
            new Books { Id = 2, Title = "Title2", Author = "Author2", YearOfPublished = 2024 },
            new Books { Id = 3, Title = "Title3", Author = "Author3", YearOfPublished = 2025 }
        );
    }

    public DbSet<Books> Books { get; set; }
}
```

**`Data/DevConnectDbContext.cs`** — for Users (production context):
```csharp
public class DevConnectDbContext : DbContext
{
    public DevConnectDbContext(DbContextOptions<DevConnectDbContext> options) 
        : base(options) { }

    public DbSet<User> Users { get; set; }
}
```

### How DbContext is Registered (`Program.cs`)
```csharp
builder.Services.AddDbContext<FirstAPIContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<DevConnectDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```
> Both contexts connect to the same SQL Server database but manage different tables.

### How DbContext is Used in Controllers
```csharp
// EF Core LINQ queries — translated to SQL automatically
await _context.Users.ToListAsync();           // SELECT * FROM Users
await _context.Users.FindAsync(id);           // SELECT * FROM Users WHERE Id = @id
await _context.Users.AnyAsync(u => u.Email == dto.Email);  // SELECT TOP 1 WHERE Email = @email

_context.Users.Add(user);
await _context.SaveChangesAsync();            // INSERT INTO Users ...

_context.Users.Remove(user);
await _context.SaveChangesAsync();            // DELETE FROM Users WHERE Id = @id
```

---

## 2. DbSet ✅ Implemented

### What is DbSet?
`DbSet<T>` represents a **table in the database**. It is the entry point for all CRUD operations on that table.

### In DevConnect

| DbSet | Context | Maps To |
|-------|---------|---------|
| `DbSet<Books> Books` | `FirstAPIContext` | `Books` table in SQL Server |
| `DbSet<User> Users` | `DevConnectDbContext` | `Users` table in SQL Server || `DbSet<Post> Posts` | `DevConnectDbContext` | `Posts` table in SQL Server |
### Common DbSet Operations Used

```csharp
// READ
_context.Books.ToListAsync()              // Get all rows
_context.Books.FindAsync(id)              // Get by primary key
_context.Books.FirstOrDefaultAsync(...)   // Get first matching
_context.Books.AnyAsync(...)              // Check if exists

// WRITE
_context.Books.Add(newBook)               // Track new entity (INSERT on SaveChanges)
_context.Books.Remove(book)               // Track for deletion (DELETE on SaveChanges)
// For UPDATE — just change properties on the tracked entity, then SaveChanges

await _context.SaveChangesAsync()         // Commit all changes to DB
```

---

## 3. Migrations ✅ Implemented

### What are Migrations?
Migrations are **version-controlled database schema changes**. Instead of writing SQL manually, you change your C# model and EF Core generates the SQL for you.

### Migrations in DevConnect

#### Books Migrations (`Migrations/`)
| Migration | What it did |
|-----------|-------------|
| `20260421122813_book model added` | Created `Books` table with `Id`, `Title`, `Author`, `YearOfPublished` |
| `20260421124209_book data added` | Seeded 3 rows of data into `Books` table |

#### User Migrations (`Migrations/DevConnectDb/`)
| Migration | What it did |
|-----------|-------------|
| `20260427121252_user model added` | Created `Users` table with `Id`, `Name`, `Email`, `Age`, `Role` |
| `20260428121049_user model updated with jwt` | Added `PasswordHash`, `CreatedAt`; changed `Role` from `int` → `string` |
| `post model added` | Created `Posts` table with FK `UserId` → `Users.Id`, cascade delete |
| `likes and comments added` | Created `Likes` & `Comments` tables with FKs to `Posts` and `Users`; unique index on `(PostId, UserId)` for Likes |

### Migration Commands

Since this project has **two DbContexts**, always specify which context:

```powershell
# Package Manager Console (Visual Studio)
Add-Migration "description" -Context DevConnectDbContext
Update-Database -Context DevConnectDbContext

Add-Migration "description" -Context FirstAPIContext
Update-Database -Context FirstAPIContext

# CLI
dotnet ef migrations add "description" --context DevConnectDbContext
dotnet ef database update --context DevConnectDbContext
```

### Migration File Structure
```
Migrations/
├── 20260421122813_book model added.cs        ← Up() creates table, Down() drops it
├── 20260421122813_book model added.Designer.cs  ← Auto-generated snapshot
├── 20260421124209_book data added.cs
├── 20260421124209_book data added.Designer.cs
├── FirstAPIContextModelSnapshot.cs           ← Current state of FirstAPIContext schema
└── DevConnectDb/
    ├── 20260427121252_user model added.cs
    ├── 20260428121049_user model updated with jwt.cs
    └── DevConnectDbContextModelSnapshot.cs   ← Current state of DevConnectDbContext schema
```

### How a Migration Works
```
You change Model (Books.cs / User.cs)
        │
        ▼
Add-Migration → EF Core compares model to snapshot
        │
        ▼
Generates Up() method  → SQL to apply change
Generates Down() method → SQL to roll back change
        │
        ▼
Update-Database → Runs Up() on SQL Server
```

---

## 4. Relationships ✅ Implemented

### What are Relationships?
Relationships define how tables connect to each other using **Foreign Keys**.

| Type | Example |
|------|---------|
| **One-to-Many** | One `User` has many `Posts` ✅ |
| **One-to-Many** | One `Post` has many `Comments` ✅ |
| **One-to-Many** | One `Post` has many `Likes` ✅ |
| **Many-to-Many** | Many `Users` can like many `Posts` (via `Likes` join table) ✅ |
| **One-to-One** | One `User` has one `Profile` ❌ Not yet |

### Implemented: User → Posts (One-to-Many)

One `User` can create many `Posts`. Each `Post` belongs to exactly one `User`.

```
Users                    Posts
─────────────────        ──────────────────────────
Id (PK)          ◄───── UserId (FK)
Name                     Id (PK)
Email                    Title
PasswordHash             Content
Role                     CreatedAt
CreatedAt
Age
```

#### `Models/Post.cs`
```csharp
public class Post
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Key
    public int UserId { get; set; }

    // Navigation Property — EF Core uses this to JOIN tables
    public User User { get; set; } = null!;
}
```

#### `Models/User.cs` (navigation property added)
```csharp
public class User
{
    public int Id { get; set; }
    // ... existing fields ...

    // Navigation Property (collection)
    public List<Post> Posts { get; set; } = new();
}
```

#### `Data/DevConnectDbContext.cs` (relationship configured)
```csharp
public DbSet<User> Users { get; set; }
public DbSet<Post> Posts { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Post>()
        .HasOne(p => p.User)            // Post has one User
        .WithMany(u => u.Posts)         // User has many Posts
        .HasForeignKey(p => p.UserId)   // FK column
        .OnDelete(DeleteBehavior.Cascade); // Delete posts when user is deleted
}
```

#### Querying with `Include` (JOIN)
```csharp
// Get all posts with author name — used in PostsController
var posts = await _context.Posts
    .Include(p => p.User)
    .Select(p => new PostResponseDTO
    {
        Id = p.Id,
        Title = p.Title,
        Content = p.Content,
        CreatedAt = p.CreatedAt,
        AuthorName = p.User.Name   // ← from the JOIN
    })
    .ToListAsync();
```

### Posts API Endpoints
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/posts` | Public | Get all posts with author names |
| `GET` | `/api/posts/{id}` | Public | Get single post |
| `GET` | `/api/posts/my` | 🔒 JWT | Get only your posts |
| `POST` | `/api/posts` | 🔒 JWT | Create a post |
| `PUT` | `/api/posts/{id}` | 🔒 JWT | Update your own post |
| `DELETE` | `/api/posts/{id}` | 🔒 JWT / Admin | Delete own post or any (Admin) |

---

### Implemented: Post → Likes (One-to-Many / Unique constraint)

One `Post` can have many `Likes`. A `User` can only like a `Post` once (unique index on `PostId + UserId`).

```
Users            Likes              Posts
─────────        ─────────────      ─────────────
Id (PK)  ◄────  UserId (FK)        Id (PK)
                 PostId (FK)  ────► Id (PK)
                 Id (PK)
                 CreatedAt
```

#### Key config in `DevConnectDbContext`
```csharp
// Prevent duplicate likes
modelBuilder.Entity<Like>()
    .HasIndex(l => new { l.PostId, l.UserId })
    .IsUnique();
```

### Likes API Endpoints
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/posts/{postId}/likes` | Public | Get like count + liked by me |
| `POST` | `/api/posts/{postId}/likes` | 🔒 JWT | Toggle like / unlike |

---

### Implemented: Post → Comments (One-to-Many)

One `Post` can have many `Comments`. Each `Comment` belongs to one `Post` and one `User`.

```
Users            Comments           Posts
─────────        ─────────────      ─────────────
Id (PK)  ◄────  UserId (FK)        Id (PK)
                 PostId (FK)  ────► Id (PK)
                 Id (PK)
                 Content
                 CreatedAt
                 UpdatedAt
```

### Comments API Endpoints
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/posts/{postId}/comments` | Public | Get all comments on a post |
| `POST` | `/api/posts/{postId}/comments` | 🔒 JWT | Add a comment |
| `PUT` | `/api/posts/{postId}/comments/{id}` | 🔒 JWT | Edit own comment |
| `DELETE` | `/api/posts/{postId}/comments/{id}` | 🔒 JWT / Admin | Delete comment |

---

## 5. ACID ⚠️ Partial (Handled by EF Core + SQL Server)

### What is ACID?
ACID is a set of properties that guarantee **database transactions are reliable**.

| Property | Meaning | Example |
|----------|---------|---------|
| **Atomicity** | All or nothing — if one step fails, all steps roll back | Register user: insert user + send email. If email fails, user insert is rolled back |
| **Consistency** | Data always moves from one valid state to another | You can't insert a `Post` with a `UserId` that doesn't exist |
| **Isolation** | Concurrent transactions don't interfere with each other | Two users registering at the same time don't corrupt each other's data |
| **Durability** | Once committed, data survives crashes | After `SaveChangesAsync()` returns, data is safe even if server restarts |

### In DevConnect
EF Core and SQL Server **automatically provide ACID** for single `SaveChangesAsync()` calls.

```csharp
// This is automatically atomic — either both succeed or neither does
_context.Users.Add(user);
await _context.SaveChangesAsync();  // ← Single transaction
```

### When You Need Explicit Transactions
If you have **multiple SaveChangesAsync() calls** that must succeed together:

```csharp
// Example: Register user AND create default profile — both must succeed or both fail
using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    _context.Users.Add(user);
    await _context.SaveChangesAsync();

    _context.Profiles.Add(new Profile { UserId = user.Id });
    await _context.SaveChangesAsync();

    await transaction.CommitAsync();   // ← Both committed together
}
catch
{
    await transaction.RollbackAsync(); // ← Both rolled back on error
    throw;
}
```

> Currently in DevConnect, explicit transactions are **not used** because each operation is a single `SaveChangesAsync()`.

---

## 6. CAP Theorem ❌ Not Applicable (Theory Only)

### What is CAP Theorem?
CAP Theorem states that a **distributed database** can only guarantee **2 of these 3** properties at the same time:

| Property | Meaning |
|----------|---------|
| **Consistency (C)** | Every read returns the most recent write |
| **Availability (A)** | Every request gets a response (not guaranteed to be latest) |
| **Partition Tolerance (P)** | System keeps working even if network between nodes fails |

```
         Consistency
              /\
             /  \
            /    \
           / CA   \
          /        \
         /----CP----|
        /            \
Availability -------- Partition
       AP              Tolerance
```

### Common Database Choices
| Database | Chooses | Used When |
|----------|---------|-----------|
| SQL Server, PostgreSQL | **CA** (no partition tolerance) | Single server, financial data |
| MongoDB (default) | **CP** or **AP** | Distributed, high scale |
| Cassandra | **AP** | Always available, eventual consistency ok |
| DynamoDB | **AP** | Global scale, availability priority |

### Why Not Applicable in DevConnect
DevConnect uses **SQL Server on a single server** — there is no distributed setup, so CAP Theorem doesn't apply here. It becomes relevant when you:
- Run multiple database replicas
- Use microservices with separate databases
- Need global/geo-distributed data

### When You Would Apply It
If DevConnect scaled to use multiple SQL Server replicas or moved to a NoSQL database for high-traffic scenarios, you would need to decide:
- **CP** (bank-like: correct data > availability) → choose Consistency + Partition Tolerance
- **AP** (social feed: show something > wait for perfect data) → choose Availability + Partition Tolerance

---

## What's Next

```
✅ Post model with User FK relationship
✅ PostsController with full CRUD
✅ Like model with unique constraint (PostId + UserId)
✅ LikesController with toggle like/unlike
✅ Comment model with Post + User FK
✅ CommentsController with full CRUD

Next ideas to build on:
→ Pagination — add page/size params to GET /api/posts and GET /api/posts/{id}/comments
→ Filtering  — GET /api/posts?userId=1 or ?search=keyword
→ Profile    — One-to-One: User has one Profile (bio, avatar, etc.)
→ Follow     — Many-to-Many: User follows User
```
