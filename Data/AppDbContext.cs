using UrlShortenerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace UrlShortenerApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<ShortenedUrl> ShortenedUrls { get; set; }
}
