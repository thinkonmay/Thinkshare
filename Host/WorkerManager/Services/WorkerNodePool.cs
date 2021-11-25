using WorkerManager.Models;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using WorkerManager.Interfaces;
using SharedHost.Models.Shell;
using SharedHost.Models.Device;
using System.Threading.Tasks;
using SharedHost;
using WorkerManager.Data;

namespace WorkerManager.Services
{
    public class WorkerNodePool 
    {
        private readonly IConductorSocket _socket;

        private List<ClusterWorkerNode> _systemSnapshot;

        private List<ScriptModel> _model_list;

        private readonly ClusterDbContext _db;

        public WorkerNodePool(IConductorSocket socket, ClusterDbContext db)
        {
            _db = db;
            _socket = socket;
            _systemSnapshot = _db.Devices.ToList();
            Task.Run(() => SystemHeartBeat());
            Task.Run(() => StateSyncing());
            Task.Run(() => SessionHeartBeat());
        }

        public async Task SessionHeartBeat()
        {
            try
            {
                while(true)
                {
                    var devices = _systemSnapshot.Where(x => x._workerState == WorkerState.OnSession);
                    foreach (var item in devices)
                    {
                        item.RestoreWorkerNode();
                        var result = await item.PingSession();
                        if(result)
                        {
                           item.sessionFailedPing = 0; 
                        }
                        else
                        {
                            item.sessionFailedPing++;
                        }

                        if(item.sessionFailedPing > 5)
                        {
                            item._workerState = WorkerState.OffRemote;
                            await _db.SaveChangesAsync();
                        }
                    }
                    Thread.Sleep(((int)TimeSpan.FromSeconds(1).TotalMilliseconds));
                }
            }
            catch (Exception ex)
            {
                Thread.Sleep(((int)TimeSpan.FromSeconds(1).TotalMilliseconds));
                await SessionHeartBeat();
            }
        }

        public async Task SystemHeartBeat()
        {
            _model_list = await _socket.GetDefaultModel();
            try
            {
                while(true)
                {
                    foreach(var i in _model_list)
                    {
                        foreach(var device in _systemSnapshot)
                        {
                            device.RestoreWorkerNode();
                            var session = new ShellSession { Script = i.Script };
                            var result = await device.PingWorker(session);
                            if(result != null)
                            {
                                session.Output = result;
                                session.ModelID = i.ID;
                                session.WorkerID = device.PrivateID;
                                session.Time = DateTime.Now;
                                device.agentFailedPing = 0;
                            }
                            else
                            {
                                device.agentFailedPing++;
                            }

                            if(device.agentFailedPing > 5)
                            {
                                device._workerState = WorkerState.Disconnected;
                            }
                            if(session != null)
                            {
                                _db.CachedSession.Add(session);
                                await _db.SaveChangesAsync();
                            }
                        }
                    }
                    Thread.Sleep(((int)TimeSpan.FromSeconds(10).TotalMilliseconds));
                }
            }catch (Exception ex)
            {
                Thread.Sleep(((int)TimeSpan.FromSeconds(10).TotalMilliseconds));
                await SystemHeartBeat();
            }
        }

        public async Task StateSyncing()
        {
            try
            {
                while (true)
                {

                    foreach ( var unsyncedDevice in _systemSnapshot)
                    {

                        var snapshotedDevice = _db.Devices.Find(unsyncedDevice.PrivateID);

                        // if device is not available in database, then remove it from system snapshoot
                        if(snapshotedDevice == null)
                        {
                            _systemSnapshot.Remove(unsyncedDevice);
                            await _db.SaveChangesAsync();
                        }

                        // if device haven't been registered, register it
                        if (snapshotedDevice._workerState == WorkerState.Unregister)
                        {
                            await _socket.ReportWorkerRegistered(snapshotedDevice);
                            snapshotedDevice._workerState = WorkerState.Registering;
                            await _db.SaveChangesAsync();
                            continue;
                        }

                        // if device is registering, continue
                        if (snapshotedDevice._workerState == WorkerState.Registering)
                        {
                            continue;
                        }

                        // otherwise, sync its state 
                        if (snapshotedDevice._workerState != unsyncedDevice._workerState)
                        {
                            await _socket.WorkerStateSyncing((int)snapshotedDevice.GlobalID, snapshotedDevice._workerState);
                            _systemSnapshot.Remove(unsyncedDevice);
                            _systemSnapshot.Add(snapshotedDevice);
                            continue;
                        }
                    }
                Thread.Sleep(((int)TimeSpan.FromMilliseconds(100).TotalMilliseconds));
                }
            }
            catch (Exception ex)
            {
                Thread.Sleep(((int)TimeSpan.FromSeconds(1).TotalMilliseconds));
                await StateSyncing();
            }
        }
    }
}
