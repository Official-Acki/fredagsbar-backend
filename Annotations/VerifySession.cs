using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class VerifySession : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionArguments.TryGetValue("form", out var formObj) && formObj is StandardAuthorizedForm form)
        {
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