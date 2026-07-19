using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class InternalOnlyFilter : IAsyncResourceFilter
{
	private readonly string _expectedKey;
	private readonly ILogger<InternalOnlyFilter> _logger;

	public InternalOnlyFilter(IConfiguration config, ILogger<InternalOnlyFilter> logger)
	{
		_expectedKey = config["Internal:ApiKey"]
			?? throw new InvalidOperationException("Internal:ApiKey not configured");
		_logger = logger;
	}

	public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
	{
		var httpContext = context.HttpContext;
		var providedKey = httpContext.Request.Headers["X-Internal-Key"].ToString();

		if (!string.IsNullOrEmpty(providedKey) &&
			CryptographicOperations.FixedTimeEquals(
				Encoding.UTF8.GetBytes(providedKey),
				Encoding.UTF8.GetBytes(_expectedKey)))
		{
			await next();
			return;
		}
		else
		{
			_logger.LogWarning("Rejected internal-only request from {Ip}",
				httpContext.Connection.RemoteIpAddress);
		}

		context.Result = new UnauthorizedResult();
	}
}


public class InternalOnlyFilterAttribute : ServiceFilterAttribute
{
	public InternalOnlyFilterAttribute() : base(typeof(InternalOnlyFilter))
	{
	}
}
