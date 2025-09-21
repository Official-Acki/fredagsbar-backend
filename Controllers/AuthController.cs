using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : Controller
{
    public class RegisterForm
    {
        // public ulong Discord_Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Invite_Code { get; set; }
    }

    [HttpPost("register")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(Person), 200, "application/json")]
    [ProducesResponseType(typeof(MessageResponse), 400, "application/json")]
    public IActionResult Register([FromForm] RegisterForm form)
    {
        Console.WriteLine(form.Invite_Code);
        Console.WriteLine(Environment.GetEnvironmentVariable("INVITE_CODE"));
        // It has an id parameter
        if (string.IsNullOrEmpty(form.Invite_Code) || form.Invite_Code != Environment.GetEnvironmentVariable("INVITE_CODE"))
        {
            return BadRequest(new MessageResponse("Invalid invite code"));
        }
        if (string.IsNullOrEmpty(form.Username) || string.IsNullOrEmpty(form.Password))
        {
            return BadRequest(new MessageResponse("Missing parameters"));
        }
        else
        {
            // Return json
            var result = DatabaseController.Instance.CreatePerson(form.Username, form.Password);
            if (result != null)
            {
                return Ok(JsonSerializer.Serialize(result));
            }
            else
            {
                return BadRequest(new MessageResponse("Failed to create person, username or discord_id may already exist"));
            }
        }
    }

    public class LoginForm
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public Guid? Session_Token { get; set; }
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
        else if (form.Session_Token != null)
        {
            bool valid = DatabaseController.Instance.VerifySession(form.Session_Token.Value);
            if (valid)
            {
                Session? session = DatabaseController.Instance.RenewSession(form.Session_Token.Value);

                return Ok(JsonSerializer.Serialize(session));
            }
            else
            {
                return BadRequest(new MessageResponse("Session is invalid or expired."));
            }
        }
        return BadRequest(new MessageResponse("Username or Password isn't provided."));
    }

    public class LogoutForm
    {
        public Guid? guid { get; set; }
        public int? Person_Id { get; set; }
    }

    [HttpPost("logout")]
    [Produces("application/json")]
    public IActionResult Logout([FromForm] LogoutForm form)
    {
        Console.Write("Logout request! " + form.guid + " : " + form.Person_Id);
        if (form.guid.HasValue)
        {
            if (form.Person_Id.HasValue && form.Person_Id != 0)
            {
                if (DatabaseController.Instance.VerifySession(form.guid.Value, form.Person_Id.Value))
                {
                    Console.Write(" Session verified with person id");
                    DatabaseController.Instance.DeleteSession(form.guid.Value);
                    // Verify the token
                    return Ok(new MessageResponse("Logged out. Old session deleted."));
                }
            }
            return BadRequest(new MessageResponse("Inconsistencies in request form."));
        }
        Console.WriteLine();
        return Ok(new MessageResponse("Logged out."));
    }

    [HttpPost("whoami")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(Person), 200, "application/json")]
    [ProducesResponseType(typeof(MessageResponse), 400, "application/json")]
    public IActionResult WhoAmI([FromForm] StandardAuthorizedForm form)
    {
        Person? person = DatabaseController.Instance.GetPerson(form.guid);
        if (person != null)
        {
            return Ok(JsonSerializer.Serialize(person));
        }
        return BadRequest(new MessageResponse("Invalid session."));
    }
}