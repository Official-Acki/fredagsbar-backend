
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class SessionController : Controller
{
    [HttpPost("verify")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(MessageResponse), 200, "application/json")]
    [ProducesResponseType(typeof(MessageResponse), 400, "application/json")]
    [VerifySession]
    public IActionResult Verify([FromForm] StandardAuthorizedForm form)
    {
        return Ok(new MessageResponse("Token verified."));
    }

}