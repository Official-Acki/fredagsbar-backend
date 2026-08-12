using AutoMapper;
using fredagsbar_backend.Database;
using Microsoft.AspNetCore.Mvc;

namespace fredagsbar_backend.Controllers;

public class BaseController(IMapper mapper, ApplicationDbContext dbContext) : ControllerBase
{
	protected readonly IMapper _mapper = mapper;
	protected readonly ApplicationDbContext _dbContext = dbContext;

	public class ContentTypes
	{
		public const string JSON = "application/json";
	}
}
