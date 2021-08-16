using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Signalling.Filters
{
    public class ClientIpFilter : ActionFilterAttribute
    {
        private readonly HashSet<IPAddress> _safeList;

        public ClientIpFilter(string ipList)
        {
            foreach (var ip in ipList.Split(';'))
            {
                _safeList.Add(IPAddress.Parse(ip));
            }
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var remoteIp = context.HttpContext.Connection.RemoteIpAddress;

            if (remoteIp.IsIPv4MappedToIPv6)
            {
                remoteIp = remoteIp.MapToIPv4();
            }

            var badIp = _safeList.Contains(remoteIp);

            if (badIp)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
