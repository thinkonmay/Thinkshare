using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http.Features;
using WorkerManager.Interfaces;
using WorkerManager.Data;
using SharedHost.Models.User;
using SharedHost.Models.Local;
using SharedHost;

namespace WorkerManager.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        
        private readonly ITokenGenerator _generator;

        private readonly ClusterDbContext _db;

        private readonly RestClient _token;

        public JwtMiddleware(RequestDelegate next,
                            ClusterConfig config,
                            ITokenGenerator generator,
                            ClusterDbContext db)
        {
            _db = db;
            _next = next;
            _generator = generator;
            _token = new RestClient(config.OwnerAccountUrl);
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
                if (node == null)
                {
                    context.Items.Add("PrivateID", node.PrivateID.ToString());
                }
                else
                {
                    var request = new RestRequest("GetInfor")
                        .AddHeader("token","Bearer "+ token);
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



    public class AuthorizeMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthorizeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        { 
            var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
            var userAttribute = endpoint?.Metadata.GetMetadata<WorkerAttribute>();
            if (userAttribute != null)
            {
                var isUser = context.Items["PrivateID"];
                if (isUser == null)
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