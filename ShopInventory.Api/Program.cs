using Microsoft.EntityFrameworkCore;
using ShopInventory.Api.Data;
using ShopInventory.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOptions<AnalyticsOptions>().BindConfiguration("Analytics")
    .Validate(options => System.Text.RegularExpressions.Regex.IsMatch(options.Currency, "^[A-Z]{3}$"), "Currency must be an ISO currency code")
    .Validate(options => TimeZoneInfo.TryFindSystemTimeZoneById(options.Timezone, out _), "Timezone must be valid")
    .ValidateOnStart();
builder.Services.AddScoped<AnalyticsService>();
builder.Services.AddSingleton<SalesAnalyticsSource>();
builder.Services.AddHostedService<InventorySnapshotWorker>();
builder.Services.AddScoped<ProductsService>();
builder.Services.AddScoped<CategoriesService>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is not configured.");

builder.Services.AddDbContext<ShopInventoryContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Adjust to your frontend URL
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowAngular");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
