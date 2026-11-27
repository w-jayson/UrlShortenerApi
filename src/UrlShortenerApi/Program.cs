// Ponto de entrada da aplicação: registra os serviços no container de DI e configura o pipeline HTTP.
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using UrlShortenerApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Configura o EF Core com SQLite usando a connection string do appsettings.json.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=urlshortener.db"));

builder.Services.AddControllers();
builder.Services.AddOpenApi();
// Habilita respostas de erro padronizadas no formato RFC 9457 (ProblemDetails).
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Converte exceções não tratadas em respostas ProblemDetails padronizadas.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Exposto como partial class para permitir o uso de WebApplicationFactory<Program> nos testes de integração.
public partial class Program;
