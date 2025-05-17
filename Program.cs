using System.Text;
using System.Text.Json.Serialization;
using ExpenseTrackerBackend.Repositories;
using ExpenseTrackerBackend.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()  // Ensure logs are written to the console for Log Stream
    .WriteTo.File("/home/LogFiles/app_log.txt", rollingInterval: RollingInterval.Day)  // Optional file logging
    .CreateLogger();

Log.Information("Starting Expense Tracker Backend...");

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Host.UseSerilog();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
    
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var jwtSecretKey = builder.Configuration["Jwt:SecretKey"];
var refreshTokenSecretKey = builder.Configuration["Jwt:RefreshTokenSecretKey"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000",
            "https://asantaes-expense-tracker.netlify.app"
        ) 
            .AllowAnyHeader()                     
            .AllowAnyMethod() 
            .AllowCredentials();                
    });
});

builder.Services.AddScoped<ExpenseRepository>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<ExpenseRepository>>(); 
    return new ExpenseRepository(connectionString, logger);
});

builder.Services.AddScoped<UserRepository>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<UserRepository>>(); 
    return new UserRepository(connectionString, logger);
});

builder.Services.AddSingleton<JwtTokenUtility>(provider =>
{
    return new JwtTokenUtility(jwtSecretKey, refreshTokenSecretKey, jwtIssuer, connectionString);
});

builder.Services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute(500));
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug); // Ensure logging at Debug level

var app = builder.Build();

app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    IdentityModelEventSource.ShowPII = true;
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();  // Logs HTTP requests automatically
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();