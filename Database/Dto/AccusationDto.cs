namespace Fredagsbar.Backend.Database.Dto;

public class AccusationDto
{
    public int ID { get; set; }
    public int RuleID { get; set; }
    public ulong AccuserID { get; set; }
    public ulong AccusedID { get; set; }
    public bool IsResolved { get; set; }
    public int YesVotes { get; set; }
    public int NoVotes { get; set; }
}