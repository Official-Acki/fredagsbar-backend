namespace fredagsbar_backend.Database.DTO;

public class UserDto
{
	public required ulong ID { get; set; }

	public required string Username { get; set; }
	public required string DisplayName { get; set; }

	public required float BeerCasesOwed { get; set; }
}
