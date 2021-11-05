using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;

namespace SharedHost.Auth
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly RestClient _TokenIssuer;

        private readonly string IssuerUrl;

        public JwtMiddleware(RequestDelegate next, IOptions<SystemConfig> config)
        {
            _next = next;

            IssuerUrl = config.Value.Authenticator;

            _TokenIssuer = new RestClient(config.Value.Authenticator + "/Token");
        }

        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path;
            string token;

            if ((path.StartsWithSegments("/ClientHub")) || 
                (path.StartsWithSegments("/AdminHub" )))
            {
                token = context.Request.Query["access_token"];
            }
            else
            {
                token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            }

            if (token != null)
            {
                attachUserToContext(context,  token);
            }

            await _next(context);
        }

        private void attachUserToContext(HttpContext context, string token)
        {
            try
            {
                var tokenRequest = new AuthenticationRequest
                {
                   token = token,
                   Validator = IssuerUrl
                };
                var request = new RestRequest("Challange")
                    .AddJsonBody(tokenRequest);
                request.Method = Method.GET;

                var result = _TokenIssuer.Execute(request);
                if(result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var claim = JsonConvert.DeserializeObject<AuthenticationResponse>(result.Content);
                    // attach user to context on successful jwt


                    context.Items["IsUser"] = claim.IsUser ? "true" : "false";
                    context.Items["IsManager"] = claim.IsManager ? "true" : "false";
                    context.Items["IsAdmin"] = claim.IsAdmin ? "true" : "false";

                    context.Items["UserID"] = claim.UserID.ToString();
                }
                return;
            }
            catch
            {
                // do nothing if jwt validation fails
                // user is not attached to context so request won't have access to secure routes
            }
        }
    }
}