﻿using System.Threading.Tasks;
using Conductor.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedHost.Models.Shell;
using DbSchema.SystemDb.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using SharedHost.Auth.ThinkmayAuthProtocol;
using SharedHost.Models.User;

namespace Conductor.Controllers
{
    /// <summary>
    /// Route use by admin to create shell remote session with slave devices
    /// </summary>
    [Route("/Shell")]
    [ApiController]
    public class ShellController : Controller
    {
        private readonly IWorkerCommnader _slmsocket;
        private readonly UserManager<UserAccount> _userManager;
        private readonly GlobalDbContext _db;

        public ShellController(UserManager<UserAccount> userManager,
                            IWorkerCommnader slmSocket, 
                            GlobalDbContext db )
        {
            _slmsocket = slmSocket;
            _userManager = userManager;
            _db = db;
        }



        [HttpGet("Model/All")]
        public IActionResult Model()
        {
            var model = _db.ScriptModels.ToList();
            return Ok(model);
        }


        [Manager]
        [HttpPost("Add")]
        public async Task<IActionResult> UpdateShellSession([FromBody] List<ShellSession> session, int WorkerID, string ClusterName)
        {
            var UserID = HttpContext.Items["UserID"];
            var manager = await _userManager.FindByIdAsync(UserID.ToString());
            var cluster = manager.ManagedCluster.Where(x => x.Name == ClusterName).First();
            var worker = cluster.WorkerNode.Where(x => x.ID == WorkerID).First();

            if (worker == null)
            {
                return BadRequest();
            }

            worker.Shells.Union(session);
            _db.Update(worker);

            await _db.SaveChangesAsync();

            return Ok();
        }
    }
}
