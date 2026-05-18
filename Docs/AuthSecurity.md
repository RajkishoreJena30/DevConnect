# Authentication & Security Concepts in DevConnect

---

## Status Summary

| Concept | Implemented? | Where |
|---------|-------------|-------|
| **OAuth2** | ⚠️ Partial | JWT Bearer flow only — no OAuth2 server, no external providers |
| **OIDC (OpenID Connect)** | ❌ No | Not implemented — no external identity provider |
| **JWT** | ✅ Yes | `AuthController.cs` — generation & validation |
| **ASP.NET Core Identity** | ❌ No | Custom User model + BCrypt used instead |
| **Roles** | ✅ Yes | `User.Role` string field — `"User"` / `"Admin"` |
| **Groups** | ❌ No | No group concept — only individual roles |
| **Permissions** | ⚠️ Partial | Role-based only — no fine-grained permission system |
| **Authentication** | ✅ Yes | JWT Bearer via `UseAuthentication()` + `[Authorize]` |
| **Authorization** | ✅ Yes | `[Authorize]`, `[Authorize(Roles = "Admin")]`, ownership checks |

---

## 1. OAuth2 ⚠️ Partial

### What is OAuth2?
OAuth2 is an **authorization framework** that defines flows for obtaining access tokens. It has several grant types:

| Grant Type | Use Case |
|-----------|----------|
| Authorization Code | Web apps — user logs in via external provider (Google, GitHub) |
| Client Credentials | Machine-to-machine (no user) |
| Resource Owner Password | User provides username/password directly to your API ← **this is what DevConnect does** |
| Implicit | Legacy SPAs (deprecated) |

### What DevConnect Uses
DevConnect implements the **Resource Owner Password** pattern:
- User sends `email + password` to `POST /api/auth/login`
- API validates credentials and returns a **JWT Bearer token**
- Client includes the token in `Authorization: Bearer <token>` header

This is OAuth2-**inspired** but not a full OAuth2 implementation — there is no authorization server, no token endpoint spec, no scope system.

### What's NOT Implemented
- No external login (Google, GitHub, Microsoft) — no `AddOAuth()` or `AddGoogle()`
- No authorization code flow
- No token refresh endpoint
- No token introspection or revocation

---

## 2. OIDC (OpenID Connect) ❌ Not Implemented

### What is OIDC?
OpenID Connect is an **identity layer built on top of OAuth2**. It adds:
- **ID Token** (a JWT that proves who the user is)
- **UserInfo endpoint** to get profile data
- Standard claims: `sub`, `name`, `email`, `picture`

### Current State
DevConnect issues its own JWTs with custom claims but does **not** follow the OIDC spec. There is no:
- `/.well-known/openid-configuration` discovery endpoint
- `id_token` alongside an access token
- External identity provider (Google, Azure AD, Auth0)

### How to Add OIDC (future)
```csharp
// Program.cs — add Google OIDC login
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = "...";
        options.ClientSecret = "...";
    });
```

---

## 3. JWT (JSON Web Token) ✅ Implemented

### What is JWT?
A JWT is a compact, self-contained token with three parts: `Header.Payload.Signature`

```
eyJhbGciOiJIUzI1NiJ9   ← Header (algorithm)
.eyJzdWIiOiIxIn0       ← Payload (claims)
.SflKxwRJSMeKKF2QT4fw  ← Signature (HMAC-SHA256)
```

### Token Generation — `AuthController.cs`
```csharp
private string GenerateToken(User user)
{
    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]!));

    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),  // sub
        new Claim(ClaimTypes.Name, user.Name),                     // name
        new Claim(ClaimTypes.Email, user.Email),                   // email
        new Claim(ClaimTypes.Role, user.Role)                      // role → "User" or "Admin"
    };

    var token = new JwtSecurityToken(
        issuer: _config["JwtSettings:Issuer"],          // "DevConnect"
        audience: _config["JwtSettings:Audience"],      // "DevConnectUsers"
        claims: claims,
        expires: DateTime.UtcNow.AddDays(
            int.Parse(_config["JwtSettings:ExpiryInDays"]!)),  // 7 days
        signingCredentials: new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256)
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

### Token Validation — `Program.cs`
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,               // rejects expired tokens
            ValidateIssuerSigningKey = true,       // verifies signature
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!))
        };
    });
```

### JWT Settings — `appsettings.json`
```json
"JwtSettings": {
  "Key": "",                    // ← secret key (stored in appsettings.Development.json, gitignored)
  "Issuer": "DevConnect",
  "Audience": "DevConnectUsers",
  "ExpiryInDays": 7
}
```

### Reading Claims in Controllers
```csharp
// Extract userId from token (used in PostsController, UsersController, etc.)
var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

// Extract role from token (used in PostsController.Delete)
var role = User.FindFirstValue(ClaimTypes.Role)!;
```

