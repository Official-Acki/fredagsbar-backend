using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class VerifySession : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionArguments.TryGetValue("form", out var formObj) && formObj is StandardAuthorizedForm form)
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" && form.guid == Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"))
            {
                await next();
                return;
            }
            if (form.guid == Guid.Empty)
            {
                context.Result = new BadRequestObjectResult("Missing guid");
                return;
            }
            Session session = new Session(form.guid);
            if (!await session.VerifySession())
            {
                context.Result = new BadRequestObjectResult("Invalid guid");
                return;
            }
        }
        await next();
    }
}