using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Simple_Account_Service.Features.Accounts.Entities;
using Simple_Account_Service.Features.Transactions.Entities;

namespace Simple_Account_Service.Infrastructure.Data
{
    public class SasDbContext(DbContextOptions<SasDbContext> options) : DbContext(options)
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSnakeCaseNamingConvention();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.Property(a => a.Balance)
                    .HasColumnType("numeric(18,2)")
                    .IsRequired();

                entity.Property(a => a.InterestRate)
                    .HasColumnType("numeric(5,4)");

                entity.Property(a => a.Currency)
                    .HasMaxLength(3);

                entity.HasMany(a => a.Transactions)
                    .WithOne(t => t.Account)
                    .HasForeignKey(t => t.AccountId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                entity.Property(a => a.Version)
                    .IsRowVersion();

                entity.Property(a => a.CreatedAt)
                    .HasColumnType("timestamptz")
                    .IsRequired();

                entity.Property(a => a.ClosedAt)
                    .HasColumnType("timestamptz");

                entity.HasIndex(a => a.OwnerId)
                    .HasMethod("hash");
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.Property(t => t.Amount)
                    .HasColumnType("numeric(18,2)")
                    .IsRequired();

                entity.Property(t => t.Currency)
                    .HasMaxLength(3);

                entity.Property(t => t.Description)
                    .HasMaxLength(500);

                entity.Property(t => t.Timestamp)
                    .HasColumnType("timestamptz")
                    .IsRequired();

                entity.Property(t => t.Version)
                    .IsRowVersion();

                entity.HasIndex(t => new { t.AccountId, t.Timestamp });

                entity.HasIndex(t => t.Timestamp)
                    .HasMethod("gist");
            });

            // Это ValueConverter для всех DateTime свойств
            var dateTimeConverter = new DateTimeUtcConverter();

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(dateTimeConverter);
                    }

                    if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(new NullableDateTimeUtcConverter());
                    }
                }
            }

            base.OnModelCreating(modelBuilder);
        }
    }

    public class DateTimeUtcConverter() : ValueConverter<DateTime, DateTime>(
        d => d.Kind == DateTimeKind.Utc ? d : d.ToUniversalTime(),
        d => DateTime.SpecifyKind(d, DateTimeKind.Utc).ToLocalTime());

    public class NullableDateTimeUtcConverter() : ValueConverter<DateTime?, DateTime?>(d => d.HasValue
            ? (d.Value.Kind == DateTimeKind.Utc ? d.Value : d.Value.ToUniversalTime())
            : d,
        d => d.HasValue
            ? DateTime.SpecifyKind(d.Value, DateTimeKind.Utc).ToLocalTime()
            : d);
}
