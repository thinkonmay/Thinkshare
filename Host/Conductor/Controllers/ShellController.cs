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
        public async Task<IActionResult> Model([FromBody] ScriptModel model)
        {
            try
            {
                _db.ScriptModels.Add(model);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
            return Ok();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetModelHistory")]
        public IActionResult Model(int modelID, int SlaveID)
        {
            List<ShellSession> session;
            try
            {
                session = _db.ScriptModels.Find(modelID).History.Where(o => o.Slave.ID == SlaveID).ToList();
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
            return Ok(session);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetModel")]
        public IActionResult Model()
        {
            var ret = new List<ScriptModel>();
            var model = _db.ScriptModels.ToList();
            foreach ( var item in model)
            {
                item.History = null;
                ret.Add(item);
            };
            return Ok(ret);
        }
    }
}
