using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fredagsbar.Backend.Database.Models;

public class User : BaseEntity
{
	public required ulong ID { get; set; }

	public required string Username { get; set; }
	public required string DisplayName { get; set; }

	public ICollection<BeerCaseTransaction> BeerCaseTransactions { get; set; } = null!;
}

public class UserConfiguration : IEntityTypeConfiguration<User>
{
	public void Configure(EntityTypeBuilder<User> builder)
	{
		builder.HasIndex(e => e.Username)
			.IsUnique();
	}
}