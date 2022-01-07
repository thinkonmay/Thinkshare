using Newtonsoft.Json;
using SharedHost;
using Microsoft.Extensions.Options;
using RestSharp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Authenticator.Interfaces;
using SharedHost.Models.User;
using System;
using SharedHost.Models.AWS;
using System.Threading.Tasks;
using DbSchema.DbSeeding;
using SharedHost.Auth.ThinkmayAuthProtocol;
using System.Linq;
using DbSchema.SystemDb.Data;

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

        public ManagerController(UserManager<UserAccount> userManager,
                                 ITokenGenerator token,
                                 IOptions<SystemConfig> config,
                                 GlobalDbContext db)
        {
            _userManager = userManager;
            _db = db;
            _token = token;
        }



        [User]
        [HttpPost("Request")]
        public async Task<IActionResult> Elevate(string Description, 
                                                 bool SelfHost, 
                                                 string ClusterName)
        {
            var UserID = HttpContext.Items["UserID"];
            var account = await _userManager.FindByIdAsync(UserID.ToString());
            
            var result = await _userManager.AddToRoleAsync(account,RoleSeeding.MOD);
            Serilog.Log.Information(UserID.ToString() + " want to elevate to manager, Description: "+Description);
            return Ok(result);
        }

        [Manager]
        [HttpPost("/ManagedCluster/Request")]
        public async Task<IActionResult> RequestCluster( string ClusterName,
                                                         bool Private)
        {
            var UserID = HttpContext.Items["UserID"];
            var account = await _userManager.FindByIdAsync(UserID.ToString());

            var request = new RestRequest(_config.ManagedClusterServiceGrantor);
            request.Method = Method.GET;

            var coturnResult = await (new RestClient()).ExecuteAsync(request);
            ClusterInstance instance = JsonConvert.DeserializeObject<ClusterInstance>(coturnResult.Content);

            var managerToken = await _token.GenerateUserJwt(account);

            var clusterRequest = new RestRequest(_config.AutoScaling + "/Cluster/Register")
                .AddHeader("Authorization","Bearer "+managerToken)
                .AddQueryParameter("ClusterName",ClusterName)
                .AddQueryParameter("Private",Private.ToString());
            request.Method = Method.POST;

            var clusterResult = await (new RestClient()).ExecuteAsync(clusterRequest); 
            var token = JsonConvert.DeserializeObject<string>(clusterResult.Content);


            var result = _token.ValidateClusterToken(token);
            var registeredCluster = _db.Clusters.Find(result.Id);

            registeredCluster.instance = instance;

            _db.Update(registeredCluster);
            await _db.SaveChangesAsync();
            return Ok(registeredCluster);
        }

        [Manager]
        [HttpPost("/Cluster/Unregister")]
        public async Task<IActionResult> UnregisterCluster( string ClusterName)
        {
            var UserID = HttpContext.Items["UserID"];
            var account = await _userManager.FindByIdAsync(UserID.ToString());

            var cluster = account.ManagedCluster.Where(x => x.Name == ClusterName).First();
            cluster.Unregister = DateTime.Now;



            var clusterRequest = new RestRequest(_config.AutoScaling + "/Instance/Terminate")
                .AddQueryParameter("ID",cluster.instance.InstanceID);
            var clusterResult = await (new RestClient()).ExecuteAsync(clusterRequest); 
            var success = JsonConvert.DeserializeObject<bool>(clusterResult.Content);
            if (success)
            {
                cluster.Unregister = DateTime.Now;
                await _userManager.UpdateAsync(account);
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
