using SharedHost.Models;
using SlaveManager.Data;
using SlaveManager.Interfaces;
using SlaveManager.Models;
using System;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using SignalRChat.Hubs;
using SlaveManager.SlaveDevices.SlaveStates;

namespace SlaveManager.Administration
{    public class Admin : IAdmin
    {
        private readonly ApplicationDbContext _db;
        private readonly ISlavePool _slavePool;
        private readonly IHubContext<AdminHub, IAdminHub> _adminHubctx;

        public Admin(ApplicationDbContext db, IHubContext<AdminHub, IAdminHub> adminHub) //, ISlavePool slavePool
        {
            _db = db;
            _adminHubctx = adminHub;

        }

        public async Task ReportSlaveRegistered(SlaveDeviceInformation information)
        {
            await _adminHubctx.Clients.All.ReportSlaveRegistered(information);

            var now = DateTime.Now;
            Slave device = new Slave(information);
            _db.Devices.Add(device);
            await _db.SaveChangesAsync();

        }

        public async Task LogSlaveCommandLine(int slaveID, ReceiveCommand result)
        {
            Slave machine = _db.Devices.Find(slaveID);
            if (machine == null)
            {
                throw new InvalidOperationException($"Slave device id {slaveID} not found!");
            }

            CommandLog cmdLog = new CommandLog(machine,result);
            cmdLog.Slave = machine;

            _db.CommandLogs.Add(cmdLog);
            await _db.SaveChangesAsync();

            await _adminHubctx.Clients.All.LogSlaveCommandLine(slaveID, result.ProcessID, result.Command);
            return;
        }

        public async Task ReportSessionCoreExit(int slaveID, SessionCoreExit exit)
        {
            _db.SessionCoreExits.Add(exit);
            await _db.SaveChangesAsync();

            var slave = _slavePool.GetSlaveDevice(slaveID);
            var state = new OnSessionOffRemote();
            slave.ChangeState(state);
            _slavePool.AddSlaveDeviceWithKey(slaveID,slave);

            await _adminHubctx.Clients.All.ReportSessionCoreExit(slaveID, exit);
        }


        public async Task ReportSessionCoreError(GeneralError err)
        {
            _db.GeneralErrors.Add(err);
            await _db.SaveChangesAsync();

            await _adminHubctx.Clients.All.ReportSessionCoreError(err);        
        }

        public async Task ReportAgentError(GeneralError err)
        {
            _db.GeneralErrors.Add(err);
            await _db.SaveChangesAsync();

            await _adminHubctx.Clients.All.ReportAgentError(err);
        }

        public async Task ReportNewSession(int SlaveID, int ClientID)
        {
            await _adminHubctx.Clients.All.ReportSessionStart(SlaveID, ClientID);
        }
    }
}
