using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CasesController : Controller
{

    [HttpPost("owed")]

    [HttpPost("owed/total")]
    [VerifySession]
    public IActionResult GetTotalOwedCases([FromForm] StandardAuthorizedForm form)
    {
        return Ok(CasesOwed.GetTotalOwedCases());
    }

    [HttpPost("given/total")]
    [VerifySession]
    public IActionResult GetTotalGivenCases([FromForm] StandardAuthorizedForm form)
    {
        return Ok(CasesGiven.GetTotalGivenCases());
    }

    [HttpPost("owed/self")]
    [VerifySession]
    public async Task<IActionResult> GetOwedCasesSelf([FromForm] StandardAuthorizedForm form)
    {
        var person = await Person.ReadObj(form.guid);
        float cases = await CasesOwed.GetTotalOwedCases(person!.id);
        return Ok(cases);
    }


    [HttpPost("owed/total/self")]
    [VerifySession]
    public async Task<IActionResult> GetTotalOwedCasesSelf([FromForm] StandardAuthorizedForm form)
    {
        var person = await Person.ReadObj(form.guid);
        float cases = await CasesOwed.GetTotalOwedCases(person!.id);
        cases += await CasesGiven.GetTotalGivenCases(person.id);
        return Ok(cases);
    }

    [HttpPost("given/total/self")]
    [VerifySession]
    public async Task<IActionResult> GetTotalGivenCasesSelf([FromForm] StandardAuthorizedForm form)
    {
        var person = await Person.ReadObj(form.guid);
        float cases = await CasesGiven.GetTotalGivenCases(person!.id);
        return Ok(cases);
    }

}