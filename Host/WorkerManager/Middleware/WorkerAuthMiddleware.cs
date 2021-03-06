using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http.Features;
using WorkerManager.Interfaces;
using SharedHost.Models.User;
using SharedHost;
using WorkerManager.Models;

namespace WorkerManager.Middleware
{
    public class ClusterJwtMiddleware
    {
        private readonly RequestDelegate _next;
        
        private readonly ITokenGenerator _generator;

        private readonly ILocalStateStore _cache;

        private readonly ClusterConfig _config;

        public ClusterJwtMiddleware(RequestDelegate next,
                            ILocalStateStore cache,
                            IOptions<ClusterConfig> config,
                            ITokenGenerator generator)
        {
            _next = next;
            _cache = cache;
            _generator = generator;
            _config = config.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            string token = context.Request.Headers["Authorization"].FirstOrDefault();
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
                ClusterWorkerNode? node = await _generator.ValidateToken(token);
                if (node != null)
                {
                    context.Items.Add("WorkerID", node.ID.ToString());
                    context.Items.Add("IsWorker", "true");
                }
                else
                {
                    context.Items.Add("IsWorker", "false");
                }


                var request = new RestRequest($"https://{_config.Domain}{_config.OwnerAuthorizeUrl}",Method.GET)
                    .AddHeader("Authorization",token);

                var result = await (new RestClient()).ExecuteAsync(request);
                var Cluster = await _cache.GetClusterInfor();
                var jsonResult = JsonConvert.DeserializeObject<UserInforModel>(result.Content);

                if(Cluster.OwnerName == jsonResult.UserName)
                {
                    context.Items.Add("IsOwner", "true");
                }
                else
                {
                    context.Items.Add("IsOwner", "false");
                }
            }
            catch
            {
                // do nothing if jwt validation fails
                // user is not attached to context so request won't have access to secure routes
            }
        }
    }



    public class ClusterAuthorizeMiddleware
    {
        private readonly RequestDelegate _next;

        public ClusterAuthorizeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        { 
            var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
            var userAttribute = endpoint?.Metadata.GetMetadata<WorkerAttribute>();
            if (userAttribute != null)
            {
                var isWorker = (string)context.Items["IsWorker"];
                if (isWorker != "true")
                {
                    context.Response.StatusCode =  StatusCodes.Status401Unauthorized;
                    return;
                }
            }

            var managerAttribute = endpoint?.Metadata.GetMetadata<OwnerAttribute>();
            if (managerAttribute != null)
            {
                var isOwner = (string)context.Items["IsOwner"];
                if (isOwner != "true")
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }
            }
            await _next(context);
        }

        
    }
}