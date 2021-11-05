using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace SharedHost.Auth.ThinkmayAuthProtocol
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class UserAttribute : ActionFilterAttribute
    {
        public void OnActionExecuting(AuthorizationFilterContext context)
        {
            string isUser = (string)context.HttpContext.Items["IsUser"];
            if (isUser == "true")
            {
                context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ManagerAttribute : ActionFilterAttribute
    {
        public void OnActionExecuting(AuthorizationFilterContext context)
        {
            string isManger = (string)context.HttpContext.Items["IsManager"];
            if (isManger == "true")
            {
                context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
            }
        }
    }


    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AdminAttribute : ActionFilterAttribute
    {
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string isAdmin = (string)filterContext.HttpContext.Items["IsAdmin"];
            if (isAdmin == "true")
            {
                filterContext.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
            }
        }
    }
}