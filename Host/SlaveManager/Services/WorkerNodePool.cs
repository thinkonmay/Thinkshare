using WorkerManager.SlaveDevices;
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
    public class WorkerNodePool : IWorkerNodePool
    {
        private readonly SystemConfig _config;

        private readonly IConductorSocket _socket;

        private readonly ClusterDbContext _db;

        private List<ClusterWorkerNode> _systemSnapshot;

        public WorkerNodePool(SystemConfig config, ClusterDbContext db, IConductorSocket socket)
        {
            _db = db;
            _config = config;
            _socket = socket;
            Task.Run(() => SystemHeartBeat());
            Task.Run(() => StateSyncing());
        }

        public async Task SystemHeartBeat()
        {
            try
            {
                var model_list = await _socket.GetDefaultModel();
                while(true)
                {
                    foreach(var i in model_list)
                    {
                        var devices = _db.Devices.ToList();
                        foreach(var device in devices)
                        {
                            // if a device is not exist on system snapshot, add it even globalID is null
                            if (_systemSnapshot.Where(o => o.PrivateID == device.PrivateID).Count() == 0)
                            {
                                _systemSnapshot.Add(device);
                                continue;
                            }

                            device.RestoreWorkerNode(_config);
                            var session = new ShellSession { Script = i.Script };
                            var result = await device.PingWorker(session);
                            if(result != null)
                            {
                                session.Output = result;
                                session.ModelID = i.ID;
                                session.WorkerID = device.PrivateID;
                                session.Time = DateTime.Now;
                                _db.CachedSession.Add(session);
                            }
                            else
                            {
                                device._workerState = WorkerState.Disconnected;
                            }
                        }
                    }
                    await _db.SaveChangesAsync();
                    Thread.Sleep(10*1000);
                }
            }catch
            {
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
                            continue;
                        }
                    }
                }
            }
            catch
            {
                await StateSyncing();
            }
        }
    }
}
