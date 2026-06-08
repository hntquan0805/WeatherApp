using Microsoft.EntityFrameworkCore;

namespace WeatherApp.Models;

public partial class WeatherContext : DbContext
{
    public WeatherContext()
    {
    }

    public WeatherContext(DbContextOptions<WeatherContext> options)
        : base(options)
    {
    }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Weatherlog> Weatherlogs { get; set; }

    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Role).HasDefaultValueSql("'user'");
        });

        modelBuilder.Entity<Weatherlog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.FeelsLike).HasComment("Đơn vị: °C");
            entity.Property(e => e.Humidity).HasComment("Đơn vị: %  (0 - 100)");
            entity.Property(e => e.SearchedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Temperature).HasComment("Đơn vị: °C");
            entity.Property(e => e.WindSpeed).HasComment("Đơn vị: m/s");

            entity.HasOne(d => d.User).WithMany(p => p.Weatherlogs)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_WeatherLog_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
