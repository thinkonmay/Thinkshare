﻿using Microsoft.AspNetCore.Mvc;
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
using System.Linq;

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

        private readonly GlobalDbContext _db;

        private readonly ITokenGenerator _token;

        public ClusterController(GlobalDbContext db,
                                 UserManager<UserAccount> userManager,
                                 ITokenGenerator token)
        {
            _db = db;
            _token = token;
            _userManager = userManager;
        }





        /// <summary>
        /// Get list of available slave device, contain device information
        /// </summary>
        /// <returns></returns>
        [Manager]
        [HttpPost("Register")]
        public async Task<IActionResult> NewCluster(string ClusterName, bool Private)
        {
            var ManagerID = HttpContext.Items["UserID"];
            UserAccount account =  await _userManager.FindByIdAsync((string)ManagerID);
            if(account.ManagedCluster.Where(x => x.Name == ClusterName).Any())
            {
                return BadRequest("Choose a different name");
            }
            var cluster = new GlobalCluster
            {
                Name = ClusterName,
                Register = DateTime.Now,
                Private = Private
            };
            account.ManagedCluster.Add(cluster);
            await _userManager.UpdateAsync(account);

            return Ok();
        }

        /// <summary>
        /// Get list of available slave device, contain device information
        /// </summary>
        /// <returns></returns>
        [Manager]
        [HttpGet("Token")]
        public async Task<IActionResult> GetToken(string ClusterName)
        {
            var ManagerID = HttpContext.Items["UserID"];
            UserAccount account = await _userManager.FindByIdAsync((string)ManagerID);
            var cluster = account.ManagedCluster.Where(x => x.Name == ClusterName);
            if(!cluster.Any())
            {
                return BadRequest("Cluster not found");
            }

            var token = await _token.GenerateClusterJwt((string)ManagerID,ClusterName);
            return Ok(token);
        }


        [Manager]
        [HttpGet("Turn")]
        public async Task<IActionResult> SetTURN(string ClusterName, 
                                                string turnIP, 
                                                string turnUSER, 
                                                string turnPASSWORD)
        {
            var ManagerID = HttpContext.Items["UserID"];
            UserAccount account = await _userManager.FindByIdAsync((string)ManagerID);
            var cluster = account.ManagedCluster.Where(x => x.Name == ClusterName).FirstOrDefault();
            if (cluster == null)
            {
                return BadRequest("Cluster not found");
            }



            cluster.turnIP = turnIP;
            cluster.turnUSER = turnUSER;
            cluster.turnPASSWORD = turnPASSWORD;    
            _db.Update(cluster);
            await _db.SaveChangesAsync();
            return Ok();
        }

    }
}
