using AutoMapper;
using DevConnect.Data;
using DevConnect.Interfaces;
using DevConnect.Mappings;
using DevConnect.Repositories;
using DevConnect.Services;
using DevConnect.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

using Serilog;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build())
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, services, config) =>
    config.ReadFrom.Configuration(ctx.Configuration)
          .ReadFrom.Services(services));

// Add services to the container.

// CORS Policy
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

// FluentValidation — registers all validators (RegisterValidator, LoginValidator, etc.)
// Note: AddFluentValidationAutoValidation was removed in v11. Inject IValidator<T> manually in controllers.
builder.Services.AddValidatorsFromAssemblyContaining<RegisterValidator>();

builder.Services.AddControllers();

// Repository — Scoped means one instance per HTTP request
builder.Services.AddScoped<IPostRepository, PostRepository>();

// Service — Scoped for same reason
builder.Services.AddScoped<IPostService, PostService>();

// AutoMapper — scans assembly for all Profile classes
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "DevConnect API", Version = "v1" });

    // Add JWT Authentication to Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token. Example: eyJhbGci..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<FirstAPIContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<DevConnectDbContext>(option => option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register AuthService
builder.Services.AddScoped<IAuthService, AuthService>();



//JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!))
        };
    })// Google OIDC
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Google:ClientSecret"]!;

        // Extra fields we want from Google
        options.Scope.Add("email");
        options.Scope.Add("profile");
    })
    // GitHub OAuth
    .AddGitHub(options =>
    {
        options.ClientId = builder.Configuration["GitHub:ClientId"]!;
        options.ClientSecret = builder.Configuration["GitHub:ClientSecret"]!;

        // Extra fields we want from GitHub
        options.Scope.Add("user:email");
    });


builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("Posts", builder =>
        builder.Expire(TimeSpan.FromSeconds(30))
               .Tag("posts"));       // tag enables targeted invalidation
});

var app = builder.Build();

// Catch all unhandled exceptions and return a generic error response
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseSerilogRequestLogging(); // logs method, path, status, elapsed ms
app.UseCors("AllowFrontend");  // Must be before Authentication & Authorization
app.UseOutputCache();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
