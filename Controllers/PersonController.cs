using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PersonController : Controller
{

    public class MessageResponse
    {
        public string message { get; set; }
        public MessageResponse(string message)
        {
            this.message = message;
        }
    }

    // // 
    // // GET: /HelloWorld/
    // public string Index()
    // {
    //     return "This is my default action...";
    // }
    // 
    // GET: /Person/get/ 
    [HttpGet("get/{id?}")]
    public string get(string? id)
    {
        // It has an id parameter
        if (string.IsNullOrEmpty(id))
        {
            return "No id provided";
        }
        else
        {
            bool parsed = int.TryParse(id, out int parsedId);
            if (!parsed || parsedId <= 0)
            {
                return "Invalid id provided";
            }
            // Return json
            var person = DatabaseController.Instance.GetPerson(int.Parse(id));
            if (person == null)
            {
                return "No person found with id " + id;
            }
            else
            {
                return System.Text.Json.JsonSerializer.Serialize(person);
            }
        }
    }

    public class RegisterForm
    {
        public ulong Discord_Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Invite_Code { get; set; }
    }

    [HttpPost("register")]
    public IActionResult Register([FromForm] RegisterForm form)
    {
        Console.WriteLine(form.Invite_Code);
        Console.WriteLine(Environment.GetEnvironmentVariable("INVITE_CODE"));
        // It has an id parameter
        if (string.IsNullOrEmpty(form.Invite_Code) || form.Invite_Code != Environment.GetEnvironmentVariable("INVITE_CODE"))
        {
            return BadRequest("Invalid invite code");
        }
        if (string.IsNullOrEmpty(form.Username) || string.IsNullOrEmpty(form.Password) || form.Discord_Id == 0)
        {
            return BadRequest("Missing parameters");
        }
        else
        {
            // Return json
            var result = DatabaseController.Instance.CreatePerson(form.Username, form.Discord_Id, form.Password);
            return result == null ? BadRequest("Failed to create person, username or discord_id may already exist") : Ok(JsonSerializer.Serialize(result));
        }
    }

    public class LoginForm
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // Returns 200 json
    [HttpPost("login")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(Session), 200, "application/json")]
    [ProducesResponseType(typeof(MessageResponse), 400, "application/json")]
    public IActionResult Login([FromForm] LoginForm form)
    {
        if (!String.IsNullOrEmpty(form.Username) && !String.IsNullOrEmpty(form.Password))
        {
            Person? person = DatabaseController.Instance.GetPerson(form.Username);
            if (person != null)
            {
                bool password_match = BCrypt.Net.BCrypt.Verify(form.Password, DatabaseController.Instance.GetPasswordHash(person));

                if (password_match)
                {
                    // Create session and send session token back with expiry date
                    Session? session = DatabaseController.Instance.CreateSession(person);
                    if (session == null) return StatusCode(500);
                    return Ok(JsonSerializer.Serialize(session));
                }
            }
            return BadRequest(new MessageResponse("Username or Password is wrong."));
        }
        return BadRequest(new MessageResponse("Username or Password isn't provided."));
    }
}