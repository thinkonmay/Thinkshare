using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace WorkerManager.Middleware
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class WorkerAttribute : ActionFilterAttribute
    {
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class OwnerAttribute : ActionFilterAttribute
    {
    }
}
