using Microsoft.AspNetCore.Mvc;
using SharedHost;
using SystemHub.Interfaces;
using SharedHost.Models.Hub;

namespace SystemHub.Controllers
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
            _Pool.BroadcastClientEventById(ID,data);
            return Ok();
        }


        [HttpPost("Broadcast")]
        public IActionResult BroadcastPost([FromBody] EventModel data)
        {
            _Pool.BroadcastClientEvent(data);
            return Ok();
        }

        [HttpPost("Admin")]
        public IActionResult AdminPost([FromBody] EventModel data)
        {
            _Pool.BroadcastAdminEvent(data);
            return Ok();
        }
    }
}
