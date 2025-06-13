using PersonalFinanceManager.WebApi.Models;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.WebApi.Models;

using System.Collections.Generic;
using System.Reflection.Emit;

namespace PersonalFinanceManager.WebApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<AppUser> AppUsers { get; set; }
    public DbSet<AccountPermission> AccountPermissions { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Transaction> Transactions { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Account>().ToTable("Account");
        modelBuilder.Entity<Account>().Property(e => e.Type).HasColumnType("smallint");

        modelBuilder.Entity<AppUser>().ToTable("AppUser");
        modelBuilder.Entity<AppUser>().HasIndex(u => u.Login).IsUnique();
        modelBuilder.Entity<AppUser>().HasIndex(u => u.Email).IsUnique();

        modelBuilder.Entity<AccountPermission>().ToTable("AccountPermission");
        modelBuilder.Entity<AccountPermission>()
            .HasKey(ap => new { ap.AccountId, ap.AppUserId });

        modelBuilder.Entity<AccountPermission>()
            .HasOne(ap => ap.Account)
            .WithMany(a => a.AccountPermissions)
            .HasForeignKey(ap => ap.AccountId);
        modelBuilder.Entity<AccountPermission>()
            .HasOne(ap => ap.AppUser)
            .WithMany(u => u.AccountPermissions)
            .HasForeignKey(ap => ap.AppUserId);

        modelBuilder.Entity<Account>()
            .HasOne(a => a.Owner)
            .WithMany(u => u.OwnedAccounts)
            .HasForeignKey(a => a.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Category>().ToTable("Category");
        modelBuilder.Entity<Category>()
                .HasOne(c => c.Owner)
                .WithMany(u => u.OwnedCategories)
                .HasForeignKey(c => c.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Transaction>().ToTable("Transaction");
        modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Account)
                .WithMany(a => a.Transactions)
                .HasForeignKey(t => t.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Category)
            .WithMany()
            .HasForeignKey(t => t.CategoryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Owner)
            .WithMany(u => u.OwnedTransactions)
            .HasForeignKey(t => t.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
