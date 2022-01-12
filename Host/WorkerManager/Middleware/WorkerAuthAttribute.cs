using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace WorkerManager.Middleware
{
    [AttributeUsage(AttributeTargets.Method)]
    public class WorkerAttribute : ActionFilterAttribute
    {
    }
    [AttributeUsage(AttributeTargets.Method)]
    public class OwnerAttribute : ActionFilterAttribute
    {
    }
}
