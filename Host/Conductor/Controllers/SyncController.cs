using Conductor.Hubs;
using Conductor.Interfaces;
using DbSchema.SystemDb.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SharedHost;
using SharedHost.Auth.ThinkmayAuthProtocol;
using SharedHost.Models.Device;
using SharedHost.Models.User;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Conductor.Controllers
{
    [Route("Sync/")]
    [ApiController]
    public class SyncController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IClientHub _clientHubctx;
        private readonly IWorkerCommnader _Cluster;
        private readonly UserManager<UserAccount> _userManager;

        public SyncController(ApplicationDbContext db,
                     IClientHub clientHub,
                     UserManager<UserAccount> userManager,
                     IWorkerCommnader SlaveManager,
                     SystemConfig config)
        {
            _db = db;
            _clientHubctx = clientHub;
            _Cluster = SlaveManager;
            _userManager = userManager;
        }

        [HttpPost("State")]
        public async Task<IActionResult> Update(int ID, string NewState)
        {
            var device = _db.Devices.Find(ID);
            device.WorkerState = NewState;

            var Session = _db.RemoteSessions.Where(o => o.WorkerID == ID && !o.EndTime.HasValue);

            // if device is already obtained by one user (dont have endtime)
            if (Session.Any())
            {
                switch (NewState)
                {
                    case WorkerState.Open:
                        await _clientHubctx.ReportNewSlaveAvailable(device);
                        Session.First().EndTime = DateTime.Now;
                        break;
                    case WorkerState.Disconnected:
                        await _clientHubctx.ReportSessionDisconnected(Session.First().WorkerID, Session.First().ClientId);
                        Session.First().EndTime = DateTime.Now;
                        break;
                    case WorkerState.OffRemote:
                        await _clientHubctx.ReportSessionDisconnected(Session.First().WorkerID, Session.First().ClientId);
                        break;
                    case WorkerState.OnSession:
                        await _clientHubctx.ReportSessionInitialized(Session.First().Worker, Session.First().ClientId);
                        break;
                }
            }
            // if device is not obtained by any one (has endtime)
            else
            {
                switch (NewState)
                {
                    case WorkerState.Open:
                        await _clientHubctx.ReportNewSlaveAvailable(device);
                        break;
                    case WorkerState.Disconnected:
                        await _clientHubctx.ReportSlaveObtained(Session.First().WorkerID);
                        break;
                }

            }

            await _db.SaveChangesAsync();
            return Ok();
        }


        [HttpPost("Register")]
        public async Task<IActionResult> Register(int ClusterID,[FromBody] ClusterWorkerNode body)
        {
            var cluster = _db.Clusters.Find(ClusterID);
            var current = DateTime.Now;

            var newWorker = new WorkerNode
            {
                Register = current,
                WorkerState = WorkerState.Registering,
                CPU = body.CPU,
                GPU = body.GPU,
                RAMcapacity = body.RAMcapacity,
                OS = body.OS
            };

            cluster.Slave.Add(newWorker);
            await _db.SaveChangesAsync();

            var device = _db.Clusters.Slave.Where(o => o.Register == current).First();

            await _Cluster.AssignGlobalID(cluster.ID, device.ID, body.PrivateID);
        }
    }
}
