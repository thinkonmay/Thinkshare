using Newtonsoft.Json;
using System.Threading;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using SharedHost.Models.Cluster;
using SharedHost;
using Microsoft.Extensions.Options;
using RestSharp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Authenticator.Interfaces;
using SharedHost.Models.User;
using System;
using SharedHost.Models.AWS;
using SharedHost.Models.Device;
using System.Threading.Tasks;
using DbSchema.DbSeeding;
using SharedHost.Auth;
using DbSchema.SystemDb.Data;
using SharedHost.Logging;
using SharedHost.Models.Auth;
using System.Text;
using System.Linq;

namespace Authenticator.Controllers
{
    [ApiController]
    [Route("/Manager")]
    public class ManagerController : ControllerBase
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly GlobalDbContext _db;
        private readonly SystemConfig _config;
        private readonly ITokenGenerator _token;
        private readonly ILog _log;

        public ManagerController(UserManager<UserAccount> userManager,
                                 ITokenGenerator token,
                                 ILog log,
                                 IOptions<SystemConfig> config,
                                 GlobalDbContext db)
        {
            _userManager = userManager;
            _db = db;
            _log = log;
            _token = token;
            _config = config.Value;
        }



        [User]
        [HttpPost("Request")]
        public async Task<IActionResult> Elevate(string Description)
        {
            var UserID = HttpContext.Items["UserID"];
            var account = await _userManager.FindByIdAsync(UserID.ToString());
            
            var result = await _userManager.AddToRoleAsync(account,RoleSeeding.MOD);
            _log.Information(UserID.ToString() + " want to elevate to manager, Description: "+Description);
            return Ok(result);
        }

        [Manager]
        [HttpGet("Clusters")]
        public async Task<IActionResult> GetClusters()
        {
            var result = new List<object>();
            var UserID = int.Parse((string)HttpContext.Items["UserID"]);
            var clusters = _db.Clusters.Where(x => x.OwnerID == UserID && 
                                             !x.Unregister.HasValue).ToList();

            clusters.ForEach(x => result.Add(new {
                Name = x.Name,
                URL  = $"http://{x.instance.IPAdress}:3000/"}));
            return Ok(result);
        }


        [Manager]
        [HttpPost("Cluster/Request")]
        public async Task<IActionResult> RequestCluster(string ClusterName, 
                                                        string region,
                                                        [FromBody] string password)
        {
            if(!Region.CorrectTypo(region))
                return BadRequest("Incorrect region");

            var UserID = HttpContext.Items["UserID"].ToString();
            var user = await _userManager.FindByIdAsync(UserID);

            if(! await _userManager.CheckPasswordAsync(user,password))
                return BadRequest("Invalid password");

            if(_db.Clusters.Where(x => x.Name == ClusterName && x.OwnerID == user.Id).Any())
                return BadRequest("Choose a different name");

            var request = new RestRequest($"{_config.AutoScaling}/Instance/Managed",Method.POST)
                                    .AddQueryParameter("region",region)
                                    .AddQueryParameter("name",ClusterName)
                                    .AddQueryParameter("OwnerID",UserID)
                                    .AddJsonBody(new LoginModel{
                                        UserName = user.UserName,
                                        Password = password}) ;

            var client = new RestClient();
            client.Timeout = (int)TimeSpan.FromMinutes(10).TotalMilliseconds;
            var coturnResult = await client.ExecuteAsync(request);
            return Ok(JsonConvert.DeserializeObject<GlobalCluster>(coturnResult.Content));
        }
    }
}
