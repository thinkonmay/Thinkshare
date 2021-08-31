using Conductor.Data;
using Conductor.Interfaces;
using System;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using SignalRChat.Hubs;
using System.Linq;
using SharedHost.Models.Error;
using SharedHost.Models.Device;
using SharedHost.Models.Command;
using SharedHost.Models.Session;

namespace Conductor.Administration
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


        /// <summary>
        /// report new slave available to admin and save change in database
        /// </summary>
        public async Task ReportSlaveRegistered(SlaveDeviceInformation information)
        {
            await _adminHubctx.Clients.All.ReportSlaveRegistered(information);

            Slave device = new Slave(information);
            if(_db.Devices.Find(information.ID) != null) {return;}
            _db.Devices.Add(device);
            await _db.SaveChangesAsync();
        }

        public async Task ReportShellSessionTerminated(int SlaveID, int ProcessID)
        {
            var slave = _db.Devices.Find(SlaveID);
            var session = _db.ShellSession.Where(o =>
                o.Device == slave &&
                o.ProcessID == ProcessID &&
                !o.EndTime.HasValue).FirstOrDefault();

            session.EndTime = DateTime.Now;
            await _db.SaveChangesAsync();
        }


        public async Task LogSlaveCommandLine(ReceiveCommand command)
        {
            Slave machine = _db.Devices.Find(command.SlaveID);
            if (machine == null)
            {
                var error = new ReportedError()
                {
                    ErrorMessage = $"Slave device id {command.SlaveID} not found!",
                    Module = (int)Module.HOST_MODULE,
                    SlaveID = command.SlaveID
                };
                await ReportError(error);

                CommandLog cmdLog = new CommandLog()
                {
                    ProcessID = command.ProcessID,
                    Command = command.Command
                };
                cmdLog.Slave = machine;

                _db.CommandLogs.Add(cmdLog);
                await _db.SaveChangesAsync();

                await _adminHubctx.Clients.All.LogSlaveCommandLine(command.SlaveID, command.ProcessID, command.Command);
                return;
            }
        }






        public async Task ReportError(ReportedError err)
        {
            var slave = _db.Devices.Find(err.SlaveID);
            var error = new GeneralError()
            {
                Machine = slave,
                Module = err.Module,
                ErrorMessage = err.ErrorMessage
            };

            _db.Errors.Add(error);
            await _db.SaveChangesAsync();

            await _adminHubctx.Clients.All.ReportAgentError(error);
        }






        /// <summary>
        /// broadcase report session terminate and generate for all user  
        /// </summary>
        public async Task ReportNewSession(RemoteSession session)
        {
            _db.RemoteSessions.Add(session);
            await _db.SaveChangesAsync();

            await _adminHubctx.Clients.All.ReportSessionStart(session.SlaveID, session.ClientID);
            await _clientHubctx.Clients.All.ReportSlaveObtained(session.SlaveID);
        }

        public async Task ReportSessionTermination(RemoteSession session)
        {
            session.EndTime = DateTime.Now;
            _db.RemoteSessions.Update(session);
            await _db.SaveChangesAsync();

            var slave = _db.Devices.Find(session.SlaveID);
            var device_infor = new SlaveDeviceInformation(slave);
            await _adminHubctx.Clients.All.ReportSessionTermination(session.SlaveID, session.ClientID);
            await _clientHubctx.Clients.All.ReportNewSlaveAvailable(device_infor);
        }







        /// <summary>
        /// Report session state change to user 
        /// </summary>
        public async Task ReportRemoteControlDisconnected(int SlaveID)
        {
            RemoteSession ses = _db.RemoteSessions.Where(s =>s.SlaveID == SlaveID  
                                             && !s.EndTime.HasValue).FirstOrDefault();
                                                
            await _clientHubctx.Clients.User(ses.ClientID.ToString()).ReportSessionDisconnected(SlaveID);
        }
        public async Task ReportRemoteControlDisconnected(RemoteSession session)
        {
            await _clientHubctx.Clients.User(session.ClientID.ToString()).ReportSessionDisconnected(session.SlaveID);
        }

        public async Task ReportRemoteControlReconnect(int SlaveID)
        {
            RemoteSession ses = _db.RemoteSessions.Where(s =>s.SlaveID == SlaveID  
                                             && !s.EndTime.HasValue).FirstOrDefault();
            await _clientHubctx.Clients.User(ses.ClientID.ToString()).ReportSessionReconnected(SlaveID);
        }
        public async Task ReportRemoteControlReconnect(RemoteSession session)
        {
            await _clientHubctx.Clients.User(session.ClientID.ToString()).ReportSessionReconnected(session.SlaveID);
        }

        public async Task ReportSlaveDisconnected(int SlaveID)
        {
            var slave = _db.Devices.Find(SlaveID);
            var remote = _db.RemoteSessions.Where(o => o.SlaveID == SlaveID && !o.EndTime.HasValue);
            var shell = _db.ShellSession.Where(o => o.Device == slave && !o.EndTime.HasValue);

            foreach (var i in shell)
            {
                i.EndTime = DateTime.Now;
            }
            foreach (var i in remote)
            {
                i.EndTime = DateTime.Now;
            }
            await _db.SaveChangesAsync();
        }
    }
}
