using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace fredagsbar_backend.Database;

using Models;


public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
	public DbSet<BeerCaseTransaction> BeerCaseTransactions { get; set; }
	public DbSet<User> Users { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		// BeerCaseTransactions
		modelBuilder.Entity<BeerCaseTransaction>()
			.HasKey(e => e.ID);

		modelBuilder.Entity<BeerCaseTransaction>()
			.HasIndex(e => e.UserID);
		modelBuilder.Entity<BeerCaseTransaction>()
			.HasIndex(e => e.Time);
		modelBuilder.Entity<BeerCaseTransaction>()
			.HasIndex(e => new { e.UserID, e.Time });

		modelBuilder.Entity<BeerCaseTransaction>()
			.HasOne(e => e.User)
			.WithMany(e => e.BeerCaseTransactions)
			.HasForeignKey(e => e.UserID);

		modelBuilder.Entity<BeerCaseTransaction>()
			.Property(e => e.Time)
			.HasDefaultValueSql("now()");

		// User
		modelBuilder.Entity<User>()
			.HasIndex(e => e.Username)
			.IsUnique();
	}


	// Timestamps
	public override int SaveChanges()
	{
		AddTimestamps();
		return base.SaveChanges();
	}

	public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		AddTimestamps();
		return await base.SaveChangesAsync(cancellationToken);
	}

	private void AddTimestamps()
	{
		var entries = ChangeTracker.Entries()
			.Where(x => x.Entity is BaseEntity && (x.State == EntityState.Added || x.State == EntityState.Modified));

		foreach (var entry in entries)
		{
			var now = DateTime.Now; // current datetime

			if (entry.State == EntityState.Added)
			{
				((BaseEntity)entry.Entity).CreatedAt = now;
			}
			((BaseEntity)entry.Entity).UpdatedAt = now;

			// Special cases
			if (entry.Entity is BeerCaseTransaction transaction && transaction.Time == default)
				transaction.Time = DateTime.Now;
		}
	}

}

public static class MigrationService
{
	public static async Task MigrateAsync(this IServiceProvider services, IWebHostEnvironment env)
	{
		using var scope = services.CreateScope();
		var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		await ctx.Database.MigrateAsync();
		await ctx.SaveChangesAsync();
	}
}
