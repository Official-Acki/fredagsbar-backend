namespace fredagsbar_backend.Database.Models;

public class BeerCaseTransaction : BaseEntity
{
	public uint ID { get; set; }

	public required ulong UserID { get; set; }
	public User User { get; set; } = null!;

	public DateTime Time { get; set; }
	/// <summary>Positive is giving, negative is receiving (penalty)</summary>
	public float Amount { get; set; }
}
