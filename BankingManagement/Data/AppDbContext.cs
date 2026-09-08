

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
            public DbSet<Account> Accounts { get; set; }  // code vs datbase is written 


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

    }

    }

