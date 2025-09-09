using eCommerce.Infrastructure;
using eCommerce.Core;
using eCommerce.API.Middlewares;
using System.Text.Json.Serialization;
using eCommerce.Core.Mappers;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DotNetEnv; 

var builder = WebApplication.CreateBuilder(args);

// Load .env file in Development environment
if (builder.Environment.IsDevelopment())
{
    Env.Load(); 
}

// Configuration setup
var configuration = builder.Configuration;

builder.Configuration.AddEnvironmentVariables();

//var jwtKey = builder.Configuration["Jwt:Key"]
 //   ?? throw new ArgumentNullException("JWT Key is missing. Set JWT_KEY in env or appsettings");

// Validate and read JWT settings
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
    ?? throw new InvalidOperationException("JWT Key is missing. Set JWT_KEY in environment variables.");

// Verify key length
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
if (keyBytes.Length < 16) // 16 bytes = 128 bits
{
    throw new ArgumentException($"JWT Key must be at least 128 bits. Current: {keyBytes.Length * 8} bits");
}

var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
    ?? throw new InvalidOperationException("JWT Issuer is missing. Set JWT_ISSUER in environment variables.");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
    ?? throw new InvalidOperationException("JWT Audience is missing. Set JWT_AUDIENCE in environment variables.");

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// Add application services
builder.Services
    .AddInfrastructure()
    .AddCore();

// Configure Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });


//auto mapper for the same assembly profile will be executed for all the mapper under this assembly

builder.Services.AddAutoMapper(typeof(ApplicationUserMappingProfile).Assembly);


//FluentValidations
builder.Services.AddFluentValidationAutoValidation();

//Add API explorer services
builder.Services.AddEndpointsApiExplorer();

//Add swagger generation services to create swagger specification
builder.Services.AddSwaggerGen();

//Add cors services
// Enable both Angular and React/Vite
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder => {
        builder.WithOrigins(
                "http://localhost:5173",  // React
                "http://localhost:4200"   // Angular
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); //tokens
    });
});

//Build the web application
var app = builder.Build();

app.UseExceptionHandlingMiddleware();

//Routing
app.UseRouting();

//enable cors
app.UseCors();

//Auth
app.UseAuthentication();
app.UseAuthorization();

//Add endpoint that can serve the sawgger.json
app.UseSwagger();

//Add swagger UI 
app.UseSwaggerUI();


//Controller routes
app.MapControllers();

app.Run();