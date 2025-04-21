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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Account>().ToTable("Account"); // upewniamy się, że pasuje do istniejącej tabeli
    }
}
