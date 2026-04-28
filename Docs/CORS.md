# CORS — Cross-Origin Resource Sharing

## What is CORS?

**CORS** is a **browser security feature** that blocks requests made from one origin to a different origin unless the server explicitly allows it.

An **origin** is the combination of:
```
protocol + domain + port
http://localhost:3000
│         │         └── port
│         └── domain
└── protocol
```

---

## Why Does It Matter?

```
Frontend: http://localhost:3000   (React / Angular)
Backend:  https://localhost:7238  (DevConnect API)
```

These are **different origins** (different port). The browser will block the request unless the API says *"I allow requests from localhost:3000"*.

> ⚠️ CORS is a **browser-only** restriction.  
> Postman, Swagger, and curl are **not affected** by CORS.

---

## What Counts as a Different Origin?

| Request From | API At | Different? | Reason |
|---|---|---|---|
| `http://localhost:3000` | `https://localhost:7238` | ✅ Yes | Different port |
| `http://myapp.com` | `https://myapp.com` | ✅ Yes | Different protocol |
| `http://myapp.com` | `http://myapi.com` | ✅ Yes | Different domain |
| `http://myapp.com` | `http://myapp.com` | ❌ No | Same origin |

---

## How CORS Works (Browser Flow)

```
1. Browser sends a preflight OPTIONS request to API
        OPTIONS /api/users
        Origin: http://localhost:3000

2. API responds with allowed origins
        Access-Control-Allow-Origin: http://localhost:3000
        Access-Control-Allow-Methods: GET, POST, PUT, DELETE
        Access-Control-Allow-Headers: Content-Type, Authorization

3. Browser checks response
        ✅ Origin allowed → sends actual request
        ❌ Origin not allowed → blocks request (CORS error)
```

---

## Implementation in DevConnect

### `Program.cs`

```csharp
// Step 1 — Register CORS policy (in services)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",   // React default
                "http://localhost:4200"    // Angular default
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Step 2 — Apply CORS middleware (in pipeline)
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");  // ← Must be before Auth
app.UseAuthentication();
app.UseAuthorization();
```

---

## CORS Methods Explained

| Method | Purpose |
|--------|---------|
| `WithOrigins("url")` | Whitelist specific frontend URLs |
| `AllowAnyOrigin()` | Allow ALL origins — ⚠️ not safe for production |
| `AllowAnyHeader()` | Allow any request header (e.g. `Authorization`, `Content-Type`) |
| `AllowAnyMethod()` | Allow GET, POST, PUT, DELETE, PATCH etc. |
| `AllowCredentials()` | Allow cookies and `Authorization` headers with JWT tokens |
| `WithMethods("GET","POST")` | Restrict to specific HTTP methods only |
| `WithHeaders("Authorization")` | Restrict to specific headers only |

> ⚠️ `AllowAnyOrigin()` and `AllowCredentials()` **cannot be used together** — it's a security violation and .NET will throw an exception.

---

## Middleware Order — Critical!

```csharp
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");   // ✅ FIRST — before auth
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

If `UseCors()` is placed **after** `UseAuthentication()`, CORS headers won't be added to failed auth responses. The browser will show a **CORS error** instead of a **401 Unauthorized**, making it very hard to debug.

---

## Multiple CORS Policies

You can define different policies for different controllers:

```csharp
builder.Services.AddCors(options =>
{
    // Strict policy for public endpoints
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod());

    // Open policy for public read-only endpoints
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});
```

Apply per controller:
```csharp
[EnableCors("AllowAll")]
[HttpGet]
public async Task<ActionResult<List<Books>>> GetBooks() { ... }
```

---

## Production Checklist

- [ ] Never use `AllowAnyOrigin()` in production
- [ ] Always whitelist specific frontend domains
- [ ] Use environment-based origins (dev vs prod URLs)
- [ ] Keep `UseCors()` before `UseAuthentication()` in the pipeline
