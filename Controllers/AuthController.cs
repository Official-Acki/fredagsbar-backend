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
        public string Invite_Code { get; set; } = string.Empty;
    }

    [HttpPost("register")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(Person), 200, "application/json")]
    [ProducesResponseType(typeof(MessageResponse), 400, "application/json")]
    public async Task<IActionResult> Register([FromForm] RegisterForm form)
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
            var result = await Person.CreateObj(form.Username, form.Password);
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
    public async Task<IActionResult> Login([FromForm] LoginForm form)
    {
        if (!String.IsNullOrEmpty(form.Username) && !String.IsNullOrEmpty(form.Password))
        {
            Person? person = await Person.ReadObj(form.Username);
            if (person != null)
            {
                bool password_match = BCrypt.Net.BCrypt.Verify(form.Password, await Person.GetPasswordHash(person.id));

                if (password_match)
                {
                    // Create session and send session token back with expiry date
                    Session? session = await Session.CreateObj(person);
                    if (session == null) return StatusCode(500);
                    return Ok(JsonSerializer.Serialize(session));
                }
            }
            return BadRequest(new MessageResponse("Username or Password is wrong."));
        }
        else if (form.Session_Token != null)
        {
            Session? session = new Session(form.Session_Token.Value);
            bool valid = await session.VerifySession();
            if (valid)
            {
                session = await session.Renew();
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
    public async Task<IActionResult> Logout([FromForm] LogoutForm form)
    {
        Console.Write("Logout request! " + form.guid + " : " + form.Person_Id);
        if (form.guid.HasValue)
        {
            if (form.Person_Id.HasValue && form.Person_Id != 0)
            {
                Session? session = new Session(form.guid.Value);
                if (await session.VerifySession(form.Person_Id.Value))
                {
                    Console.Write(" Session verified with person id");
                    await session.Delete();
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
    public async Task<IActionResult> WhoAmI([FromForm] StandardAuthorizedForm form)
    {
        Person? person = await Person.ReadObj(form.guid);
        if (person != null)
        {
            return Ok(JsonSerializer.Serialize(person));
        }
        return BadRequest(new MessageResponse("Invalid session."));
    }
}