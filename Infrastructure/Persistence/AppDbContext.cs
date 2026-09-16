using Microsoft.EntityFrameworkCore;
using UrlShortenerApi.Domain.Entities;

namespace UrlShortenerApi.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ShortenedUrl> ShortenedUrls { get; set; }
}
