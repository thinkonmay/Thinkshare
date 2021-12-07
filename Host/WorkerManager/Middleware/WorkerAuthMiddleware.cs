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
using DbSchema.LocalDb;
using DbSchema.LocalDb.Models;

namespace WorkerManager.Middleware
{
    public class ClusterJwtMiddleware
    {
        private readonly RequestDelegate _next;
        
        private readonly ITokenGenerator _generator;

        private readonly ClusterDbContext _db;

        private readonly RestClient _token;

        private readonly ClusterConfig _config;

        public ClusterJwtMiddleware(RequestDelegate next,
                            IOptions<ClusterConfig> config,
                            ITokenGenerator generator,
                            ClusterDbContext db)
        {
            _db = db;
            _next = next;
            _generator = generator;
            _config = config.Value;
            _token = new RestClient();
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
                ClusterWorkerNode node = await _generator.ValidateToken(token);
                if (node != null)
                {
                    context.Items.Add("PrivateID", node.ID.ToString());
                    context.Items.Add("IsWorker", "true");
                }
                else
                {
                    context.Items.Add("IsWorker", "false");
                    var request = new RestRequest(_config.OwnerAuthorizeUrl)
                        .AddHeader("Authorization","Bearer "+ token);
                    request.Method = Method.GET;

                    var result = await _token.ExecuteAsync(request);

                    if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var jsonResult = JsonConvert.DeserializeObject<UserInforModel>(result.Content);
                        if(jsonResult != null)
                        {
                            if(_db.Owner.Where(o => o.Name == jsonResult.UserName).Any())
                            {
                                context.Items.Add("IsOwner", "true");
                            }
                            else
                            {
                                context.Items.Add("IsOwner", "false");
                            }
                        }
                    }
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