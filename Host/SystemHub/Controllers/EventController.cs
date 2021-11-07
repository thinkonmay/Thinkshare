using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Post(int ID, string EventName, [FromBody] string data )
        {
            foreach(var item in _Pool.GetClientSockets(ID))
            {
                var i = new HubMessage
                {
                    EventName = EventName,
                    Message = data
                };
                _wsHandler.SendMessage(item,JsonConvert.SerializeObject(i));
            }
            return Ok();
        }
    }
}
