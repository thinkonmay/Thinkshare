using Microsoft.AspNetCore.Mvc;
using DbSchema.SystemDb.Data;
using Conductor.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SharedHost.Models.User;
using SharedHost.Models.Device;
using SharedHost.Auth.ThinkmayAuthProtocol;
using SharedHost.Models.Cluster;
using SharedHost;
using RestSharp;

namespace Conductor.Controllers
{
    /// <summary>
    /// Routes used by user to fetch information about the system
    /// </summary>
    [Manager]
    [ApiController]
    [Route("/Cluster")]
    [Produces("application/json")]
    public class ClusterController : Controller
    {
        private readonly UserManager<UserAccount> _userManager;

        private readonly ApplicationDbContext _db;

        private readonly IWorkerCommnader _slmsocket;

        private readonly RestClient _restClient;

        public ClusterController(ApplicationDbContext db,
                            UserManager<UserAccount> userManager,
                            IWorkerCommnader slm,
                            SystemConfig config)
        {
            _slmsocket = slm;
            _db = db;
            _userManager = userManager;
            _restClient = new RestClient(config.Authenticator + "/Token");
        }





        /// <summary>
        /// Get list of available slave device, contain device information
        /// </summary>
        /// <returns></returns>
        [HttpPost("Register")]
        public async Task<IActionResult> NewCluster(bool Private , string TURN)
        {
            var ManagerID = HttpContext.Items["UserID"];
            UserAccount account =  await _userManager.FindByIdAsync((string)ManagerID);
            account.ManagedCluster.Add(new GlobalCluster 
            { 
                TURN = TURN,
                Register = DateTime.Now, 
                Private = Private 
            });
            await _userManager.UpdateAsync(account);
            var updated_cluster = (await _userManager.FindByIdAsync((string)ManagerID)).ManagedCluster.First();

            var request = new RestRequest("GrantCluster")
                .AddJsonBody(updated_cluster);
            request.Method = Method.POST;

            var result = await _restClient.ExecuteAsync(request);
            return Ok(result.Content);
        }


        /// <summary>
        /// Get list of available slave device, contain device information
        /// </summary>
        /// <returns></returns>
        [HttpPost("Infor")]
        public IActionResult GetInfor(int ID)
        {
            return Ok(_db.Clusters.Find(ID));
        }
    }
}
