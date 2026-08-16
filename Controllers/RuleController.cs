using AutoMapper;
using Fredagsbar.Backend.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fredagsbar.Backend.Controllers;

[ApiController]
[Route("Rules")]
public class RuleController(IMapper mapper, ApplicationDbContext dbContext) : BaseController(mapper, dbContext)
{
	// Get
	[HttpGet]
	public async Task<IActionResult> Get()
	{
		var rules = await _dbContext.Rules.ToListAsync();
		return Ok(rules);
	}

	// Create

	// Update

	// Delete

	// Private methods

}
