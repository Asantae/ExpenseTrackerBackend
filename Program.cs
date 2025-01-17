using System.Text;
using ExpenseTrackerBackend.Repositories;
using ExpenseTrackerBackend.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
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
        policy.WithOrigins("http://localhost:3000") 
              .AllowAnyHeader()                     
              .AllowAnyMethod() 
              .AllowCredentials();                
    });
});

builder.Services.AddScoped<ExpenseRepository>(provider =>
{
    return new ExpenseRepository(connectionString);
});

builder.Services.AddSingleton<JwtTokenUtility>(provider =>
{
    return new JwtTokenUtility(jwtSecretKey, refreshTokenSecretKey, jwtIssuer, connectionString);
});

var app = builder.Build();

app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    IdentityModelEventSource.ShowPII = true;
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();