using Microsoft.AspNetCore.Mvc;
using SharedHost;
using SystemHub.Interfaces;
using SharedHost.Models.Hub;

namespace SystemHub.Controllers
{
    [Route("/UserEvent")]
    [ApiController]
    [Produces("application/json")]
    public class ClusterEventController : ControllerBase
    {
        private readonly IClusterSocketPool _Cluster;

        public ClusterEventController(IClusterSocketPool queue)
        {
            _Cluster = queue;
        }

        [HttpPost("Client")]
        public IActionResult ClientPost(int ID, [FromBody] EventModel data )
        {
            return Ok();
        }


        [HttpPost("Broadcast")]
        public IActionResult BroadcastPost([FromBody] EventModel data)
        {
            return Ok();
        }

        [HttpPost("Admin")]
        public IActionResult AdminPost([FromBody] EventModel data)
        {
            return Ok();
        }
    }
}
