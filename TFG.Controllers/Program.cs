using Microsoft.EntityFrameworkCore;
using TFG.Context.Context;
using TFG.Services;

var  MyAllowSpecificOrigins = "AllowAngularApp";
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
builder.Services.AddScoped<TransactionService>();
builder.Services.AddDbContext<BankContext>(options => { options.UseNpgsql(connectionString); });
/*builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        builder => builder.WithOrigins("https://localhost:44464") // Replace with your Angular app's URL
            .AllowAnyHeader()
            .AllowAnyMethod());
});*/
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy => { policy.WithOrigins("https://localhost:44464").AllowAnyHeader().AllowAnyMethod(); });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"); });
}

app.UseDefaultFiles();
app.UseStaticFiles();
/*app.UseHttpsRedirection();*/
app.UseStaticFiles();
app.UseRouting();
app.UseCors(MyAllowSpecificOrigins);

app.MapControllerRoute(
    name: "Users",
    pattern: "users/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "BankAccounts",
    pattern: "bankAccounts/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "Transactions",
    pattern: "transactions/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.Run();