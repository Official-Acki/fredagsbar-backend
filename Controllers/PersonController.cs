using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PersonController : Controller
{

    // // 
    // // GET: /HelloWorld/
    // public string Index()
    // {
    //     return "This is my default action...";
    // }
    // 
    // GET: /Person/get/ 
    [HttpPost("get/{id?}")]
    [Produces("application/json")]
    [VerifySession]
    public async Task<IActionResult> Get([FromForm] StandardAuthorizedForm form, int? id)
    {
        // It has an id parameter
        if (id == null)
        {
            return BadRequest("No id provided");
        }
        else
        {
            if (id <= 0)
            {
                return BadRequest("Invalid id provided");
            }
            // Return json
            var person = await Person.ReadObj(id.Value);
            if (person == null)
            {
                return NotFound("No person found with id " + id);
            }
            else
            {
                return Ok(System.Text.Json.JsonSerializer.Serialize(person));
            }
        }
    }
}