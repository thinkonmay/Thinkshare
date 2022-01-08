using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace SharedHost.Auth.ThinkmayAuthProtocol
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class UserAttribute : ActionFilterAttribute
    {
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ManagerAttribute : ActionFilterAttribute
    {
    }


    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AdminAttribute : ActionFilterAttribute
    {
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ClusterAttribute : ActionFilterAttribute
    {
    }
}