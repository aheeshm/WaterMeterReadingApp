using Microsoft.EntityFrameworkCore;
using Api.Models;
using Api.Models.Tariffs;

namespace Api.Data
{
    public class WaterMeterContext : DbContext
    {
        public DbSet<WaterReading> WaterReadings { get; set; }
        public DbSet<RateConfig> RateConfigs { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<TariffSchedule> TariffSchedules { get; set; }
        public DbSet<TariffCategory> TariffCategories { get; set; }
        public DbSet<TariffBand> TariffBands { get; set; }
        public DbSet<PropertyAccount> PropertyAccounts { get; set; }
        public DbSet<BillingCalculation> BillingCalculations { get; set; }
        public DbSet<CalculationLineItem> CalculationLineItems { get; set; }

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

            modelBuilder.Entity<TariffSchedule>()
                .HasKey(schedule => schedule.TariffScheduleId);
            modelBuilder.Entity<TariffSchedule>()
                .Property(schedule => schedule.VatRate)
                .HasColumnType("decimal(12,4)");
            modelBuilder.Entity<TariffSchedule>()
                .HasIndex(schedule => new { schedule.Municipality, schedule.EffectiveFrom, schedule.EffectiveTo, schedule.IsActive });

            modelBuilder.Entity<TariffCategory>()
                .HasKey(category => category.TariffCategoryId);
            modelBuilder.Entity<TariffCategory>()
                .HasIndex(category => category.Code)
                .IsUnique();

            modelBuilder.Entity<TariffBand>()
                .HasKey(band => band.TariffBandId);
            modelBuilder.Entity<TariffBand>()
                .Property(band => band.ServiceType)
                .HasConversion<string>();
            modelBuilder.Entity<TariffBand>()
                .Property(band => band.LowerBoundKl)
                .HasColumnType("decimal(12,4)");
            modelBuilder.Entity<TariffBand>()
                .Property(band => band.UpperBoundKl)
                .HasColumnType("decimal(12,4)");
            modelBuilder.Entity<TariffBand>()
                .Property(band => band.RateExcludingVat)
                .HasColumnType("decimal(12,4)");
            modelBuilder.Entity<TariffBand>()
                .Property(band => band.RateIncludingVat)
                .HasColumnType("decimal(12,4)");
            modelBuilder.Entity<TariffBand>()
                .Property(band => band.PropertyValueMaximum)
                .HasColumnType("decimal(12,4)");
            modelBuilder.Entity<TariffBand>()
                .Property(band => band.PropertyValueMinimumExclusive)
                .HasColumnType("decimal(12,4)");
            modelBuilder.Entity<TariffBand>()
                .Property(band => band.DischargePercentage)
                .HasColumnType("decimal(12,4)");
            modelBuilder.Entity<TariffBand>()
                .HasIndex(band => new
                {
                    band.TariffScheduleId,
                    band.TariffCategoryId,
                    band.ServiceType,
                    band.LowerBoundKl,
                    band.UpperBoundKl,
                    band.PropertyValueMaximum,
                    band.PropertyValueMinimumExclusive,
                    band.SortOrder
                })
                .IsUnique();
            modelBuilder.Entity<TariffBand>()
                .HasOne(band => band.TariffSchedule)
                .WithMany(schedule => schedule.TariffBands)
                .HasForeignKey(band => band.TariffScheduleId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<TariffBand>()
                .HasOne(band => band.TariffCategory)
                .WithMany(category => category.TariffBands)
                .HasForeignKey(band => band.TariffCategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PropertyAccount>()
                .HasKey(account => account.PropertyAccountId);
            modelBuilder.Entity<PropertyAccount>()
                .Property(account => account.AccountBillingType)
                .HasConversion<string>();
            modelBuilder.Entity<PropertyAccount>()
                .Property(account => account.SupplyType)
                .HasConversion<string>();
            modelBuilder.Entity<PropertyAccount>()
                .Property(account => account.DevelopmentType)
                .HasConversion<string>();
            modelBuilder.Entity<PropertyAccount>()
                .Property(account => account.PropertyRateableValue)
                .HasColumnType("decimal(12,4)");
            modelBuilder.Entity<PropertyAccount>()
                .Property(account => account.AgreedSewerDischargePercentage)
                .HasColumnType("decimal(12,4)");
            modelBuilder.Entity<PropertyAccount>()
                .HasOne(account => account.User)
                .WithMany()
                .HasForeignKey(account => account.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PropertyAccount>()
                .HasIndex(account => account.MunicipalAccountNumber);

            modelBuilder.Entity<BillingCalculation>()
                .HasKey(calculation => calculation.BillingCalculationId);
            modelBuilder.Entity<BillingCalculation>()
                .Property(calculation => calculation.PreviousReadingKl)
                .HasColumnType("decimal(12,4)");
            modelBuilder.Entity<BillingCalculation>()
                .Property(calculation => calculation.CurrentReadingKl)
                .HasColumnType("decimal(12,4)");
            modelBuilder.Entity<BillingCalculation>()
                .Property(calculation => calculation.ConsumptionKl)
                .HasColumnType("decimal(12,4)");
            modelBuilder.Entity<BillingCalculation>()
                .Property(calculation => calculation.TotalIncludingVat)
                .HasColumnType("decimal(12,4)");
            modelBuilder.Entity<BillingCalculation>()
                .Property(calculation => calculation.TotalExcludingVat)
                .HasColumnType("decimal(12,4)");
            modelBuilder.Entity<BillingCalculation>()
                .HasOne(calculation => calculation.TariffSchedule)
                .WithMany(schedule => schedule.BillingCalculations)
                .HasForeignKey(calculation => calculation.TariffScheduleId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<BillingCalculation>()
                .HasOne(calculation => calculation.PropertyAccount)
                .WithMany(account => account.BillingCalculations)
                .HasForeignKey(calculation => calculation.PropertyAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CalculationLineItem>()
                .HasKey(lineItem => lineItem.CalculationLineItemId);
            modelBuilder.Entity<CalculationLineItem>()
                .Property(lineItem => lineItem.ServiceType)
                .HasConversion<string>();
            modelBuilder.Entity<CalculationLineItem>()
                .Property(lineItem => lineItem.BandLowerKl)
                .HasColumnType("decimal(12,4)");
            modelBuilder.Entity<CalculationLineItem>()
                .Property(lineItem => lineItem.BandUpperKl)
                .HasColumnType("decimal(12,4)");
            modelBuilder.Entity<CalculationLineItem>()
                .Property(lineItem => lineItem.VolumeAllocatedKl)
                .HasColumnType("decimal(12,4)");
            modelBuilder.Entity<CalculationLineItem>()
                .Property(lineItem => lineItem.DischargePercentage)
                .HasColumnType("decimal(12,4)");
            modelBuilder.Entity<CalculationLineItem>()
                .Property(lineItem => lineItem.BillableSewerVolumeKl)
                .HasColumnType("decimal(12,4)");
            modelBuilder.Entity<CalculationLineItem>()
                .Property(lineItem => lineItem.UnitRateIncludingVat)
                .HasColumnType("decimal(12,4)");
            modelBuilder.Entity<CalculationLineItem>()
                .Property(lineItem => lineItem.UnitRateExcludingVat)
                .HasColumnType("decimal(12,4)");
            modelBuilder.Entity<CalculationLineItem>()
                .Property(lineItem => lineItem.UnroundedLineAmountIncludingVat)
                .HasColumnType("decimal(12,4)");
            modelBuilder.Entity<CalculationLineItem>()
                .Property(lineItem => lineItem.UnroundedLineAmountExcludingVat)
                .HasColumnType("decimal(12,4)");
            modelBuilder.Entity<CalculationLineItem>()
                .Property(lineItem => lineItem.LineAmountIncludingVat)
                .HasColumnType("decimal(12,4)");
            modelBuilder.Entity<CalculationLineItem>()
                .Property(lineItem => lineItem.LineAmountExcludingVat)
                .HasColumnType("decimal(12,4)");
            modelBuilder.Entity<CalculationLineItem>()
                .HasOne(lineItem => lineItem.BillingCalculation)
                .WithMany(calculation => calculation.LineItems)
                .HasForeignKey(lineItem => lineItem.BillingCalculationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}