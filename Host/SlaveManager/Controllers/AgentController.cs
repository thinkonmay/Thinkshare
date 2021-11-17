using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkerManager.Interfaces;
using System;
using System.Net;
using System.Threading.Tasks;

namespace WorkerManager.Controllers
{
    [ApiController]
    [Route("/Agent")]
    [Produces("application/json")]
    public class WebSocketApiController : ControllerBase
    {
        public WebSocketApiController(ISlavePool slavePool)
        {
        }

        [HttpGet("Register")]
        public async Task<IActionResult> Get()
        {
            return new EmptyResult();
        }
    }
}
