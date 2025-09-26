using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("v1/[controller]")]
public class EventsController : Controller
{

    [HttpPost()]
    [VerifySession]
    public async Task<IActionResult> Index([FromForm] StandardAuthorizedForm form)
    {
        return Ok(JsonSerializer.Serialize(await EventObject.GetAll()));
    }

    public class CreateEventForm : StandardAuthorizedForm
    {
        public string name { get; set; }
        public string? description { get; set; }
        public DateTime? start_time { get; set; }
        public DateTime? end_time { get; set; }
        public int? repeat_interval { get; set; }
    }

    [HttpPost("create/")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(MessageResponse), 200, "application/json")]
    [ProducesResponseType(typeof(MessageResponse), 400, "application/json")]
    [VerifySession]
    public async Task<IActionResult> Create([FromForm] CreateEventForm form)
    {
        Console.WriteLine(JsonSerializer.Serialize(form));
        if (string.IsNullOrEmpty(form.name) || form.start_time == null || form.end_time == null)
        {
            return BadRequest(new MessageResponse("Missing parameters"));
        }
        else
        {
            EventObject.EventTimes eventTime = new EventObject.EventTimes(form.start_time.Value, form.end_time.Value, new TimeSpan(form.repeat_interval ?? 0, 0, 0, 0));
            var newEvent = new EventObject(null, form.name, form.description, [eventTime]);
            newEvent.CreateObj();
            return Ok(new MessageResponse("Event created"));
        }
    }

    [HttpPost("current/")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(MessageResponse), 200, "application/json")]
    [ProducesResponseType(typeof(MessageResponse), 400, "application/json")]
    [VerifySession]
    public async Task<IActionResult> Current([FromForm] StandardAuthorizedForm form)
    {
        var currentEvents = await EventObject.GetCurrentEvents();
        return Ok(JsonSerializer.Serialize(currentEvents));
    }

}