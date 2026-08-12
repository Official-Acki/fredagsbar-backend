using System.ComponentModel.DataAnnotations.Schema;

namespace Fredagsbar.Backend.Database.Models;

public class User : BaseEntity
{
	public required ulong ID { get; set; }

	public required string Username { get; set; }
	public required string DisplayName { get; set; }

	public ICollection<BeerCaseTransaction> BeerCaseTransactions { get; set; } = null!;
}
