using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CasesController : Controller
{

    [HttpPost("owed")]

    [HttpPost("owed/total")]
    public IActionResult GetTotalOwedCases([FromForm] StandardAuthorizedForm form)
    {
        if (DatabaseController.Instance.VerifySession(form.guid))
        {
            float cases = DatabaseController.Instance.GetTotalOwedCases();
            return Ok(cases);
        }
        return BadRequest();
    }

    [HttpPost("given/total")]
    public IActionResult GetTotalGivenCases([FromForm] StandardAuthorizedForm form)
    {
        return Ok();
    }

    [HttpPost("owed/self")]
    public IActionResult GetOwedCasesSelf([FromForm] StandardAuthorizedForm form)
    {
        if (DatabaseController.Instance.VerifySession(form.guid))
        {
            float cases = DatabaseController.Instance.GetOwedCases(form.guid);
            return Ok(cases);
        }
        return BadRequest();
    }


    [HttpPost("owed/total/self")]
    public IActionResult GetTotalOwedCasesSelf([FromForm] StandardAuthorizedForm form)
    {
        if (DatabaseController.Instance.VerifySession(form.guid))
        {
            float cases = DatabaseController.Instance.GetOwedCases(form.guid);
            cases += DatabaseController.Instance.GetGivenCases(form.guid);
            return Ok(cases);
        }
        return BadRequest();
    }

    [HttpPost("given/total/self")]
    public IActionResult GetTotalGivenCasesSelf([FromForm] StandardAuthorizedForm form)
    {
        if (DatabaseController.Instance.VerifySession(form.guid))
        {
            float cases = DatabaseController.Instance.GetGivenCases(form.guid);
            return Ok(cases);
        }
        return BadRequest();
    }

}