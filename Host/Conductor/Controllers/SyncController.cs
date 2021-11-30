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
using DbSchema.CachedState;
using Microsoft.Extensions.Options;

namespace Conductor.Controllers
{
    [Route("Sync/")]
    [ApiController]
    public class SyncController : ControllerBase
    {
        private readonly GlobalDbContext _db;
        private readonly IClientHub _clientHubctx;
        private readonly IWorkerCommnader _Cluster;
        private readonly UserManager<UserAccount> _userManager;
        private readonly IGlobalStateStore _cache;


        public SyncController(GlobalDbContext db,
                     IClientHub clientHub,
                     IGlobalStateStore cache,
                     UserManager<UserAccount> userManager,
                     IWorkerCommnader SlaveManager)
        {
            _db = db;
            _cache = cache;
            _Cluster = SlaveManager;
            _clientHubctx = clientHub;
            _userManager = userManager;
        }

        [HttpPost("State")]
        public async Task<IActionResult> Update(int ID, string NewState)
        {

            var Session = _db.RemoteSessions.Where(o => o.WorkerID == ID && !o.EndTime.HasValue);
            // if device is already obtained by one user (dont have endtime)
            if (Session.Any())
            {
                switch (NewState)
                {
                    case WorkerState.Open:
                        var device = _db.Devices.Find(ID);
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
                        var device = _db.Devices.Find(ID);
                        await _clientHubctx.ReportNewSlaveAvailable(device);
                        break;
                    case WorkerState.Disconnected:
                        await _clientHubctx.ReportSlaveObtained(Session.First().WorkerID);
                        break;
                }

            }
            return Ok();
        }





        

        /// <summary>
        /// Get list of available slave device, contain device information
        /// </summary>
        /// <returns></returns>
        [HttpPost("Disconnected")]
        public async Task<IActionResult> Disconnected(int ClusterID)
        {
            var cluster = _db.Clusters.Find(ClusterID);
            foreach (var worker in cluster.WorkerNode)
            {
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ClusterID"></param>
        /// <returns></returns>
        [HttpGet("GetNode")]
        public IActionResult GetClusterInfor(int ClusterID)
        {
            GlobalCluster cluster = _db.Clusters.Find(ClusterID);
            return Ok(cluster.WorkerNode);
        }
    }
}
