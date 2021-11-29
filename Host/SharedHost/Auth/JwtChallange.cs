using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http.Features;
using SharedHost.Auth.ThinkmayAuthProtocol;
using System.Net.Http;

namespace SharedHost.Auth
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly HttpClient _UserTokenIssuer;

        private readonly string IssuerUrl;

        private readonly SystemConfig _config;

        public JwtMiddleware(RequestDelegate next, IOptions<SystemConfig> config)
        {
            _next = next;
            _config = config.Value;
            _UserTokenIssuer = new HttpClient();
        }

        public async Task Invoke(HttpContext context)
        {
            string token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
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
                var request = new HttpRequestMessage(HttpMethod.Post,_config.UserTokenValidator);
                request.Content = new StringContent(
                    JsonConvert.SerializeObject(tokenRequest),null,"application/json");

                var result = await _UserTokenIssuer.SendAsync(request);
                if(result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var content = await result.Content.ReadAsStringAsync();
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