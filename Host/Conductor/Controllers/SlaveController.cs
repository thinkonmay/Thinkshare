using Conductor.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedHost.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Conductor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SlaveController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public SlaveController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Register(SlaveSession session)
        {
            var slave = _db.Sessions.Where(p => p.SessionClientID == session.SessionSlaveID).FirstOrDefault();

            if (slave != null)
            {
                return Ok();
            }


        }
    }
}
