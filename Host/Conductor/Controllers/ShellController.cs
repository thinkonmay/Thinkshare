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
        private readonly ISlaveManagerSocket _slmsocket;

        private readonly ApplicationDbContext _db;

        public ShellController(ISlaveManagerSocket slmSocket, ApplicationDbContext db)
        {
            _slmsocket = slmSocket;
            _db = db;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Manager]
        [HttpGet("AllModel")]
        public IActionResult Model()
        {
            var model = _db.ScriptModels.ToList();
            return Ok(model);
        }
    }
}
