using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fredagsbar.Backend.Database.Models;

public class AccusationVote : BaseEntity
{
	public int AccusationID { get; set; }
	public Accusation Accusation { get; set; } = null!;

	public required ulong VoterID { get; set; }
	public User Voter { get; set; } = null!;

	/// <summary>Yes is true, No is false. Like asking, do you think this is true</summary>
	public bool VotedFor { get; set; }
}

public class AccusationVoteConfiguration : IEntityTypeConfiguration<AccusationVote>
{
	public void Configure(EntityTypeBuilder<AccusationVote> builder)
	{
		builder.HasKey(e => new { e.AccusationID, e.VoterID });

		builder.HasOne(e => e.Accusation).WithMany(e => e.Votes).HasForeignKey(e => e.AccusationID);

		builder.HasOne(e => e.Voter).WithMany(e => e.Votes).HasForeignKey(e => e.VoterID);
	}
}