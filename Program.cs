using Microsoft.EntityFrameworkCore;
using UrlShortenerApi.Application.Abstractions;
using UrlShortenerApi.Application.UseCases.CreateShortUrl;
using UrlShortenerApi.Application.UseCases.ResolveShortUrl;
using UrlShortenerApi.Infrastructure.Persistence;
using UrlShortenerApi.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar o Entity Framework com Banco em Memória
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("UrlDb"));

builder.Services.AddScoped<IShortenedUrlRepository, ShortenedUrlRepository>();
builder.Services.AddSingleton<IBase62Service, Base62Service>();
builder.Services.AddScoped<CreateShortUrlHandler>();
builder.Services.AddScoped<ResolveShortUrlHandler>();

builder.Services.AddControllers();

// Configurar o NSwag
builder.Services.AddOpenApiDocument(config =>
{
    config.PostProcess = document =>
    {
        document.Info.Title = "URL Shortener API";
        document.Info.Description = "API simples para encurtar URLs com strings aleatórias.";
    };
});

var app = builder.Build();


app.UseOpenApi();
app.UseSwaggerUi();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
