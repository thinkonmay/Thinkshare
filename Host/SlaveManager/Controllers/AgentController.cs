using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SlaveManager.Interfaces;
using System;
using System.Net;
using System.Threading.Tasks;
using SlaveManager.Data;
using SlaveManager.Services;
using SharedHost.Models;
using System.Collections.Generic;
using System.Linq;
using SlaveManager.SlaveDevices;

namespace SlaveManager.Controllers
{
    [Route("/Agent")]
    [ApiController]
    public class WebSocketApiController : ControllerBase
    {
        private readonly IWebSocketConnection _connection;

        private readonly IAdmin _admin;

        private readonly ApplicationDbContext _db;

        static private bool initialized = false;

        public WebSocketApiController(IWebSocketConnection connection, ISlavePool slavePool, ApplicationDbContext db )
        {
            _connection = connection;
            _db = db;

            //initialize device from database, only used in the first connection to slave manager
            if(!initialized)
            {
                var list = _db.Devices.ToList();
                foreach (var i in list)
                {
                    var slave = new SlaveDevice(_admin);
                    slavePool.AddSlaveId(i.ID,slave);
                }
            }
        }

        [HttpGet("/Register")]
        public async Task<IActionResult> Get()
        {
            var context = ControllerContext.HttpContext;

            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();    
                await _connection.KeepReceiving(webSocket);
                await _connection.Close(webSocket);
                return new EmptyResult();
            }
            else
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }
        }
    }
}
