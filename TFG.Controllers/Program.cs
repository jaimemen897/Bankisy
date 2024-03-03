using Microsoft.EntityFrameworkCore;
using TFG.Context.Context;
using TFG.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<UsersService>();
builder.Services.AddScoped<BankAccountService>();
builder.Services.AddDbContext<BankContext>(options =>
{
    options.UseNpgsql("Host=localhost;Database=bank;Username=postgres;Password=pass");
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
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "Users",
    pattern: "users/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "BankAccounts",
    pattern: "bankAccounts/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");
;

app.Run();