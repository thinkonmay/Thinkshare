using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http.Features;
using SharedHost.Auth.ThinkmayAuthProtocol;
using SharedHost.Models.Cluster;
using System.Net;
using RestSharp;
using System;

namespace SharedHost.Auth
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;


        private readonly string IssuerUrl;

        private readonly SystemConfig _config;

        public JwtMiddleware(RequestDelegate next, 
                            IOptions<SystemConfig> config)
        {
            _next = next;
            _config = config.Value;
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
                   Validator = "authenticator"
                };

                var UserTokenRequest = new RestRequest(_config.Authenticator+"/Token/Challenge/User")
                    .AddJsonBody(tokenRequest);
                UserTokenRequest.Method = Method.Post;

                var ClusterTokenRequest = new RestRequest(_config.Authenticator+"/Token/Challenge/Cluster")
                    .AddJsonBody(tokenRequest);
                ClusterTokenRequest.Method = Method.Post;

                var UserTokenResult =    await (new RestClient()).ExecuteAsync(UserTokenRequest);
                if(UserTokenResult.StatusCode == HttpStatusCode.OK)
                {
                    var claim = JsonConvert.DeserializeObject<AuthenticationResponse>(UserTokenResult.Content);

                    context.Items.Add("IsUser", claim.IsUser ? "true" : "false");
                    context.Items.Add("IsManager", claim.IsManager ? "true" : "false");
                    context.Items.Add("IsAdmin", claim.IsAdmin ? "true" : "false");
                    context.Items.Add("UserID", claim.UserID);
                }

                var ClusterTokenResult = await (new RestClient()).ExecuteAsync(ClusterTokenRequest);
                if (ClusterTokenResult.StatusCode == HttpStatusCode.OK)
                {
                    var credential = JsonConvert.DeserializeObject<ClusterCredential>(ClusterTokenResult.Content);

                    context.Items.Add("IsCluster", "true" );
                    context.Items.Add("OwnerID", credential.OwnerID );
                    context.Items.Add("ClusterID", credential.ID );
                    context.Items.Add("ClusterName", credential.ClusterName );
                }
                return;
            }
            catch (Exception ex)
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
            if (clusterAttribute != null)
            {
                string IsCluster = (string)context.Items["IsCluster"];
                if (IsCluster != "true")
                {
                    context.Response.StatusCode =  StatusCodes.Status401Unauthorized;
                    return;
                }

            }

            await _next(context);
        }
    }
}