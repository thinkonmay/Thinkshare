using System.Threading.Tasks;
using Conductor.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedHost.Models.Device;
using SharedHost.Models.Shell;
using DbSchema.SystemDb.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using SharedHost.Auth.ThinkmayAuthProtocol;

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

        private readonly GlobalDbContext _db;

        public ShellController(IWorkerCommnader slmSocket, GlobalDbContext db)
        {
            _slmsocket = slmSocket;
            _db = db;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("Model/All")]
        public IActionResult Model()
        {
            var model = _db.ScriptModels.ToList();
            return Ok(model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Manager]
        [HttpPost("Model/Update")]
        public async Task<IActionResult> Update([FromBody] List<ShellSession> session)
        {
            _db.ShellSession.AddRange(session);
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}