### JWT Flow in DevConnect
```
1. POST /api/auth/register  →  creates user with BCrypt hashed password
2. POST /api/auth/login     →  verifies password, returns JWT token
3. Client stores token
4. Client sends: Authorization: Bearer <token>  on every protected request
5. ASP.NET Core middleware validates token → populates User.Claims
6. [Authorize] checks if token is valid
7. [Authorize(Roles = "Admin")] checks ClaimTypes.Role claim
```

---

## 4. ASP.NET Core Identity ❌ Not Implemented

### What is ASP.NET Core Identity?
A built-in membership system that provides:
- `IdentityUser` model (Id, Email, PasswordHash, etc.)
- `UserManager<T>`, `SignInManager<T>` services
- Built-in password hashing, lockout, email confirmation
- Role management via `RoleManager<T>`

### Why DevConnect Does NOT Use It
DevConnect uses a **custom User model** with manual BCrypt hashing. This was a deliberate learning choice — building auth from scratch to understand how JWT works.

### Custom Implementation Instead
| ASP.NET Identity | DevConnect Custom |
|-----------------|------------------|
| `IdentityUser` | `Models/User.cs` |
| `UserManager.CreateAsync()` | `AuthController.Register()` |
| `SignInManager.CheckPasswordSignInAsync()` | `BC.Verify(dto.Password, user.PasswordHash)` |
| `UserManager.AddToRoleAsync()` | `user.Role = "Admin"` string field |
| Built-in password hash | `BCrypt.Net.BCrypt.HashPassword()` |

### How to Add Identity (future)
```csharp
// Program.cs
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<DevConnectDbContext>()
    .AddDefaultTokenProviders();
```

---

## 5. Roles ✅ Implemented

### What is a Role?
A role is a label assigned to a user that grants access to certain operations. DevConnect uses a simple string-based role system.

### Role Model — `Models/User.cs`
```csharp
public string Role { get; set; } = "User";  // default role on registration
```

Possible values: `"User"` (default) | `"Admin"`

### Role in JWT Token — `AuthController.cs`
```csharp
new Claim(ClaimTypes.Role, user.Role)  // embedded in token on login
```

### Role Enforcement — `UsersController.cs`
```csharp
// Any authenticated user can view/update their own profile
[Authorize]
public class UsersController : ControllerBase
{
    // GET /api/users — Admin only
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<User>>> GetAllUsers() { ... }

    // DELETE /api/users/{id} — Admin only
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(int id) { ... }
}
```

### Role Enforcement in Service Logic — `PostService.cs`
```csharp
// Delete post — owner OR Admin can delete
public async Task<bool> DeletePostAsync(int postId, int userId, string role)
{
    var post = await _repo.GetByIdAsync(postId);
    if (post == null) return false;
    if (post.UserId != userId && role != "Admin") return false;  // ← role check
    await _repo.DeleteAsync(post);
    return true;
}
```

### Endpoint Access Summary
| Endpoint | Required Role |
|----------|--------------|
| `GET /api/posts` | Public |
| `POST /api/posts` | Any authenticated user |
| `DELETE /api/posts/{id}` | Owner OR Admin |
| `GET /api/users` | Admin only |
| `DELETE /api/users/{id}` | Admin only |
| `GET /api/users/profile` | Any authenticated user (own profile) |

---

## 6. Groups ❌ Not Implemented

### What are Groups?
Groups allow assigning multiple users to a named set — e.g., `"Moderators"`, `"PremiumUsers"`. A user inherits all permissions of their group.

### Current State
DevConnect has no group concept. Role is a single string per user — a user can only have one role at a time.

### How to Add Groups (future)
```csharp
// Many-to-many: User ↔ Group
public class Group { public int Id; public string Name; public List<User> Users; }
public class User { ... public ICollection<Group> Groups { get; set; } }

// Then check membership in policy:
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Moderator", p => p.RequireAssertion(ctx =>
        ctx.User.IsInRole("Admin") || ctx.User.IsInRole("Moderator")));
});
```

---

## 7. Permissions ⚠️ Partial (Role-Based Only)

### What are Permissions?
Permissions define specific actions a user can perform — finer-grained than roles.
- **Role-based (RBAC):** `Admin` can delete any post
- **Permission-based:** `post:delete:any` — specific named permission

### What DevConnect Has
DevConnect implements **RBAC** (Role-Based Access Control) only:
- `[Authorize]` — any authenticated user
- `[Authorize(Roles = "Admin")]` — Admin role required
- Ownership check — `post.UserId == userId` (can only edit your own content)

### What's Missing
- No `[Authorize(Policy = "...")]` policies
- No claim-based permissions (`permission:post:delete`)
- No permission table in the database

