using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http.Features;
using SharedHost.Models.Cluster;
using System.Net;
using RestSharp;
using System;

namespace SharedHost.Logging
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ILog _log;

        public LoggingMiddleware(RequestDelegate next, 
                                 ILog log)
        {
            _next = next;
            _log = log;
        }

        public async Task Invoke(HttpContext context)
        {
            string ip = string.Empty;
            string Path = context.Request.Path;
            string Method = context.Request.Method;

            if (!string.IsNullOrEmpty(context.Request.Headers["X-Forwarded-For"]))
            {
                ip = context.Request.Headers["X-Forwarded-For"];
            }
            else
            {
                ip = context.Request.HttpContext.Features.Get<IHttpConnectionFeature>().RemoteIpAddress.ToString();
            }

            _log.Warning($"[REQUEST] [IP] {ip} [PATH] {Path} [METHOD] {Method}");


            try
            {
                await _next(context);
            }
            catch(Exception ex)
            {
                _log.Error("Handled by Logging Middleware",ex);
                context.Response.StatusCode =  StatusCodes.Status500InternalServerError;
                return;
            }
        }
    }
}