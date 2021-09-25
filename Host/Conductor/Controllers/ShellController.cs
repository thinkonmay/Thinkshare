using System.Threading.Tasks;
using Conductor.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedHost.Models.Device;
using SharedHost.Models.Shell;
using Conductor.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Conductor.Controllers
{
    /// <summary>
    /// Route use by admin to create shell remote session with slave devices
    /// </summary>
    [Authorize(Roles = "Administrator")]
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
        /// Send a command line to an specific process id of an specific slave device
        /// </summary>
        /// <param name="ModelID"></param>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        [HttpPost("Execute")]
        public async Task<IActionResult> Shell(int ModelID, int SlaveID)
        {
            if((await _slmsocket.GetSlaveState(SlaveID)).SlaveServiceState == SlaveServiceState.Disconnected)
            {
                return BadRequest("Device not available");
            }

            var model = _db.ScriptModels.Find(ModelID);
            var shell = new ShellScript(model, SlaveID);
            await _slmsocket.InitializeShellSession(shell);
            return Ok();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("AddModel")]
        public IActionResult Model([FromBody] ScriptModel model)
        {
            _db.ScriptModels.Add(model);
            return Ok();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet("GetModel")]
        public IActionResult GetModel()
        {
            return Ok(_db.ScriptModels.ToList());
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetModelHistory")]
        public IActionResult Model(int modelID)
        {
            var session = _db.ScriptModels.Find(modelID).History.ToList();
            return Ok(session);
        }
    }
}
