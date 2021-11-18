using Microsoft.AspNetCore.Mvc;
using SharedHost;
using SystemHub.Interfaces;
using SharedHost.Models.Hub;

namespace SystemHub.Controllers
{
    [Route("/UserEvent")]
    [ApiController]
    [Produces("application/json")]
    public class UserEventController : ControllerBase
    {
        private readonly IUserSocketPool _User;

        public UserEventController(IUserSocketPool queue)
        {
            _User = queue;
        }

        [HttpPost("Client")]
        public IActionResult ClientPost(int ID, [FromBody] EventModel data )
        {
            _User.BroadcastClientEventById(ID,data);
            return Ok();
        }


        [HttpPost("Broadcast")]
        public IActionResult BroadcastPost([FromBody] EventModel data)
        {
            _User.BroadcastClientEvent(data);
            return Ok();
        }

        [HttpPost("Admin")]
        public IActionResult AdminPost([FromBody] EventModel data)
        {
            _User.BroadcastAdminEvent(data);
            return Ok();
        }
    }
}
