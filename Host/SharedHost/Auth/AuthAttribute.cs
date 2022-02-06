using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace SharedHost.Auth
{
    [AttributeUsage(AttributeTargets.Method)]
    public class UserAttribute : ActionFilterAttribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ManagerAttribute : ActionFilterAttribute
    {
    }


    [AttributeUsage(AttributeTargets.Method)]
    public class AdminAttribute : ActionFilterAttribute
    {
    }
    [AttributeUsage(AttributeTargets.Method)]
    public class ClusterAttribute : ActionFilterAttribute
    {
    }
}