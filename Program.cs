using Microsoft.EntityFrameworkCore;
using UrlShortenerApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Configurar o Entity Framework com Banco em Memória
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("UrlDb"));

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