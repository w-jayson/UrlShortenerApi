using Microsoft.EntityFrameworkCore;
using UrlShortenerApi.Data;
using UrlShortenerApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar o Entity Framework com Banco em Memória
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("UrlDb"));

builder.Services.AddSingleton<IBase62Service, Base62Service>();

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