namespace Fredagsbar.Backend.Database.Models;

public class Rule : BaseEntity
{
	public int ID { get; set; }

	public required string Name { get; set; }
	public required string Description { get; set; }
	public required float BreachFee { get; set; }

	public IEnumerable<BeerCaseTransaction> BeerCaseTransactions { get; set; } = null!;
}
