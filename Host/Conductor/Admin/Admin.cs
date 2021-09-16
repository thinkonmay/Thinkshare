﻿using Conductor.Data;
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
using RestSharp;
using SharedHost;
using Newtonsoft.Json;
using SharedHost.Models.User;
using Microsoft.AspNetCore.Identity;

namespace Conductor.Administration
{


    public class Admin : IAdmin
    {
        private readonly ApplicationDbContext _db;
        private readonly IHubContext<AdminHub, IAdminHub> _adminHubctx;
        private readonly IHubContext<ClientHub, IClientHub> _clientHubctx;
        private readonly RestClient _slavemanager;
        private readonly UserManager<UserAccount> _userManager;

        public Admin(ApplicationDbContext db, 
                     IHubContext<AdminHub, IAdminHub> adminHub, 
                     IHubContext<ClientHub,IClientHub> clientHub,
                     UserManager<UserAccount> userManager,
                     SystemConfig config)
        {
            _db = db;
            _adminHubctx = adminHub;
            _clientHubctx = clientHub;
            _slavemanager = new RestClient(config.SlaveManager + "/Session");
            _userManager = userManager;
        }


        /// <summary>
        /// report new slave available to admin and save change in database
        /// </summary>
        public async Task<bool> ReportSlaveRegistered(SlaveDeviceInformation information)
        {
            var device = _db.Devices.Find(information.ID);
            if(device == null) { return false; }
            

            if (device.CPU == null)
            {
                //setup device infomation for slave device
                device.CPU = information.CPU;
                device.GPU = information.GPU;
                device.OS = information.OS;
                device.RAMcapacity = information.RAMcapacity;
                await _db.SaveChangesAsync();
                    
                //broadcast slave register event
                await _adminHubctx.Clients.All.ReportSlaveRegistered(information);
                return true;
            }
            else
            {
                //accept device if the hw configuration match 
                if (device.CPU == information.CPU &&
                   device.GPU == information.GPU &&
                   device.OS == information.OS &&
                   device.RAMcapacity == information.RAMcapacity)
                {

                    //broadcast slave register event
                    await _adminHubctx.Clients.All.ReportSlaveRegistered(information);
                    return true;
                }
                else
                {
                    return false;
                }
            }
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
                System.Console.WriteLine(JsonConvert.SerializeObject(error));

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





        /// <summary>
        /// broadcase report session terminate and generate for all user  
        /// </summary>
        public async Task ReportNewSession(RemoteSession session)
        {
            var account = await _userManager.GetUserIdAsync(session.Client);
            
            session.Client = null;
            session.ClientId = Int32.Parse(account);

            session.SlaveID = session.Slave.ID;
            session.Slave = null;

            _db.RemoteSessions.Add(session);
            await _db.SaveChangesAsync();

            var slave = _db.Devices.Find(session.Slave.ID);
            var device_infor = new SlaveDeviceInformation(slave);
            await _adminHubctx.Clients.All.ReportSessionStart(session.Slave.ID, session.Client);
            await _clientHubctx.Clients.All.ReportSlaveObtained(session.Slave.ID);
            await _clientHubctx.Clients.Group(account).ReportSessionInitialized(device_infor);
        }

        public async Task ReportSessionTermination(RemoteSession session)
        {
            session.EndTime = DateTime.Now;
            _db.RemoteSessions.Update(session);
            await _db.SaveChangesAsync();

            var account = await _userManager.GetUserIdAsync(session.Client);

            var slave = _db.Devices.Find(session.Slave.ID);
            var device_infor = new SlaveDeviceInformation(slave);
            await _adminHubctx.Clients.All.ReportSessionTermination(session.Slave.ID, session.Client);
            await _clientHubctx.Clients.Group(account).ReportSessionTerminated(session.Slave.ID);
            await _clientHubctx.Clients.All.ReportNewSlaveAvailable(device_infor);
        }







        /// <summary>
        /// Report session state change to user 
        /// </summary>
        public async Task ReportRemoteControlDisconnected(int SlaveID)
        {
            RemoteSession session = _db.RemoteSessions.Where(s =>s.Slave.ID == SlaveID  
                                             && !s.EndTime.HasValue).FirstOrDefault();

            var account = await _userManager.GetUserIdAsync(session.Client);

            await _clientHubctx.Clients.Group(account).ReportSessionDisconnected(SlaveID);
        }
        public async Task ReportRemoteControlDisconnected(RemoteSession session)
        {
            var account = await _userManager.GetUserIdAsync(session.Client);
            await _clientHubctx.Clients.Group(account).ReportSessionDisconnected(session.Slave.ID);
        }

        public async Task ReportRemoteControlReconnect(int SlaveID)
        {
            RemoteSession session = _db.RemoteSessions.Where(s =>s.Slave.ID == SlaveID  
                                             && !s.EndTime.HasValue).FirstOrDefault();

            var account = await _userManager.GetUserIdAsync(session.Client);
            await _clientHubctx.Clients.Group(account).ReportSessionReconnected(SlaveID);
        }
        public async Task ReportRemoteControlReconnect(RemoteSession session)
        {
            var account = await _userManager.GetUserIdAsync(session.Client);
            await _clientHubctx.Clients.Group(account).ReportSessionReconnected(session.Slave.ID);
        }

        public async Task ReportSlaveDisconnected(int SlaveID)
        {
            var slave = _db.Devices.Find(SlaveID);
            var remote = _db.RemoteSessions.Where(o => o.Slave.ID == SlaveID && !o.EndTime.HasValue);
            var shell = _db.ShellSession.Where(o => o.Device == slave && !o.EndTime.HasValue);

            foreach (var i in shell)
            {
                i.EndTime = DateTime.Now;
                await _db.SaveChangesAsync();
            }
            foreach (var i in remote)
            {
                i.EndTime = DateTime.Now;
                await _db.SaveChangesAsync();
            }
        }

        public async Task ReportRemoteControlDisconnectedFromSignalling(SessionPair session)
        {
            var remoteSession = _db.RemoteSessions.Where(o => o.SessionClientID == session.SessionClientID &&
                                                         o.SessionSlaveID == session.SessionSlaveID &&
                                                        !o.EndTime.HasValue).FirstOrDefault();

            await ReportRemoteControlDisconnected(remoteSession);
            var request = new RestRequest("Disconnect")
                .AddQueryParameter("SlaveID", remoteSession.Slave.ID.ToString());

            request.Method = Method.POST;

            await _slavemanager.ExecuteAsync(request);                        
        }
    }
}
