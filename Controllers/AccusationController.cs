using AutoMapper;
using AutoMapper.QueryableExtensions;
using Fredagsbar.Backend.Database;
using Fredagsbar.Backend.Database.Dto;
using Fredagsbar.Backend.Database.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fredagsbar.Backend.Controllers;

[ApiController]
[Route("[controller]s")]
public class AccusationController(IMapper mapper, ApplicationDbContext dbContext) : BaseController(mapper, dbContext)
{
    // Get
    [HttpGet]
    [ProducesResponseType<AccusationDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Accusations()
    {
        var accusations = await _dbContext.Accusations
            .ProjectTo<AccusationDto>(_mapper.ConfigurationProvider).ToListAsync();
        return Ok(accusations);
    }

    [HttpGet("ongoing")]
    [ProducesResponseType<AccusationDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> OngoingAccusations()
    {
        var ongoingAccusations = await _dbContext.Accusations
            .Where(accusation => !accusation.IsResolved).ProjectTo<AccusationDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
        return Ok(ongoingAccusations);
    }

    [HttpGet("old")]
    [ProducesResponseType<AccusationDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> OldAccusations()
    {
        var oldAccusations = await _dbContext.Accusations
            .Where(accusation => accusation.IsResolved).ProjectTo<AccusationDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
        return Ok(oldAccusations);
    }
    
    // Create
    [HttpPost]
    [ProducesResponseType<AccusationDto>(StatusCodes.Status201Created)]
    [InternalOnlyFilter]
    public async Task<IActionResult> Accuse([FromBody] AccusationCreateDto accusationCreateDto)
    {
        var accusation = _mapper.Map<Accusation>(accusationCreateDto);
        await _dbContext.Accusations.AddAsync(accusation);
        await _dbContext.SaveChangesAsync();
        return Created(nameof(Accusations),_mapper.Map<AccusationDto>(accusation));
    }

    // Update
    

    // Delete

    
    // Private methods

}