using Microsoft.EntityFrameworkCore;
using UrlShortenerApi.Application.Abstractions;
using UrlShortenerApi.Application.UseCases.CreateShortUrl;
using UrlShortenerApi.Application.UseCases.ResolveShortUrl;
using UrlShortenerApi.Infrastructure.Persistence;
using UrlShortenerApi.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar o Entity Framework com SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=urlshortener.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddSingleton<IShortCodeGenerator, ShortCodeGenerator>();
builder.Services.AddScoped<CreateShortUrlHandler>();
builder.Services.AddScoped<ResolveShortUrlHandler>();

builder.Services.AddControllers();

// Configurar o NSwag
builder.Services.AddOpenApiDocument(config =>
{
    config.PostProcess = document =>
    {
        document.Info.Title = "URL Shortener API";
        document.Info.Description = "API para encurtar URLs com persistência de código curto, suporte a custom slugs e alta resiliência.";
    };
});

var app = builder.Build();

// Aplicar migrações pendentes e ativar o modo WAL no SQLite
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
    dbContext.Database.ExecuteSqlRaw("PRAGMA journal_mode = WAL;");
}

app.UseOpenApi();
app.UseSwaggerUi();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
