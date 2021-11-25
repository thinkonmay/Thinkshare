using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http.Features;
using SharedHost.Auth.ThinkmayAuthProtocol;

namespace SharedHost.Auth
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly RestClient _TokenIssuer;

        private readonly string IssuerUrl;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
            IssuerUrl = "http://authenticator/Token";
            _TokenIssuer = new RestClient("http://authenticator/Token");
        }

        public async Task Invoke(HttpContext context)
        {
            string token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
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
                var request = new RestRequest("Challenge")
                    .AddJsonBody(tokenRequest);
                request.Method = Method.POST;

                var result = _TokenIssuer.Execute(request);
                if(result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var claim = JsonConvert.DeserializeObject<AuthenticationResponse>(result.Content);
                    // attach user to context on successful jwt


                    context.Items.Add("IsUser", claim.IsUser ? "true" : "false");
                    context.Items.Add("IsManager", claim.IsManager ? "true" : "false");
                    context.Items.Add("IsAdmin", claim.IsAdmin ? "true" : "false");

                    context.Items.Add("UserID", claim.UserID);
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



    public class AuthorizeMiddleWare
    {
        private readonly RequestDelegate _next;

        public AuthorizeMiddleWare(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
 
            var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
            var userAttribute = endpoint?.Metadata.GetMetadata<UserAttribute>();
            if (userAttribute != null)
            {
                string isUser = (string)context.Items["IsUser"];
                if (isUser != "true")
                {
                    context.Response.StatusCode =  StatusCodes.Status401Unauthorized;
                    return;
                }
            }
            
            var managerAttribute = endpoint?.Metadata.GetMetadata<ManagerAttribute>();
            if (managerAttribute != null)
            {

                string isManger = (string)context.Items["IsManager"];
                if (isManger != "true")
                {
                    context.Response.StatusCode =  StatusCodes.Status401Unauthorized;
                    return;
                }
            }

            var adminAttribute = endpoint?.Metadata.GetMetadata<AdminAttribute>();
            if (adminAttribute != null)
            {

                string IsAdmin = (string)context.Items["IsAdmin"];
                if (IsAdmin != "true")
                {
                    context.Response.StatusCode =  StatusCodes.Status401Unauthorized;
                    return;
                }
            }

            await _next(context);
        }

        
    }
}