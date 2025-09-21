using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Generic;

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

    // GET: /Leaderboard/get/
    [HttpPost("get/today/")]
    [HttpPost("get/today/{amount}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(List<DatabaseController.LeaderboardEntry>), 200, "application/json")]
    [ProducesResponseType(typeof(MessageResponse), 400, "application/json")]
    public IActionResult getToday([FromForm] StandardAuthorizedForm form, int amount = 10)
    {
        if (DatabaseController.Instance.VerifySession(form.guid))
        {
            var leaderboard = DatabaseController.Instance.GetLeaderboardEntriesToday(amount);
            return Ok(JsonSerializer.Serialize(leaderboard));
        }
        return BadRequest(new MessageResponse("Invalid session."));
    }
}

public class LeaderboardHub : Hub
{

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine("LeaderboardHub: OnConnectedAsync called");
        var httpContext = Context.GetHttpContext();
        var sessionToken = httpContext.Request.Query["session_token"].ToString();
        Console.WriteLine($"Session token: {sessionToken}");

        if (!Guid.TryParse(sessionToken, out var sessionGuid) || !DatabaseController.Instance.VerifySession(sessionGuid))
        {
            Console.WriteLine("Invalid session token. Connection aborted.");
            // Reject the connection
            Context.Abort();
            return;
        }
        Console.WriteLine("Valid session token. Connection accepted.");
        await base.OnConnectedAsync();
    }

    public async Task SendLeaderboard(List<DatabaseController.LeaderboardEntry> leaderboard)
    {
        await Clients.All.SendAsync("ReceiveLeaderboard", leaderboard);
    }
}