### How to Add Policy-Based Permissions (future)
```csharp
// Program.cs — define policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanDeleteAnyPost", policy =>
        policy.RequireRole("Admin", "Moderator"));

    options.AddPolicy("PremiumUser", policy =>
        policy.RequireClaim("subscription", "premium"));
});

// Controller — use policy
[Authorize(Policy = "CanDeleteAnyPost")]
public async Task<IActionResult> Delete(int id) { ... }
```

---

## 8. Authentication ✅ Implemented

### What is Authentication?
Authentication answers: **"Who are you?"** — verifying identity.

### How It Works in DevConnect

#### Step 1 — Registration
```
POST /api/auth/register
Body: { "name": "John", "email": "j@j.com", "password": "Pass@123" }

→ BCrypt hashes password → stored in Users table
→ JWT token returned immediately
```

#### Step 2 — Login
```
POST /api/auth/login
Body: { "email": "j@j.com", "password": "Pass@123" }

→ BC.Verify(dto.Password, user.PasswordHash) → true/false
→ JWT token returned on success
→ 401 Unauthorized on failure
```

#### Step 3 — Middleware validates token on every request
```csharp
// Program.cs — order matters!
app.UseAuthentication();   // reads Authorization header, validates JWT, populates User.Claims
app.UseAuthorization();    // checks [Authorize] attributes using populated User.Claims
```

#### Step 4 — Token carries identity
```csharp
// Any controller can read identity without hitting the database:
var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
var name   = User.FindFirstValue(ClaimTypes.Name);
var email  = User.FindFirstValue(ClaimTypes.Email);
var role   = User.FindFirstValue(ClaimTypes.Role);
```

### Password Hashing — BCrypt
```csharp
// Registration — hash before storing
user.PasswordHash = BC.HashPassword(dto.Password);
// BCrypt generates a random salt and embeds it in the hash

// Login — verify against stored hash
bool valid = BC.Verify(dto.Password, user.PasswordHash);
// BCrypt extracts salt from hash and re-hashes for comparison
```

> ⚠️ **Never store plain text passwords.** BCrypt is intentionally slow to resist brute-force attacks.

---

## 9. Authorization ✅ Implemented

### What is Authorization?
Authorization answers: **"What are you allowed to do?"** — enforcing access rules after identity is confirmed.

### Three Layers of Authorization in DevConnect

#### Layer 1 — Unauthenticated check (`[Authorize]`)
```csharp
[HttpPost]
[Authorize]   // ← rejects requests with no/invalid JWT → 401
public async Task<IActionResult> Create(CreatePostDTO dto) { ... }
```

#### Layer 2 — Role check (`[Authorize(Roles = "Admin")]`)
```csharp
[HttpGet]
[Authorize(Roles = "Admin")]   // ← valid JWT required + Role claim must be "Admin" → 403 if not
public async Task<ActionResult<List<User>>> GetAllUsers() { ... }
```

#### Layer 3 — Ownership check (business logic in service)
```csharp
// PostService.cs — not a decorator, but logic in code
public async Task<bool> UpdatePostAsync(int postId, int userId, CreatePostDTO dto)
{
    var post = await _repo.GetByIdAsync(postId);
    if (post == null || post.UserId != userId) return false;  // only owner can update
    ...
}
```

### HTTP Response Codes
| Scenario | Code | Reason |
|----------|------|--------|
| No token on `[Authorize]` endpoint | `401 Unauthorized` | Not authenticated |
| Valid token but wrong role | `403 Forbidden` | Authenticated but not authorized |
| Valid token, correct role, not owner | `404 Not Found` | Hides existence of resource |

### Controller-Level vs Method-Level
```csharp
[Authorize]                   // ← applies to ALL methods in this controller
public class UsersController : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin")]  // ← additional restriction on this specific method
    public async Task<ActionResult<List<User>>> GetAllUsers() { ... }
}
```

---

## Summary — What's Built vs What's Missing

```
✅ BUILT
  JWT Bearer authentication (custom implementation)
  BCrypt password hashing
  Role-based authorization ("User" / "Admin")
  Ownership-based authorization in service layer
  Middleware pipeline (UseAuthentication → UseAuthorization)
  Claims extraction in controllers

⚠️ PARTIAL
  OAuth2 — Resource Owner Password flow only (no full OAuth2 server)
  Permissions — role-based only, no fine-grained permission policies

❌ NOT BUILT
  OIDC / external identity providers (Google, GitHub, Azure AD)
  ASP.NET Core Identity (using custom model instead)
  Groups / multi-role users
  Policy-based authorization (AddAuthorization + AddPolicy)
  Refresh tokens
  Token revocation / blacklist
```
