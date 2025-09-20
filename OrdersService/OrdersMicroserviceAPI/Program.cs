using eCommerce.OrdersMicroservice.DataAccessLayer;
using eCommerce.OrdersMicroservice.BusinessLogicLayer;
using FluentValidation.AspNetCore;
using eCommerce.OrdersMicroservice.API.Middleware;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DotNetEnv;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// ========================
// Configuration Setup
// ========================
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

if (builder.Environment.IsDevelopment())
{
    Env.Load("../.env");
    Console.WriteLine("[OrdersMicroservice] .env loaded (Development only)");
}

// ========================
// JWT Configuration & Validation
// ========================
var jwtConfig = new
{
    Key = builder.Configuration["JWT_KEY"] ?? throw new InvalidOperationException("JWT_KEY not configured"),
    Issuer = builder.Configuration["JWT_ISSUER"] ?? "ecommerce-orders-service",
    Audience = builder.Configuration["JWT_AUDIENCE"] ?? "ecommerce-client"
};

// Validate key strength (minimum 256-bit for HS256)
if (Encoding.UTF8.GetByteCount(jwtConfig.Key) < 32)
{
    throw new ArgumentException(
        $"JWT Key too weak. Requires 256+ bits. Current: {Encoding.UTF8.GetByteCount(jwtConfig.Key) * 8} bits");
}

// ========================
// Services Registration
// ========================

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtConfig.Issuer,
            ValidAudience = jwtConfig.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Key)),
            ClockSkew = TimeSpan.Zero,
            RequireExpirationTime = true,
            RequireSignedTokens = true
        };
    });

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAuthenticatedUser", policy =>
        policy.RequireAuthenticatedUser());
});

// Swagger with JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Orders Microservice", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter JWT Bearer token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

// Application Services
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddBusinessLogicLayer(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173", // Vite
                "http://localhost:4200") // Angular
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// HTTP Clients with Polly
builder.Services.AddSingleton<IPollyPolicies, PollyPolicies>();
builder.Services.AddSingleton<IUserMicroservicePolicies, UserMicroservicePolicies>();
builder.Services.AddSingleton<IProductMicroservicePolicies, ProductMicroservicePolicies>();
builder.Services.AddHttpContextAccessor();

// ========================
// App Building
// ========================
var app = builder.Build();

// Middleware Pipeline
app.UseExceptionHandlingMiddleware();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Orders Microservice v1");
        c.OAuthClientId("swagger-ui");
        c.OAuthAppName("Swagger UI");
    });
}

app.MapControllers().RequireAuthorization();
app.Run();