
using Newtonsoft.Json;
using SharedHost.Models.Auth;
using SharedHost.Models.Cluster;
using RestSharp;
using Microsoft.AspNetCore.Mvc;
using WorkerManager.Interfaces;
using System;
using System.Net;
using System.Threading.Tasks;
using SharedHost.Models.Device;
using WorkerManager.Middleware;
using System.Linq;
using WorkerManager;
using Microsoft.Extensions.Options;
using SharedHost;

using WorkerManager.Models;

namespace WorkerManager.Controllers
{
    [ApiController]
    [Route("/token")]
    [Produces("application/json")]
    public class TokenController : ControllerBase
    {
        private readonly ITokenGenerator _tokenGenerator;

        private readonly ILocalStateStore _cache;

        private readonly ClusterConfig _config;

        public TokenController( ITokenGenerator token,
                                ILocalStateStore cache,
                                IOptions<ClusterConfig> config)
        {
            _cache = cache;
            _config = config.Value;
            _tokenGenerator = token;
        }

        [Route("validate")]
        [HttpPost]
        public async Task<IActionResult> TokenValidation(string token)
        {
            var result = await _tokenGenerator.ValidateToken(token);
            if(result != null)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
