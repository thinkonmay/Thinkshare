using SharedHost.Models;
using SlaveManager.Data;
using SlaveManager.Interfaces;
using SlaveManager.Models;
using System;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using SignalRChat.Hubs;
using System.Linq;


namespace SlaveManager.Administration
{    
    

    public class Admin : IAdmin
    {
        private readonly ApplicationDbContext _db;
        private readonly IHubContext<AdminHub, IAdminHub> _adminHubctx;
        private readonly IHubContext<ClientHub, IClientHub> _clientHubctx;

        public Admin(ApplicationDbContext db, 
                     IHubContext<AdminHub, IAdminHub> adminHub, 
                     IHubContext<ClientHub,IClientHub> clientHub)
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

            await ReportSessionCoreExit(SlaveID, null);

            await _adminHubctx.Clients.All.ReportAgentError(error);
        }

        public async Task ReportNewSession(Session session)
        {
            _db.Sessions.Add(session);
            await _db.SaveChangesAsync();

            await _adminHubctx.Clients.All.ReportSessionStart(session.SlaveID, session.ClientID);
            await _clientHubctx.Clients.All.ReportSlaveObtained(session.SlaveID);
        }

        public async Task ReportSessionTermination(Session session)
        {
            session.EndTime = DateTime.Now;
            _db.Sessions.Update(session);
            await _db.SaveChangesAsync();

            var slave = _db.Devices.Find(session.SlaveID);
            var device_infor = new SlaveDeviceInformation(slave);
            await _adminHubctx.Clients.All.ReportSessionTermination(session.SlaveID, session.ClientID);
            await _clientHubctx.Clients.All.ReportNewSlaveAvailable(device_infor);
        }

        public async Task ReportRemoteControlDisconnected(int SlaveID)
        {            
            Session ses = _db.Sessions.Where(s =>s.SlaveID == SlaveID  
                                             && !s.EndTime.HasValue).FirstOrDefault();
                                                
            await _clientHubctx.Clients.User(ses.ClientID.ToString()).ReportSessionDisconnected(SlaveID);
        }
        public async Task ReportRemoteControlDisconnected(Session session)
        {
            await _clientHubctx.Clients.User(session.ClientID.ToString()).ReportSessionDisconnected(session.SlaveID);
        }

        public async Task ReportRemoteControlReconnect(int SlaveID)
        {
            Session ses = _db.Sessions.Where(s =>s.SlaveID == SlaveID  
                                             && !s.EndTime.HasValue).FirstOrDefault();
            await _clientHubctx.Clients.User(ses.ClientID.ToString()).ReportSessionReconnected(SlaveID);
        }
        public async Task ReportRemoteControlReconnect(Session session)
        {
            await _clientHubctx.Clients.User(session.ClientID.ToString()).ReportSessionReconnected(session.SlaveID);
        }
    }
}
