﻿using DbSchema.SystemDb.Data;
using Conductor.Interfaces;
using System;
using System.Threading.Tasks;
using System.Linq;
using SharedHost.Models.Error;
using SharedHost.Models.Device;
using SharedHost.Models.Shell;
using SharedHost.Models.Session;
using SharedHost;
using Newtonsoft.Json;
using SharedHost.Models.User;
using Microsoft.AspNetCore.Identity;
using Conductor.Hubs;

namespace Conductor.Services
{
    public class Admin : IAdmin
    {
        private readonly ApplicationDbContext _db;
        private readonly IAdminHub _adminHubctx;
        private readonly IClientHub _clientHubctx;
        private readonly ISlaveManagerSocket _slavemanager;
        private readonly UserManager<UserAccount> _userManager;

        public Admin(ApplicationDbContext db, 
                     IAdminHub adminHub, 
                     IClientHub clientHub,
                     UserManager<UserAccount> userManager,
                     ISlaveManagerSocket SlaveManager,
                     SystemConfig config)
        {
            _db = db;
            _adminHubctx = adminHub;
            _clientHubctx = clientHub;
            _slavemanager = SlaveManager;
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

                information.serviceState = SlaveServiceState.Open;

                //broadcast slave register event
                Serilog.Log.Information("Broadcasting event device {slave} registered", device.ID);
                await _adminHubctx.ReportSlaveRegistered(information);
                await _clientHubctx.ReportNewSlaveAvailable(information);
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
                    information.serviceState = SlaveServiceState.Open;

                    //broadcast slave register event
                    await _adminHubctx.ReportSlaveRegistered(information);
                    await _clientHubctx.ReportNewSlaveAvailable(information);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }



        public async Task LogShellOutput(ShellOutput output)
        {
            Slave machine = _db.Devices.Find(output.SlaveID);
            if (machine == null)
            {
                var error = new ReportedError()
                {
                    ErrorMessage = $"Slave device id {output.SlaveID} not found!",
                    Module = (int)Module.HOST_MODULE,
                    SlaveID = output.SlaveID
                };
                System.Console.WriteLine(JsonConvert.SerializeObject(error));
                return;
            }
            else
            {
                var session = new ShellSession(output);
                session.Slave = _db.Devices.Find(output.SlaveID);
                session.Model = _db.ScriptModels.Find(output.ModelID);
                _db.ShellSession.Add(session);
                await _db.SaveChangesAsync();

                await _adminHubctx.LogShellOutput(output);
                return;
            }
        }





        /// <summary>
        /// broadcase report session terminate and generate for all user  
        /// </summary>
        public async Task ReportNewSession(RemoteSession session)
        {
            var device_infor = new SlaveDeviceInformation(session.Slave);
            var account = await _userManager.GetUserIdAsync(session.Client);
            
            Serilog.Log.Information("Broadcasting event slave device {slave} obtained by user {user}", session.Slave.ID, session.Client.UserName);
            
            session.Client = null;
            session.ClientId = Int32.Parse(account);

            session.SlaveID = session.Slave.ID;
            session.Slave = null;

            _db.RemoteSessions.Add(session);
            await _db.SaveChangesAsync();

            await _clientHubctx.ReportSlaveObtained(session.SlaveID);
            await _clientHubctx.ReportSessionInitialized(device_infor, session.ClientId);
        }

        public async Task ReportSessionTermination(RemoteSession session)
        {
            var account = await _userManager.GetUserIdAsync(session.Client);
            var device_infor = new SlaveDeviceInformation(session.Slave){serviceState = SlaveServiceState.Open};

            session.EndTime = DateTime.Now;
            _db.RemoteSessions.Update(session);
            await _db.SaveChangesAsync();

            Serilog.Log.Information("Broadcasting event slave device {slave} released by user {user}", session.Slave.ID, session.Client.UserName);
            await _clientHubctx.ReportSessionTerminated(session.Slave.ID,Int32.Parse(account));
            await _clientHubctx.ReportNewSlaveAvailable(device_infor);
        }







        /// <summary>
        /// Report session state change to user 
        /// </summary>
        public async Task ReportRemoteControlDisconnected(int SlaveID)
        {
            RemoteSession session = _db.RemoteSessions.Where(s =>s.Slave.ID == SlaveID  
                                             && !s.EndTime.HasValue).FirstOrDefault();

            var account = await _userManager.GetUserIdAsync(session.Client);
            Serilog.Log.Information("Broadcasting event slave device {slave} disconnected during {user} session", session.Slave.ID, session.Client.UserName);
            await _clientHubctx.ReportSessionDisconnected(SlaveID,Int32.Parse(account));
        }
        public async Task ReportRemoteControlDisconnected(RemoteSession session)
        {
            var account = await _userManager.GetUserIdAsync(session.Client);
            Serilog.Log.Information("Broadcasting event slave device {slave} disconnected during {user} session", session.Slave.ID, session.Client.UserName);
            await _clientHubctx.ReportSessionDisconnected(session.Slave.ID,Int32.Parse(account));
        }

        public async Task ReportRemoteControlReconnect(int SlaveID)
        {
            RemoteSession session = _db.RemoteSessions.Where(s =>s.Slave.ID == SlaveID  
                                             && !s.EndTime.HasValue).FirstOrDefault();

            var account = await _userManager.GetUserIdAsync(session.Client);
            Serilog.Log.Information("Broadcasting event slave device {slave} reconnected during {user} session", session.Slave.ID, session.Client.UserName);
            await _clientHubctx.ReportSessionReconnected(SlaveID,Int32.Parse(account));
        }
        public async Task ReportRemoteControlReconnect(RemoteSession session)
        {
            var account = await _userManager.GetUserIdAsync(session.Client);
            Serilog.Log.Information("Broadcasting event slave device {slave} reconnected during {user} session", session.Slave.ID, session.Client.UserName);
            await _clientHubctx.ReportSessionReconnected(session.Slave.ID,Int32.Parse(account));
        }

        
        public async Task EndAllRemoteSession(int SlaveID)
        {
            var remote = _db.RemoteSessions.Where(o => o.Slave.ID == SlaveID && !o.EndTime.HasValue).ToList();
            if(remote.Count() == 0)
            {
                await _clientHubctx.ReportSlaveObtained(SlaveID);
            }
            else
            {
                foreach (var i in remote)
                {
                    await _clientHubctx.ReportSessionTerminated(SlaveID, Int32.Parse(await _userManager.GetUserIdAsync(i.Client)));
                    i.EndTime = DateTime.Now;
                }

                await _db.SaveChangesAsync();
            }
        }


        public async Task ReportRemoteControlDisconnectedFromSignalling(SessionAccession session)
        {
            var remoteSession = _db.RemoteSessions.Find(session.ID);

            await ReportRemoteControlDisconnected(remoteSession);
            var result = await _slavemanager.GetSlaveState(remoteSession.Slave.ID);
            if(result.SlaveServiceState == "ON_SESSION")
            {
                await _slavemanager.RemoteControlDisconnect(remoteSession.Slave.ID);
                Serilog.Log.Information("Broadcasting event slave device {slave} reconnected during {user} session", remoteSession.Slave.ID, remoteSession.Client.UserName);
                await _clientHubctx.ReportSessionReconnected(remoteSession.Slave.ID,Int32.Parse(await _userManager.GetUserIdAsync(remoteSession.Client)));                                 
            }
            else
            {
                // prevent event if slavedevice is not on session
                return;
            }
        }
    }
}
