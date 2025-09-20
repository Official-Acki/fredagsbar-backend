using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class LeaderboardController : Controller
{
    // GET: /Leaderboard/get/
    [HttpPost("get/")]
    [HttpPost("get/{amount}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(List<DatabaseController.LeaderboardEntry>), 200, "application/json")]
    [ProducesResponseType(typeof(MessageResponse), 400, "application/json")]
    public IActionResult get([FromForm] StandardAuthorizedForm form, int amount = 10)
    {
        if (DatabaseController.Instance.VerifySession(form.guid))
        {
            var leaderboard = DatabaseController.Instance.GetLeaderboardEntries(amount);
            return Ok(JsonSerializer.Serialize(leaderboard));
        }
        return BadRequest(new MessageResponse("Invalid session."));
    }
}