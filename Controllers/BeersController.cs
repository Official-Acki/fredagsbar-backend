
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

[ApiController]
[Route("v1/[controller]")]
public class BeersController : Controller
{
    private readonly IHubContext<LeaderboardHub> _hubContext;

    public BeersController(IHubContext<LeaderboardHub> hubContext)
    {
        _hubContext = hubContext;
    }

    [HttpGet()]
    public string Index()
    {
        return "Beers controller is up and running.";
    }

    [HttpPost("drank/total/")]
    [HttpPost("drank/total/{person_id?}")]
    [VerifySession]
    public async Task<IActionResult> getBeersDrankTotal([FromForm] StandardAuthorizedForm form, int? person_id)
    {
        if (person_id.HasValue)
        {
            if (person_id <= 0) return BadRequest("Invalid person_id provided");
            return Ok(await Person.GetTotalBeersByPerson(person_id.Value));
        }
        return Ok(await Person.GetTotalBeers());
    }

    [HttpPost("drank/total/today/")]
    [HttpPost("drank/total/today/{person_id?}")]
    [VerifySession]
    public async Task<IActionResult> getBeersDrankTotalToday([FromForm] StandardAuthorizedForm form, int? person_id)
    {
        if (person_id.HasValue)
        {
            if (person_id <= 0) return BadRequest("Invalid person_id provided");
            return Ok(await Person.GetTotalBeersTodayByPerson(person_id.Value));
        }
        return Ok(await Person.GetTotalBeersToday());
    }

    [HttpPost("drank/")]
    [VerifySession]
    public async Task<IActionResult> addBeerAsync([FromForm] StandardAuthorizedForm form)
    {
        var person = await Person.ReadObj(form.guid);
        if (person == null)
        {
            return BadRequest("Invalid guid");
        }
        var rowsAffected = await person.AddBeer();
        if (rowsAffected > 0)
        {

            // Notify all clients about the updated leaderboard
            var leaderboard = await Leaderboard.BeersDrankLeaderboard(DateTime.UtcNow);
            Console.WriteLine(JsonSerializer.Serialize(leaderboard));
            await _hubContext.Clients.All.SendAsync("ReceiveLeaderboard", leaderboard);

            return Ok("Beer added");
        }
        else
        {
            return StatusCode(500, "Failed to add beer");
        }
    }

    [HttpPost("drank/total/self/")]
    [VerifySession]
    public async Task<IActionResult> getBeersDrankTotalSelf([FromForm] StandardAuthorizedForm form)
    {
        var person = await Person.ReadObj(form.guid);
        if (person == null)
        {
            return BadRequest("Invalid guid");
        }
        var total = await Person.GetTotalBeersByPerson(person.id);
        return Ok(total.ToString());
    }

    [HttpPost("drank/total/today/self/")]
    [VerifySession]
    public async Task<IActionResult> getBeersDrankTotalTodaySelf([FromForm] StandardAuthorizedForm form)
    {
        var person = await Person.ReadObj(form.guid);
        if (person == null)
        {
            return BadRequest("Invalid guid");
        }
        var total = await Person.GetTotalBeersTodayByPerson(person.id);
        return Ok(total.ToString());
    }

}