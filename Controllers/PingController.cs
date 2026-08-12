using AutoMapper;
using Fredagsbar.Backend.Database;
using Microsoft.AspNetCore.Mvc;

namespace Fredagsbar.Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class PingController(IMapper mapper, ApplicationDbContext dbContext) : BaseController(mapper, dbContext)
{

    [HttpGet]
	public async Task<IActionResult> Get()
	{
		return Ok("Pong");
	}

	[HttpGet("bot")]
	[InternalOnlyFilter]
	public async Task<IActionResult> GetBot()
	{
		return Ok("Pong");
	}
}
