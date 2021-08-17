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
        private readonly IHubContext<ClientHub, IClientHub> _clientHubctx;

        public Admin(ApplicationDbContext db, IHubContext<AdminHub, IAdminHub> adminHub, IHubContext<ClientHub,IClientHub> clientHub) //, ISlavePool slavePool
        {
            _db = db;
            _adminHubctx = adminHub;
            _clientHubctx = clientHub;
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

        public async Task ReportSessionCoreExit(int slaveID, SessionCoreExitAbsTime exit)
        {
            var slavedb = _db.Devices.Find(slaveID);
            var exit_report = new SessionCoreExit(exit, slavedb);
            _db.SessionCoreExits.Add(exit_report);
            await _db.SaveChangesAsync();

            var slave = _slavePool.GetSlaveDevice(slaveID);
            var state = new OnSessionOffRemote();
            slave.ChangeState(state);
            _slavePool.AddSlaveDeviceWithKey(slaveID,slave);

            await _adminHubctx.Clients.All.ReportSessionCoreExit(slaveID, exit_report);
        }


        public async Task ReportSessionCoreError(GeneralErrorAbsTime err, int SlaveID)
        {
            var slave = _db.Devices.Find(SlaveID);
            var error = new GeneralError(err, slave);
            _db.GeneralErrors.Add(error);
            await _db.SaveChangesAsync();

            await _adminHubctx.Clients.All.ReportSessionCoreError(error);        
        }

        public async Task ReportAgentError(GeneralErrorAbsTime err, int SlaveID)
        {
            var slave = _db.Devices.Find(SlaveID);
            var error = new GeneralError(err, slave);
            _db.GeneralErrors.Add(error);
            await _db.SaveChangesAsync();

            await _adminHubctx.Clients.All.ReportAgentError(error);
        }

        public async Task ReportNewSession(int SlaveID, int ClientID)
        {
            await _adminHubctx.Clients.All.ReportSessionStart(SlaveID, ClientID);
            await _clientHubctx.Clients.All.ReportSlaveObtained(SlaveID);
        }

        public async Task ReportSessionTermination(int SlaveID, int ClientID)
        {
            var slave = _db.Devices.Find(SlaveID);
            var device_infor = new SlaveDeviceInformation()
            {
                CPU = slave.CPU,
                GPU = slave.GPU,
                RAMcapacity = slave.RAMcapacity,
                OS = slave.OS,
                ID = slave.ID
            };
            await _adminHubctx.Clients.All.ReportSessionTermination(SlaveID, ClientID);
            await _clientHubctx.Clients.All.ReportNewSlaveAvailable(device_infor);
        }
    }
}
