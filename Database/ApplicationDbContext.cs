using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace fredagsbar_backend.Database;

using Models;


public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
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
        var entities = ChangeTracker.Entries()
            .Where(x => x.Entity is BaseEntity && (x.State == EntityState.Added || x.State == EntityState.Modified));

        foreach (var entity in entities)
        {
            var now = DateTime.UtcNow; // current datetime

            if (entity.State == EntityState.Added)
            {
                ((BaseEntity)entity.Entity).CreatedAt = now;
            }
            ((BaseEntity)entity.Entity).UpdatedAt = now;
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
