using Microsoft.AspNetCore.Mvc;
using System.Linq;
using SystemHub.Interfaces;
using SharedHost.Models.Device;
using DbSchema.SystemDb.Data;
using System.Threading.Tasks;

namespace SystemHub.Controllers
{
    [Route("Cluster/Worker")]
    [ApiController]
    [Produces("application/json")]
    public class ClusterEventController : ControllerBase
    {
        private readonly IClusterSocketPool _Cluster;

        private readonly GlobalDbContext _db;

        public ClusterEventController(IClusterSocketPool queue,
                                      GlobalDbContext db)
        {
            _Cluster = queue;
            _db = db;
        }

        [HttpPost("Initialize")]
        public async Task<IActionResult> ClientPost(int WorkerID, string token)
        {
            var message = new Message 
            {
                From = Module.HOST_MODULE,
                To = Module.CLUSTER_MODULE,
                Opcode = Opcode.SESSION_INITIALIZE,
                WorkerID = WorkerID,
                token = token,
            };

            var worker = _db.Devices.Find(WorkerID);
            var globalCluster = _db.Clusters.Where(x => x.WorkerNode.Contains(worker)).First();
            await _Cluster.SendToCluster(globalCluster.ID, message);
            return Ok();
        }

        [HttpPost("Terminate")]
        public async Task<IActionResult> Terminate(int WorkerID)
        {
            var message = new Message 
            {
                From = Module.HOST_MODULE,
                To = Module.CLUSTER_MODULE,
                Opcode = Opcode.SESSION_TERMINATE,
                WorkerID = WorkerID,
            };

            var worker = _db.Devices.Find(WorkerID);
            var globalCluster = _db.Clusters.Where(x => x.WorkerNode.Contains(worker)).First();
            await _Cluster.SendToCluster(globalCluster.ID, message);
            return Ok();
        }

        [HttpPost("Disconnect")]
        public async Task<IActionResult> Disconnect(int WorkerID)
        {
            var message = new Message 
            {
                From = Module.HOST_MODULE,
                To = Module.CLUSTER_MODULE,
                Opcode = Opcode.SESSION_DISCONNECT,
                WorkerID = WorkerID,
            };

            var worker = _db.Devices.Find(WorkerID);
            var globalCluster = _db.Clusters.Where(x => x.WorkerNode.Contains(worker)).First();
            await _Cluster.SendToCluster(globalCluster.ID, message);
            return Ok();
        }

        [HttpPost("Reconnect")]
        public async Task<IActionResult> Reconnect(int WorkerID)
        {
            var message = new Message 
            {
                From = Module.HOST_MODULE,
                To = Module.CLUSTER_MODULE,
                Opcode = Opcode.SESSION_RECONNECT,
                WorkerID = WorkerID,
            };

            var worker = _db.Devices.Find(WorkerID);
            var globalCluster = _db.Clusters.Where(x => x.WorkerNode.Contains(worker)).First();
            await _Cluster.SendToCluster(globalCluster.ID, message);
            return Ok();
        }
    }
}
