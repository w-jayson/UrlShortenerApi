using Microsoft.EntityFrameworkCore;
using UrlShortenerApi.Domain.Entities;

namespace UrlShortenerApi.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ShortenedUrl> ShortenedUrls => Set<ShortenedUrl>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ShortenedUrl>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(e => e.Code)
                .IsUnique();

            entity.Property(e => e.OriginalUrl)
                .IsRequired()
                .HasMaxLength(2048);

            entity.Property(e => e.CreatedAtUtc)
                .IsRequired();
        });
    }
}
