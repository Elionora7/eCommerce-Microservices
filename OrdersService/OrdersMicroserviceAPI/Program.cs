using eCommerce.OrdersMicroservice.DataAccessLayer;
using eCommerce.OrdersMicroservice.BusinessLogicLayer;
using FluentValidation.AspNetCore;
using eCommerce.OrdersMicroservice.API.Middleware;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

var builder = WebApplication.CreateBuilder(args);

//Add DAL and BLL services
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddBusinessLogicLayer(builder.Configuration);

builder.Services.AddControllers();

//FluentValidations
builder.Services.AddFluentValidationAutoValidation();

//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Cors
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:4200")
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});

builder.Services.AddTransient<IUserMicroservicePolicies, UserMicroservicePolicies>();
builder.Services.AddTransient<IProductMicroservicePolicies, ProductMicroservicePolicies>();
builder.Services.AddTransient<IPollyPolicies, PollyPolicies>();


//calling usermicroservice
builder.Services.AddHttpClient<UserMicroserviceClient>(client =>
{
    client.BaseAddress = new Uri($"http://{builder.Configuration["UserMicroserviceName"]}:" +
        $"{builder.Configuration["UserMicroservicePort"]}");
})
.AddPolicyHandler(
   builder.Services.BuildServiceProvider().GetRequiredService<IUserMicroservicePolicies>().GetCombinedPolicy())
;



builder.Services.AddHttpClient<ProductMicroserviceClient>(client => {
    client.BaseAddress = new Uri($"http://{builder.Configuration["ProductMicroserviceName"]}:" +
        $"{builder.Configuration["ProductMicroservicePort"]}");
}).AddPolicyHandler(
    builder.Services.BuildServiceProvider().GetRequiredService<IProductMicroservicePolicies>().GetFallbackPolicy()
)
.AddPolicyHandler(
    builder.Services.BuildServiceProvider().GetRequiredService<IProductMicroservicePolicies>().GetBulkheadIsolationPolicy()
)
;

var app = builder.Build();

app.UseExceptionHandlingMiddleware();
app.UseRouting();

//Cors
app.UseCors();

//Swagger
app.UseSwagger();
app.UseSwaggerUI();

//Auth
//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

//Endpoints
app.MapControllers();


app.Run();