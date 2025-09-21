
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

[ApiController]
[Route("api/[controller]")]
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

    [HttpGet("drank/total/")]
    [HttpGet("drank/total/{person_id?}")]
    public string getBeersDrankTotal(int? person_id)
    {
        if (person_id.HasValue)
        {
            if (person_id <= 0) return "Invalid person_id provided";
            return DatabaseController.Instance.GetTotalBeersByPerson(person_id.Value).ToString();
        }
        return DatabaseController.Instance.GetTotalBeers().ToString();
    }

    [HttpGet("drank/total/today/")]
    [HttpGet("drank/total/today/{person_id?}")]
    public string getBeersDrankTotalToday(int? person_id)
    {
        if (person_id.HasValue)
        {
            if (person_id <= 0) return "Invalid person_id provided";
            return DatabaseController.Instance.GetTotalBeersTodayByPerson(person_id.Value).ToString();
        }
        return DatabaseController.Instance.GetTotalBeersToday().ToString();
    }

    [HttpPost("drank/")]
    public async Task<IActionResult> addBeerAsync([FromForm] StandardAuthorizedForm form)
    {
        if (form.guid == Guid.Empty)
        {
            return BadRequest("Missing guid");
        }
        var person = DatabaseController.Instance.GetPerson(form.guid);
        if (person == null)
        {
            return BadRequest("Invalid guid");
        }
        var success = DatabaseController.Instance.AddBeerToPerson(person.id);
        if (success)
        {
            
            // Notify all clients about the updated leaderboard
            var leaderboard = DatabaseController.Instance.GetLeaderboardEntriesToday();
            await _hubContext.Clients.All.SendAsync("ReceiveLeaderboard", leaderboard);

            return Ok("Beer added");
        }
        else
        {
            return StatusCode(500, "Failed to add beer");
        }
    }

    [HttpPost("drank/total/self/")]
    public IActionResult getBeersDrankTotalSelf([FromForm] StandardAuthorizedForm form)
    {
        if (form.guid == Guid.Empty)
        {
            return BadRequest("Missing guid");
        }
        var person = DatabaseController.Instance.GetPerson(form.guid);
        if (person == null)
        {
            return BadRequest("Invalid guid");
        }
        var total = DatabaseController.Instance.GetTotalBeersTodayByPerson(person.id);
        return Ok(total.ToString());
    }

    [HttpPost("drank/total/today/self/")]
    public IActionResult getBeersDrankTotalTodaySelf([FromForm] StandardAuthorizedForm form)
    {
        if (form.guid == Guid.Empty)
        {
            return BadRequest("Missing guid");
        }
        var person = DatabaseController.Instance.GetPerson(form.guid);
        if (person == null)
        {
            return BadRequest("Invalid guid");
        }
        var total = DatabaseController.Instance.GetTotalBeersTodayByPerson(person.id);
        return Ok(total.ToString());
    }

}