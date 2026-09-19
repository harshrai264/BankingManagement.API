
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using BankingManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace BankingManagement.Data
{
   
        public class AppDbContext : DbContext
        {
            public AppDbContext(DbContextOptions<AppDbContext> options)
                : base(options)
            {

            }

            public DbSet<Customer> Customers { get; set; }  // here the table nane is Customers and the model class name is Customer
            public DbSet<Account> Accounts { get; set; }  // code vs database is written 
            public DbSet<AuditLog> Auditlogs { get; set; }
            public DbSet<AllUser> AllUsers { get; set; } 


        // below is used becuz when creating migration "No store type was specified for the decimal property 'Balance' on entity type 'Account'.
        // This will cause values to be silently truncated if they do not fit in the default precision and scale. Explicitly specify the SQL server
        // column type that can accommodate all the values in 'OnModelCreating' using 'HasColumnType', specify precision and scale using 'HasPrecision', or configure a value converter using 'HasConversion'. To undo this action, use Remove-Migration."
        protected override void OnModelCreating(ModelBuilder modelBuilder)

        {
            modelBuilder.Entity<Account>()
                .Property(a => a.Balance)
                .HasPrecision(18, 2);

            // to set up one-to-one relationship between Customer and Account entities
            modelBuilder.Entity<Account>()
           .HasOne(a => a.Customer)
           .WithOne(c => c.Account)
           .HasForeignKey<Account>(a => a.CustomerId);
        }


        //18 → maximum total digits

        //2 → digits after the decimal point

        //protected override void OnModelCreated (ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<Account>()
        //    .HasOne(a => a.Customer)
        //    .WithOne(c => c.Account)
        //    .HasForeignKey<Account>(a => a.CustomerId);
        //}


        // EntityEntry this in build method by EF which keep the records of the operation performed by EF core.
        private AuditLog CreateAuditLog(EntityEntry entry)
        {
            var auditLog = new AuditLog
            {
                Module = entry.Entity.GetType().Name,
                Action = entry.State.ToString(),
                CreatedDate = DateTime.UtcNow
            };

            // Get Record ID
            var primaryKey = entry.Metadata.FindPrimaryKey();

            if (primaryKey != null)
            {
                var keyProperty = primaryKey.Properties.First();

                var keyValue = entry.Property(keyProperty.Name).CurrentValue;

                if (keyValue != null)
                {
                    auditLog.RecordId = Convert.ToInt32(keyValue);
                }
            }

            // Modified
            if (entry.State == EntityState.Modified)
            {
                var oldValues = new Dictionary<string, object?>();
                var newValues = new Dictionary<string, object?>();

                foreach (var property in entry.Properties)
                {
                    if (property.IsModified)
                    {
                        oldValues[property.Metadata.Name] = property.OriginalValue;
                        newValues[property.Metadata.Name] = property.CurrentValue;
                    }
                }

                auditLog.OldValue = JsonSerializer.Serialize(oldValues);
                auditLog.NewValue = JsonSerializer.Serialize(newValues);
            }

            // Added
            else if (entry.State == EntityState.Added)
            {
                var newValues = new Dictionary<string, object?>();

                foreach (var property in entry.Properties)
                {
                    newValues[property.Metadata.Name] = property.CurrentValue;
                }

                auditLog.NewValue = JsonSerializer.Serialize(newValues);
            }

            // Deleted
            else if (entry.State == EntityState.Deleted)
            {
                var oldValues = new Dictionary<string, object?>();

                foreach (var property in entry.Properties)
                {
                    oldValues[property.Metadata.Name] = property.OriginalValue;
                }

                auditLog.OldValue = JsonSerializer.Serialize(oldValues);
            }

            return auditLog;
        }

        public override async Task<int> SaveChangesAsync(
    CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e =>
                    e.Entity is not AuditLog &&
                    (e.State == EntityState.Added ||
                     e.State == EntityState.Modified ||
                     e.State == EntityState.Deleted))
                .ToList();

            foreach (var entry in entries)
            {
                var auditLog = CreateAuditLog(entry);

                Auditlogs.Add(auditLog);
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }

    }

