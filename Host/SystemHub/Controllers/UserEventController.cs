using Microsoft.AspNetCore.Mvc;
using SystemHub.Interfaces;
using SharedHost.Models.Message;

namespace SystemHub.Controllers
{
    [Route("/User/Event")]
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
    }
}
