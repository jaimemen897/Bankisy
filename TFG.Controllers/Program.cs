using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TFG.Context.Context;
using TFG.Controllers.ExceptionsHandler;
using TFG.Services;

var myAllowSpecificOrigins = "AllowAngularApp";
Env.Load();
var host = Environment.GetEnvironmentVariable("POSTGRES_HOST");
var port = Environment.GetEnvironmentVariable("POSTGRES_PORT");
var database = Environment.GetEnvironmentVariable("DATABASE_NAME");
var user = Environment.GetEnvironmentVariable("DATABASE_USER");
var password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD");
var connectionString = $"Host={host};Port={port};Database={database};Username={user};Password={password}";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddExceptionHandler<ExceptionHandler>();
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
builder.Services.AddDbContext<BankContext>(options => { options.UseNpgsql(connectionString); });
builder.Services.AddProblemDetails();
builder.Services.AddCors(options =>
{
    options.AddPolicy(myAllowSpecificOrigins,
        origins => { origins.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); });
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

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"); });
}
Stripe.StripeConfiguration.ApiKey = "sk_test_51P7eS8D74icxIHcU4kn0dVmFuoZQhnf4gbAydb4NTzXzfI0oJTFjliD1H46CNyf2yrBuon0v3RwcHpJiUGkOZTYB00btmbH4Ic";
/*app.UseHttpsRedirection();*/
app.UseExceptionHandler();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseCors(myAllowSpecificOrigins);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("index.html");
app.Run();