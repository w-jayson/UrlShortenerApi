using Microsoft.EntityFrameworkCore;
using UrlShortenerApi.Models;

namespace UrlShortenerApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ShortenedUrl> ShortenedUrls => Set<ShortenedUrl>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShortenedUrl>(entity =>
        {
            entity.HasIndex(e => e.ShortCode)
                .IsUnique();

            entity.Property(e => e.OriginalUrl)
                .HasMaxLength(2048)
                .IsRequired();

            entity.Property(e => e.ShortCode)
                .HasMaxLength(7)
                .IsRequired();

            entity.Property(e => e.ClickCount)
                .HasDefaultValue(0L);
        });
    }
}
