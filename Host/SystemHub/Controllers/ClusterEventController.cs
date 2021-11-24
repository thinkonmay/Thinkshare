using Microsoft.AspNetCore.Mvc;
using SharedHost;
using SystemHub.Interfaces;
using SharedHost.Models.Hub;
using SharedHost.Models.Session;
using Newtonsoft.Json;
using SharedHost.Models.Device;
using SharedHost.Models.Cluster;

namespace SystemHub.Controllers
{
    [Route("Cluster")]
    [ApiController]
    [Produces("application/json")]
    public class ClusterEventController : ControllerBase
    {
        private readonly IClusterSocketPool _Cluster;

        public ClusterEventController(IClusterSocketPool queue)
        {
            _Cluster = queue;
        }

        [HttpPost("Initialize")]
        public IActionResult ClientPost(int WorkerID, string token, [FromBody] SessionBase session)
        {
            var message = new Message 
            {
                From = Module.HOST_MODULE,
                To = Module.CLUSTER_MODULE,
                Opcode = Opcode.SESSION_INITIALIZE,
                WorkerID = WorkerID,
                token = token,
                Data = JsonConvert.SerializeObject(session)
            };
            _Cluster.SendToNode(message);
            return Ok();
        }

        [HttpPost("Terminate")]
        public IActionResult Terminate(int WorkerID)

        {
            var message = new Message 
            {
                From = Module.HOST_MODULE,
                To = Module.CLUSTER_MODULE,
                Opcode = Opcode.SESSION_TERMINATE,
                WorkerID = WorkerID,
            };
            _Cluster.SendToNode(message);
            return Ok();
        }

        [HttpPost("Disconnect")]
        public IActionResult Disconnect(int WorkerID)
        {
            var message = new Message 
            {
                From = Module.HOST_MODULE,
                To = Module.CLUSTER_MODULE,
                Opcode = Opcode.SESSION_DISCONNECT,
                WorkerID = WorkerID,
            };
            _Cluster.SendToNode(message);
            return Ok();
        }

        [HttpPost("Reconnect")]
        public IActionResult Reconnect(int WorkerID, [FromBody] SessionBase data)
        {
            var message = new Message 
            {
                From = Module.HOST_MODULE,
                To = Module.CLUSTER_MODULE,
                Opcode = Opcode.SESSION_RECONNECT,
                WorkerID = WorkerID,
                Data = JsonConvert.SerializeObject(data)
            };
            _Cluster.SendToNode(message);
            return Ok();
        }

        [HttpPost("GrantID")]
        public IActionResult Reconnect(int ClusterID, int GlobalID, int PrivateID)
        {
            var IDAssign = new IDAssign
            {
                GlobalID = GlobalID,
                PrivateID = PrivateID,
                ClusterID = ClusterID
            };
            var message = new Message 
            {
                From = Module.HOST_MODULE,
                To = Module.CLUSTER_MODULE,
                Opcode = Opcode.ID_GRANT,
                Data = JsonConvert.SerializeObject(IDAssign)
            };
            _Cluster.SendToCluster(ClusterID, message);
            return Ok();
        }
    }
}
