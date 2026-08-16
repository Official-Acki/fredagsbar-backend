using Fredagsbar.Backend.Database.Models;

namespace Fredagsbar.Backend.Database.Constants;

public class Rules
{
	public static readonly Rule StartingFee = new()
	{
		ID = 1,
		Name = "Starting Fee",
		Description = "Everyone has to owe something. Right?",
		BreachFee = 1f
	};
	public static readonly Rule BeerWaste = new()
	{
		ID = 2,
		Name = "Ølspild",
		Description = "Ølspild!! Du spildte din øl, hold bedre fast næste gang.",
		BreachFee = 0.5f
	};
	public static readonly Rule BelowMark = new()
	{
		ID = 3,
		Name = "Under mærket",
		Description = "Du drak ikke under mærket. Drik mere til næste gang, tag en ordentlig slurk.",
		BreachFee = 1f
	};

	public static IEnumerable<Rule> All =>
	typeof(Rules)
		.GetFields()
		.Where(f => f.FieldType == typeof(Rule))
		.Select(f => (Rule)f.GetValue(null)!);
}
