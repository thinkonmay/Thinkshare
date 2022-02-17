using Conductor.Hubs;
using Conductor.Interfaces;
using DbSchema.SystemDb.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SharedHost.Models.Device;
using SharedHost.Models.User;
using System;
using System.Linq;
using System.Threading.Tasks;
using DbSchema.CachedState;
using SharedHost.Logging;

namespace Conductor.Controllers
{
    [Route("Sync/")]
    [ApiController]
    public class SyncController : ControllerBase
    {
        private readonly GlobalDbContext _db;
        private readonly IClientHub _clientHubctx;
        private readonly IWorkerCommnader _Cluster;
        private readonly IGlobalStateStore _cache;
        private readonly ILog _log;


        public SyncController(GlobalDbContext db,
                     IClientHub clientHub,
                     IGlobalStateStore cache,
                     ILog log,
                     IWorkerCommnader SlaveManager)
        {
            _db = db;
            _log = log;
            _cache = cache;
            _Cluster = SlaveManager;
            _clientHubctx = clientHub;
        }

        [HttpPost("Worker/State")]
        public async Task<IActionResult> Update(int ID, string NewState)
        {
            var Sessions = _db.RemoteSessions.Where(o => o.WorkerID == ID && 
                                                   !o.EndTime.HasValue);
            _log.Information("Got worker sync message from worker"+ID.ToString()+", new worker state: "+NewState);            


            // if device is already obtained by one user (dont have endtime)
            if (Sessions.Any())
            {
                _log.Information("Worker is already in session ");            
                var session = Sessions.First();
                switch (NewState)
                {
                    case WorkerState.Open:
                        await _clientHubctx.ReportNewSlaveAvailable(ID);
                        await _clientHubctx.ReportSessionTerminated(ID,session.ClientId);

                        session.EndTime = DateTime.Now;
                        _db.RemoteSessions.Update(session);
                        await _db.SaveChangesAsync();
                        break;
                    case WorkerState.Disconnected:
                        await _clientHubctx.ReportSessionTerminated(ID,session.ClientId);

                        session.EndTime = DateTime.Now;
                        _db.RemoteSessions.Update(session);
                        await _db.SaveChangesAsync();
                        break;
                    case WorkerState.OffRemote:
                        await _clientHubctx.ReportSessionDisconnected(ID, session.ClientId);
                        await _clientHubctx.ReportSlaveObtained(ID);

                        session.StartTime = session.StartTime.HasValue ? session.StartTime : DateTime.Now;
                        _db.RemoteSessions.Update(session);
                        await _db.SaveChangesAsync();
                        break;
                    case WorkerState.OnSession:
                        await _clientHubctx.ReportSessionInitialized(ID, session.ClientId);
                        await _clientHubctx.ReportSlaveObtained(ID);


                        session.StartTime = session.StartTime.HasValue ? session.StartTime : DateTime.Now;
                        _db.RemoteSessions.Update(session);
                        await _db.SaveChangesAsync();
                        break;
                }
            }
            // if device is not obtained by any one (has endtime)
            else
            {
                _log.Information("Worker is not in session ");            
                switch (NewState)
                {
                    case WorkerState.Open:
                        await _clientHubctx.ReportNewSlaveAvailable(ID);
                        break;
                    case WorkerState.Disconnected:
                        await _clientHubctx.ReportSlaveObtained(ID);
                        break;
                }

            }
            _log.Information("Handle event done");
            return Ok();
        }





        

        [HttpPost("Cluster/Disconnected")]
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
                    await _clientHubctx.ReportSlaveObtained(worker.ID);
                }
                await _db.SaveChangesAsync();
            }
            return Ok();
        }
    }
}
