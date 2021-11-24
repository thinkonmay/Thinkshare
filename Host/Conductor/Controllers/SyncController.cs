using Conductor.Hubs;
using SharedHost.Models.Cluster;
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
        public async Task<IActionResult> Register(int ClusterID,[FromBody] WorkerRegisterModel body)
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

            cluster.WorkerNode.Add(newWorker);
            await _db.SaveChangesAsync();

            var device = _db.Clusters.Find(ClusterID).WorkerNode.Where(o => o.Register == current).First();

            await _Cluster.AssignGlobalID(cluster.ID, device.ID, body.PrivateID);
            return Ok();
        }


        [Manager]
        [HttpPost("Cluster")]
        public async Task<IActionResult> RegisterCluster()
        {
            var UserID = HttpContext.Items["UserID"];
            var account = await _userManager.FindByIdAsync(UserID.ToString());

            if(account.ManagedCluster != null)
            {
                return Ok(account.ManagedCluster.ID);
            }
            else
            {
                account.ManagedCluster= new GlobalCluster();
                await _userManager.UpdateAsync(account);
                var updatedAccount = await _userManager.FindByIdAsync(UserID.ToString());
                return Ok(updatedAccount.ManagedCluster.ID);
            }
        }


        [HttpPost("NewWorker")]
        public async Task<IActionResult> Register(int ClusterID, [FromBody] WorkerNode worker)
        {
            worker.Register = DateTime.Now;
            var cluster = _db.Clusters.Find(ClusterID);
            cluster.WorkerNode.Add(worker);
            await _db.SaveChangesAsync();
            return Ok();
        }


        /// <summary>
        /// Get list of available slave device, contain device information
        /// </summary>
        /// <returns></returns>
        [HttpPost("Connected")]
        public async Task<IActionResult> Connected(int ClusterID)
        {
            var cluster = _db.Clusters.Find(ClusterID);
            foreach (var worker in cluster.WorkerNode)
            {
                worker.WorkerState = WorkerState.Disconnected;
                var Session = _db.RemoteSessions.Where(x => x.WorkerID == worker.ID && !x.EndTime.HasValue);
                if (Session.Any())
                {
                    await _clientHubctx.ReportSessionDisconnected(Session.First().WorkerID, Session.First().ClientId);
                    Session.First().EndTime = DateTime.Now;
                }
                else
                {
                    await _clientHubctx.ReportSlaveObtained(Session.First().WorkerID);
                }
                await _db.SaveChangesAsync();
            }
            return Ok();
        }
    }
}
