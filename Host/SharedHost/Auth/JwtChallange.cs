using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http.Features;
using SharedHost.Auth.ThinkmayAuthProtocol;
using System.Net;
using RestSharp;

namespace SharedHost.Auth
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly RestClient _UserTokenIssuer;

        private readonly string IssuerUrl;

        private readonly SystemConfig _config;

        public JwtMiddleware(RequestDelegate next, 
                            IOptions<SystemConfig> config)
        {
            _next = next;
            _config = config.Value;
            _UserTokenIssuer = new RestClient();
        }

        public async Task Invoke(HttpContext context)
        {
            string token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            context.Items.Add("Token", token);
            if (token != null)
            {
                await attachUserToContext(context,  token);
            }
            await _next(context);
        }

        private async Task attachUserToContext(HttpContext context, string token)
        {
            try
            {
                var tokenRequest = new AuthenticationRequest
                {
                   token = token,
                   Validator = _config.UserTokenValidator
                };
                var request = new RestRequest(_config.UserTokenValidator)
                    .AddJsonBody(JsonConvert.SerializeObject(tokenRequest));
                request.Method = Method.POST;

                var result = await _UserTokenIssuer.ExecuteAsync(request);
                if(result.StatusCode == HttpStatusCode.OK)
                {
                    var content = result.Content;
                    var claim = JsonConvert.DeserializeObject<AuthenticationResponse>(content);
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

        private readonly SystemConfig _config;

        public AuthorizeMiddleWare(RequestDelegate next, 
                                   IOptions<SystemConfig> config)
        {
            _next = next;
            _config = config.Value;
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

            var clusterAttribute = endpoint?.Metadata.GetMetadata<ClusterAttribute>();
            if (adminAttribute != null)
            {
                try
                {
                    var request = new RestRequest(_config.UserTokenValidator)
                        .AddQueryParameter("token",(string)context.Items["Token"]);
                    request.Method = Method.POST;

                    var result = await (new RestClient()).ExecuteAsync(request);
                    if(result.StatusCode == HttpStatusCode.OK)
                    {
                        var content = result.Content;
                        var claim = JsonConvert.DeserializeObject<AuthenticationResponse>(content);
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

            await _next(context);
        }
    }
}