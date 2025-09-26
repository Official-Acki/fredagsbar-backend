using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

[ApiController]
[Route("v1/[controller]")]
public class LeaderboardController : Controller
{
    // GET: /Leaderboard/get/
    [HttpPost("get/")]
    [HttpPost("get/{amount}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(Leaderboard), 200, "application/json")]
    [ProducesResponseType(typeof(MessageResponse), 400, "application/json")]
    [VerifySession]
    public async Task<IActionResult> get([FromForm] StandardAuthorizedForm form, int amount = 0)
    {
        if (amount <= 0)
        {
            return Ok(JsonSerializer.Serialize(await Leaderboard.BeersDrankLeaderboard()));
        }
        return Ok(JsonSerializer.Serialize(await Leaderboard.BeersDrankLeaderboard(amount)));
    }

    // GET: /Leaderboard/get/
    [HttpPost("get/today/")]
    [HttpPost("get/today/{amount}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(Leaderboard), 200, "application/json")]
    [ProducesResponseType(typeof(MessageResponse), 400, "application/json")]
    [VerifySession]
    public async Task<IActionResult> getToday([FromForm] StandardAuthorizedForm form, int amount = 0)
    {
        if (amount <= 0)
        {
            return Ok(JsonSerializer.Serialize(await Leaderboard.BeersDrankLeaderboard(DateTime.UtcNow)));
        }
        return Ok(JsonSerializer.Serialize(await Leaderboard.BeersDrankLeaderboard(DateTime.UtcNow, amount)));
    }
}

public class LeaderboardHub : Hub
{

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine("LeaderboardHub: OnConnectedAsync called");
        var httpContext = Context.GetHttpContext();
        if (httpContext == null)
        {
            Console.WriteLine("No HTTP context available. Connection aborted.");
            Context.Abort();
            return;
        }
        var sessionToken = httpContext.Request.Query["session_token"].ToString();
        Console.WriteLine($"Session token: {sessionToken}");
        if (!Guid.TryParse(sessionToken, out var sessionGuid) || !await new Session(sessionGuid).VerifySession())
        {
            Console.WriteLine("Invalid session token. Connection aborted.");
            // Reject the connection
            Context.Abort();
            return;
        }
        Console.WriteLine("Valid session token. Connection accepted.");
        await base.OnConnectedAsync();
    }

    public async Task SendLeaderboard(Leaderboard leaderboard)
    {
        await Clients.All.SendAsync("ReceiveLeaderboard", leaderboard);
    }
}