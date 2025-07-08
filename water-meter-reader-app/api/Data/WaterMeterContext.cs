using Microsoft.EntityFrameworkCore;
using Api.Models;

namespace Api.Data
{
    public class WaterMeterContext : DbContext
    {
        public DbSet<WaterReading> WaterReadings { get; set; }
        public DbSet<RateConfig> RateConfigs { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data Source=C:\\Users\\ahees\\Dev\\WaterMeterReadingApp\\water-meter-reader-app\\database\\water_meter.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WaterReading>()
                .HasKey(wr => wr.Id);

            modelBuilder.Entity<WaterReading>()
                .HasOne(wr => wr.User)
                .WithMany(u => u.WaterReadings)
                .HasForeignKey(wr => wr.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RateConfig>()
                .HasKey(rc => rc.Id);

            modelBuilder.Entity<User>()
                .HasKey(u => u.UserId);
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
        }
    }
}