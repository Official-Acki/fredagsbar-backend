using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fredagsbar.Backend.Database.Models;

public class Accusation : BaseEntity
{
	public int ID { get; set; }

	public required ulong AccuserID { get; set; }
	public User Accuser { get; set; } = null!;

	public required ulong AccusedID { get; set; }
	public User Accused { get; set; } = null!;

	public ICollection<AccusationVote> Votes { get; set; } = null!;
}

public class AccusationConfiguration : IEntityTypeConfiguration<Accusation>
{
	public void Configure(EntityTypeBuilder<Accusation> builder)
	{
		builder.HasKey(e => e.ID);

		builder.HasOne(e => e.Accuser).WithMany(e => e.Accusations).HasForeignKey(e => e.AccuserID);
		builder.HasOne(e => e.Accused).WithMany(e => e.Accused).HasForeignKey(e => e.AccusedID);
	}
}