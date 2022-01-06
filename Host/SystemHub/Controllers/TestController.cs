using System.Threading.Tasks;
using DbSchema.CachedState;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using SharedHost;
using SystemHub.Interfaces;
using RestSharp;
using System;
using SharedHost.Auth;
using Newtonsoft.Json;
using SharedHost.Models.Cluster;
using Microsoft.Extensions.Options;

namespace SystemHub.Controllers
{
    [ApiController]
    [Route("/Test")]
    [Produces("application/json")]
    public class TestController : ControllerBase
    {

        private readonly IUserSocketPool _User;
        private readonly IClusterSocketPool _Cluster;
        private readonly RestClient _TokenValidator;
        private readonly SystemConfig _config;
        private readonly IGlobalStateStore _cache;

        public TestController(IClusterSocketPool cluster,
                            IUserSocketPool user,
                            IGlobalStateStore cache,
                            IOptions<SystemConfig> config)
        {
            _User = user;
            _Cluster = cluster;
            _cache = cache;
            _config = config.Value;
            _TokenValidator = new RestClient();
        }

        [HttpGet("Cluster")]
        public async Task<IActionResult> GetUser(int ClusterID)
        {
            return Ok(await _cache.GetClusterSnapshot(ClusterID));
        }
    }
}
