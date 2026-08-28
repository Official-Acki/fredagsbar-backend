using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fredagsbar.Backend.Database.Models;

public class BeerCaseTransaction : BaseEntity
{
	public uint ID { get; set; }

	public required ulong UserID { get; set; }
	public User User { get; set; } = null!;

	public DateTime Time { get; set; }
	/// <summary>Positive is giving, negative is receiving (penalty)</summary>
	public float Amount { get; set; }

	public int? RuleID { get; set; }
	public Rule Rule { get; } = null!;
}

public class BeerCaseTransactionConfiguration : IEntityTypeConfiguration<BeerCaseTransaction>
{
	public void Configure(EntityTypeBuilder<BeerCaseTransaction> builder)
	{
		builder.HasKey(e => e.ID);

		builder.HasIndex(e => e.UserID);
		builder.HasIndex(e => e.Time);
		builder.HasIndex(e => new { e.UserID, e.Time });

		builder.HasOne(e => e.User)
			.WithMany(e => e.BeerCaseTransactions)
			.HasForeignKey(e => e.UserID);

		builder.Property(e => e.Time)
			.HasDefaultValueSql("now()");

		builder.HasOne(e => e.Rule)
			.WithMany(e => e.BeerCaseTransactions)
			.HasForeignKey(e => e.RuleID);
	}
}