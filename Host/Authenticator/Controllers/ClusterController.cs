using Microsoft.AspNetCore.Mvc;
using DbSchema.SystemDb.Data;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SharedHost.Models.User;
using SharedHost.Models.Device;
using SharedHost.Auth.ThinkmayAuthProtocol;
using SharedHost.Models.Cluster;
using Authenticator.Interfaces;
using SharedHost;
using RestSharp;

namespace Authenticator.Controllers
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

        private readonly ITokenGenerator _token;

        public ClusterController(ApplicationDbContext db,
                                 UserManager<UserAccount> userManager)
        {
            _db = db;
            _userManager = userManager;
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
            account.ManagedCluster = new GlobalCluster 
            { 
                TURN = TURN,
                Register = DateTime.Now, 
                Private = Private 
            };
            await _userManager.UpdateAsync(account);
            var updated_cluster = (await _userManager.FindByIdAsync((string)ManagerID)).ManagedCluster;

            var token = await _token.GenerateClusterJwt(updated_cluster);
            return Ok(token);
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
