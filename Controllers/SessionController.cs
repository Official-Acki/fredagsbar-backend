
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class SessionController : Controller
{

    public class TokenForm
    {
        public Guid guid { get; set; }
    }
    [HttpPost("verify")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(MessageResponse), 200, "application/json")]
    [ProducesResponseType(typeof(MessageResponse), 400, "application/json")]
    public IActionResult Verify([FromForm] TokenForm form)
    {
        if (DatabaseController.Instance.VerifySession(form.guid))
        {
            return Ok(new MessageResponse("Token verified."));
        }
        return BadRequest("Token couldn't be verified.");
    }

}