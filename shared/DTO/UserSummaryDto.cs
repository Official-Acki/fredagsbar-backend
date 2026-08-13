namespace Fredagsbar.Shared.DTO;

public class UserSummaryDto
{
	public float CasesDebt { get; set; }
	public float CasesGiven { get; set; }
	/// <summary>
	/// How many cases they have received. Penalties.
	/// </summary>
	public float CasesReceived { get; set; }
}
