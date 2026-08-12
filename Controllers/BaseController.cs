using AutoMapper;
using Fredagsbar.Backend.Database;
using Microsoft.AspNetCore.Mvc;

namespace Fredagsbar.Backend.Controllers;

public class BaseController(IMapper mapper, ApplicationDbContext dbContext) : ControllerBase
{
	protected readonly IMapper _mapper = mapper;
	protected readonly ApplicationDbContext _dbContext = dbContext;

	public class ContentTypes
	{
		public const string JSON = "application/json";
	}
}
