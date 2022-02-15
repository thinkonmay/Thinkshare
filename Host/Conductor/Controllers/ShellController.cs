using System.Threading.Tasks;
using Conductor.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedHost.Models.Shell;
using DbSchema.SystemDb.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using SharedHost.Auth;
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
        private readonly GlobalDbContext _db;

        public ShellController( IWorkerCommnader slmSocket, 
                                GlobalDbContext db )
        {
            _slmsocket = slmSocket;
            _db = db;
        }

        [HttpGet("Model/All")]
        public IActionResult Model()
        {
            var model = _db.ScriptModels.ToList();
            return Ok(model);
        }
    }
}
