using AutoMapper;
using fredagsbar_backend.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace fredagsbar_backend.Controllers;

[ApiController]
[Route("transactions")]
public class BeerCaseTransactionController(IMapper mapper, ApplicationDbContext dbContext) : BaseController(mapper, dbContext)
{
	// Get
	[HttpGet]
	public async Task<IActionResult> Get()
	{
		var transactions = await _dbContext.BeerCaseTransactions.ToListAsync();
		return Ok(transactions);
	}

	// Create

	// Update

	// Delete

	// Private methods

}
