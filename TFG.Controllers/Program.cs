using Microsoft.EntityFrameworkCore;
using TFG.Context.Context;
using TFG.Services;

DotNetEnv.Env.Load();
var host = Environment.GetEnvironmentVariable("POSTGRES_HOST");
var port = Environment.GetEnvironmentVariable("POSTGRES_PORT");
var database = Environment.GetEnvironmentVariable("DATABASE_NAME");
var user = Environment.GetEnvironmentVariable("DATABASE_USER");
var password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD");
var connectionString = $"Host={host};Port={port};Database={database};Username={user};Password={password}";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<UsersService>();
builder.Services.AddScoped<BankAccountService>();
builder.Services.AddDbContext<BankContext>(options =>
{
    options.UseNpgsql(connectionString);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

/*app.UseHttpsRedirection();*/
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "Users",
    pattern: "users/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "BankAccounts",
    pattern: "bankAccounts/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.Run();