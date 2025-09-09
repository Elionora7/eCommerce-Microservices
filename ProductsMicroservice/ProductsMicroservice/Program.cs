using eCommerce.ProductsMicroService.API.Middleware;
using eCommerce.ProductsService.BusinessLogicLayer;
using eCommerce.ProductsService.DataAccessLayer;
using FluentValidation.AspNetCore;
using eCommerce.ProductsMicroService.API.APIEndpoints;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using eCommerce.DataAccessLayer.Context;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Load .env file
var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env");
if (File.Exists(envPath))
{
    Console.WriteLine($"Loading .env file from: {envPath}");
    Env.Load(envPath);
}
else
{
    Console.WriteLine(".env file not found, using environment variables only");
}

// Add environment variables to configuration (Docker Compose > .env)
builder.Configuration.AddEnvironmentVariables();

// Determine if running in Docker container
bool isRunningInDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

// Map environment variables with environment-aware hostname
MapEnvironmentVariable("RABBITMQ_USERNAME", "RabbitMQ_UserName");
MapEnvironmentVariable("RABBITMQ_PASSWORD", "RabbitMQ_Password");
MapEnvironmentVariable("RABBITMQ_PORT", "RabbitMQ_Port");
MapEnvironmentVariable("RABBITMQ_PRODUCTS_EXCHANGE", "RabbitMQ_Products_Exchange");

// Handle RabbitMQ hostname based on environment
var rabbitMqHost = isRunningInDocker ?
    (builder.Configuration["RABBITMQ_HOSTNAME"] ?? "rabbitmq") :
    "localhost";

builder.Configuration["RabbitMQ_HostName"] = rabbitMqHost;
Console.WriteLine($"Set RabbitMQ_HostName to: {rabbitMqHost} (Running in Docker: {isRunningInDocker})");

void MapEnvironmentVariable(string sourceKey, string targetKey)
{
    var value = builder.Configuration[sourceKey];
    if (!string.IsNullOrEmpty(value))
    {
        builder.Configuration[targetKey] = value;
        Console.WriteLine($"Mapped {sourceKey} to {targetKey}: {value}");
    }
}

// Get environment variables from .env file
var mysqlHost = isRunningInDocker ?
    (builder.Configuration["MYSQL_HOST"] ?? "mysql") :
    "localhost";

var mysqlPort = builder.Configuration["MYSQL_PORT"] ?? "3306";
var mysqlDb = builder.Configuration["MYSQL_DATABASE"] ?? "ecommerceproductsdatabase";
var mysqlUser = builder.Configuration["MYSQL_USER"]
    ?? throw new InvalidOperationException("MYSQL_USER is required. Set it in .env or environment variables");

var mysqlPwd = builder.Configuration["MYSQL_PASSWORD"]
    ?? throw new InvalidOperationException("MYSQL_PASSWORD is required. Set it in .env or environment variables");

// Build connection string 
var connectionString = $"Server={mysqlHost};Port={mysqlPort};Database={mysqlDb};User ID={mysqlUser};Password={mysqlPwd}";

// Inject into configuration
builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;
Console.WriteLine($"Using MySQL host: {mysqlHost}");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 0))
    )
);

// Add DAL and BLL services
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddBusinessLogicLayer();

builder.Services.AddControllers();

// FluentValidations
builder.Services.AddFluentValidationAutoValidation();

// Add model binder to read values from JSON to enum
builder.Services.ConfigureHttpJsonOptions(options => {
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Cors - Read from .env or use defaults
var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")?.Split(',')
    ?? new[] { "http://localhost:5173", "http://localhost:4200" };

builder.Services.AddCors(options => {
    options.AddDefaultPolicy(builder => {
        builder.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseExceptionHandlingMiddleware();
app.UseRouting();

// Cors
app.UseCors();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Auth
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapProductAPIEndpoints();

app.Run();