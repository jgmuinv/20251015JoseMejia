using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Web.Filters;

public class RequireLoginAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var http = context.HttpContext;
        var isLogged = http.Session.GetString("auth_user") != null;
        if (!isLogged)
        {
            context.Result = new RedirectToActionResult("Index", "Ingresar", new { returnUrl = http.Request.Path + http.Request.QueryString });
        }
    }
}