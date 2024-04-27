using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using TFG.Context.Context;
using TFG.Controllers.ExceptionsHandler;
using TFG.Services;
using TFG.Services.Hub;
using BankAccountService = TFG.Services.BankAccountService;
using CardService = TFG.Services.CardService;

var myAllowSpecificOrigins = "AllowAngularApp";
Env.Load();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddSignalR();
builder.Services.AddProblemDetails();
builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<UsersService>();
builder.Services.AddScoped<BankAccountService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<IndexService>();
builder.Services.AddScoped<CardService>();
builder.Services.AddMvc().AddNewtonsoftJson();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDbContext<BankContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnectionString"));
});
builder.Services.AddProblemDetails();
/*builder.Services.AddCors(options =>
{
    options.AddPolicy(myAllowSpecificOrigins,
        origins => { origins.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); });
});*/

builder.Services.AddCors(options =>
{
    options.AddPolicy(myAllowSpecificOrigins,
        builder =>
        {
            builder.WithOrigins("https://localhost:44464")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
        ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
        IssuerSigningKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET") ??
                                                            throw new InvalidOperationException()))
    };
});
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("User",
        policy => policy.RequireAssertion(context => context.User.IsInRole("User") || context.User.IsInRole("Admin")))
    .AddPolicy("Admin", policy => policy.RequireRole("Admin"));

builder.Services.AddSingleton<IUserIdProvider, NameUserIdProvider>();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"); });
}

StripeConfiguration.ApiKey =
    "sk_test_51P7eS8D74icxIHcU4kn0dVmFuoZQhnf4gbAydb4NTzXzfI0oJTFjliD1H46CNyf2yrBuon0v3RwcHpJiUGkOZTYB00btmbH4Ic";
/*app.UseHttpsRedirection();*/
app.UseExceptionHandler();
app.UseCors(myAllowSpecificOrigins);
app.UseWebSockets();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.MapHub<MyHub>("/myHub");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("index.html");
app.Run();