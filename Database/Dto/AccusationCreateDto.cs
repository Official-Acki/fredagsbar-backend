namespace Fredagsbar.Backend.Database.Dto;

public class AccusationCreateDto
{
    public required ulong AccuserId { get; set; }
    public required ulong AccusedId { get; set; }
    public required int RuleId { get; set; }
}