using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;
using System.Text;
using DotNetEnv; 

var builder = WebApplication.CreateBuilder(args);


builder.Configuration.AddEnvironmentVariables();

//Load environment variables first
if (builder.Environment.IsDevelopment())
{
    Env.Load("../.env"); // Load from solution root
    Console.WriteLine("APIGateway .env loaded. JWT Key Length: " +
        (Environment.GetEnvironmentVariable("JWT__KEY")?.Length ?? 0));
}

// Configuration setup 
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddJsonFile("secrets.json", optional: true) 
    .AddEnvironmentVariables() // Overrides everything
    .Build();

// JWT Configuration Validation
var jwtKey = configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key not configured.");
if (Encoding.UTF8.GetByteCount(jwtKey) < 16) // 128 bits minimum
{
    throw new ArgumentException(
        $"JWT Key too short. Requires 128+ bits. Current: {Encoding.UTF8.GetByteCount(jwtKey) * 8} bits");
}

// Authentication (matches UsersMicroservice)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero // Strict expiry validation
        };
    });

//Ocelot and Polly
builder.Services
    .AddOcelot(configuration)
    .AddPolly();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

await app.UseOcelot();

app.Run();