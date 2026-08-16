using AutoMapper;
using AutoMapper.QueryableExtensions;
using Fredagsbar.Shared.DTO;
using Fredagsbar.Backend.Database;
using Fredagsbar.Backend.Database.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fredagsbar.Backend.Database.Constants;

namespace Fredagsbar.Backend.Controllers;

[ApiController]
[Route("users")]
public class UserController(IMapper mapper, ApplicationDbContext dbContext) : BaseController(mapper, dbContext)
{
	// Get
	[HttpGet]
	[ProducesResponseType<UserDto>(StatusCodes.Status200OK, ContentTypes.JSON)]
	[ProducesResponseType<IEnumerable<UserDto>>(StatusCodes.Status200OK, ContentTypes.JSON)]
	public async Task<IActionResult> Get([FromQuery] IEnumerable<ulong>? ids = null)
	{
		if (ids != null) return await GetByIds(ids);
		var users = await _dbContext.Users.ProjectTo<UserDto>(_mapper.ConfigurationProvider)
			.ToListAsync();
		return Ok(users);
	}

	[HttpGet("{id}")]
	[ProducesResponseType<UserDto>(StatusCodes.Status200OK, ContentTypes.JSON)]
	public async Task<IActionResult> GetById(ulong id)
	{
		var user = await _dbContext.Users.ProjectTo<UserDto>(_mapper.ConfigurationProvider)
			.Where(u => u.ID == id)
			.FirstOrDefaultAsync();
		return user != null ? Ok(user) : NotFound($"User with id: '{id}', doesn't exist."); // Not secure?
	}

	private async Task<IActionResult> GetByIds([FromQuery] IEnumerable<ulong> ids)
	{
		var idList = ids?.ToList() ?? [];
		if (idList.Count == 0) return Ok(Array.Empty<UserDto>());

		var users = await _dbContext.Users.ProjectTo<UserDto>(_mapper.ConfigurationProvider)
			.Where(u => idList.Contains(u.ID))
			.ToListAsync();
		return Ok(users);
	}

	[HttpGet("{id}/summary")]
	[ProducesResponseType<UserSummaryDto>(StatusCodes.Status200OK, ContentTypes.JSON)]
	public async Task<IActionResult> GetSummaryById(ulong id)
	{
		var summary = await _dbContext.Users
			.Where(u => u.ID == id)
			.Include(u => u.BeerCaseTransactions)
			.Select(u => new UserSummaryDto()
			{
				CasesDebt = -u.BeerCaseTransactions.Sum(bct => bct.Amount),
				CasesGiven = u.BeerCaseTransactions.Where(bct => bct.Amount > 0).Sum(bct => bct.Amount),
				CasesReceived = -u.BeerCaseTransactions.Where(bct => bct.Amount < 0).Sum(bct => bct.Amount)
			})
			.FirstOrDefaultAsync();
		return Ok(summary);
	}


	// Create

	[HttpPost]
	[ProducesResponseType<UserDto>(StatusCodes.Status201Created, "application/json")]
	[InternalOnlyFilter]
	public async Task<IActionResult> Create([FromBody] UserCreateDto dto)
	{
		var exists = await _dbContext.Users.AnyAsync(u => u.ID == dto.ID);
		if (exists) return Conflict("User with id already exists.");

		var user = await _dbContext.Users.AddAsync(_mapper.Map<User>(dto));
		var caseTransaction = new BeerCaseTransaction()
		{
			UserID = user.Entity.ID,
			Amount = -1,
			RuleID = Rules.StartingFee.ID,
		};
		await _dbContext.BeerCaseTransactions.AddAsync(caseTransaction);
		await _dbContext.SaveChangesAsync();
		return Ok(_mapper.Map<UserDto>(user.Entity));
	}

	// Update

	// Delete

	// Private methods

}
