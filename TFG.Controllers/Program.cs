using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Stripe;
using TFG.Context.Context;
using TFG.Controllers.DataAccessor;
using TFG.Controllers.ExceptionsHandler;
using TFG.Services;
using TFG.Services.Hub;
using BankAccountService = TFG.Services.BankAccountService;
using CardService = TFG.Services.CardService;

var myAllowSpecificOrigins = "AllowAngularApp";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddSignalR();
builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options => { options.LogToStandardErrorThreshold = LogLevel.Information; });
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Bankisy API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter 'Bearer ' followed by a valid token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddMemoryCache();
builder.Services.AddScoped<UsersService>();
builder.Services.AddScoped<BankAccountService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<CardService>();
builder.Services.AddMvc().AddNewtonsoftJson();
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
builder.Services.AddDbContext<BankContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("ConnectionURL"));
    options.EnableSensitiveDataLogging();
});
builder.Services.AddProblemDetails();
builder.Services.AddCors(options =>
{
    options.AddPolicy(myAllowSpecificOrigins,
        build =>
        {
            build.WithOrigins("https://localhost:44464")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});
builder.Services.AddControllers(options =>
{
    options.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider());
}).AddNewtonsoftJson();
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"] ?? string.Empty))
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/myHub")) context.Token = accessToken;
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("User",
        policy => policy.RequireAssertion(context => context.User.IsInRole("User") || context.User.IsInRole("Admin")))
    .AddPolicy("Admin", policy => policy.RequireRole("Admin"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseSwagger();
    app.UseSwaggerUI();
}

StripeConfiguration.ApiKey =
    "sk_test_51P7eS8D74icxIHcU4kn0dVmFuoZQhnf4gbAydb4NTzXzfI0oJTFjliD1H46CNyf2yrBuon0v3RwcHpJiUGkOZTYB00btmbH4Ic";
app.UseHttpsRedirection();
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
public partial class Program { }