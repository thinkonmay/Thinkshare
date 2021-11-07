﻿using Microsoft.AspNetCore.Mvc;
using Signalling.Interfaces;
using SharedHost;
using SystemHub.Interfaces;
using Newtonsoft.Json;
using SharedHost.Models.Hub;

namespace Signalling.Controllers
{
    [Route("/Event")]
    [ApiController]
    [Produces("application/json")]
    public class EventController : ControllerBase
    {
        private readonly IWebSocketHandler _wsHandler;

        private readonly IWebsocketPool _Pool;

        private readonly SystemConfig _config;

        public EventController(IWebSocketHandler wsHandler,
                            IWebsocketPool queue,
                            IWebsocketPool pool,
                            SystemConfig config)
        {
            _wsHandler = wsHandler;
            _Pool = queue;
            _config = config;
        }

        [HttpPost("Client")]
        public IActionResult ClientPost(int ID, [FromBody] EventModel data )
        {
            foreach(var item in _Pool.GetClientSockets(ID))
            {
                _wsHandler.SendMessage(item,JsonConvert.SerializeObject(data));
            }
            return Ok();
        }


        [HttpPost("Broadcast")]
        public IActionResult BroadcastPost([FromBody] EventModel data)
        {
            foreach (var item in _Pool.GetAllClientSockets())
            {
                _wsHandler.SendMessage(item, JsonConvert.SerializeObject(data));
            }
            return Ok();
        }

        [HttpPost("Admin")]
        public IActionResult AdminPost([FromBody] EventModel data)
        {
            foreach (var item in _Pool.GetAdminSockets())
            {
                _wsHandler.SendMessage(item, JsonConvert.SerializeObject(data));
            }
            return Ok();
        }
    }
}