using Microsoft.AspNetCore.Mvc.Filters;

namespace CoreAccess.Decorator;

[AttributeUsage(AttributeTargets.Method)]
public class CoreAccessAuthorizeDecorator() : Attribute, IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        context.HttpContext.Request.Headers.TryGetValue("Authorization", out var token);
        // Logic to execute before the action runs
        // Example: Check user authorization
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Logic to execute after the action runs
        // Example: Log action execution details
    }
}