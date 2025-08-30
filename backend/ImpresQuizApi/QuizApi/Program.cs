using Microsoft.OpenApi.Models;
using QuizApi.Implementations;
using QuizApi.Infrastructure;
using QuizApi.Middleware;
using QuizApi.Repositories;

// + FluentValidation wiring
using FluentValidation;
using FluentValidation.AspNetCore;
using QuizApi.Validators;

var builder = WebApplication.CreateBuilder(args);

// ---- Connection string
var cs = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(cs))
{
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection is missing or empty.");
}

// ---- Dapper/DB factory
builder.Services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>(_ => new SqlConnectionFactory(cs));

// ---- Repositories
builder.Services.AddScoped<IGameSessionRepository, GameSessionRepository>();
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IAnswerRepository, AnswerRepository>();
builder.Services.AddScoped<ILeaderboardRepository, LeaderboardRepository>();
builder.Services.AddScoped<IQueryRepository, QueryRepository>();
builder.Services.AddScoped<IContentRepository, ContentRepository>();

// ---- CORS (allow React dev server)
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5173", "http://localhost:5174" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("Dev", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
    // .AllowCredentials() // enable only if you later use cookies
    );
});

// ---- MVC / Controllers + FluentValidation
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // customize JSON if needed
        // options.JsonSerializerOptions.PropertyNamingPolicy = null;
    })
    .AddFluentValidation(); // adds automatic model validation

// discover validators in your assembly
builder.Services.AddValidatorsFromAssemblyContaining<SubmitAnswerRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<JoinPlayerRequestValidator>();

// ---- Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = builder.Configuration["Swagger:Title"] ?? "Impres Quiz API",
        Version = builder.Configuration["Swagger:Version"] ?? "v1"
    });
});

var app = builder.Build();

// ---- Global exception handler
app.UseMiddleware<ExceptionMiddleware>();

// ---- Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS + CORS + AuthZ (order matters: CORS before MapControllers)
app.UseHttpsRedirection();

app.UseCors("Dev");

// app.UseAuthentication(); // add later when you wire up JWT
app.UseAuthorization();

app.MapControllers();

app.Run();
