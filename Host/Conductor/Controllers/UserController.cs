using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SharedHost.Models;
using DbSchema.SystemDb.Data;
using Conductor.Interfaces;
using Conductor.Models;
using Conductor.Services;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SharedHost.Models.User;
using SharedHost.Models.Device;
using SharedHost.Models.Session;
using SharedHost.Models.ResponseModel;

namespace Conductor.Controllers
{
    /// <summary>
    /// Routes used by user to fetch information about the system
    /// </summary>
    [Route("/User")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class UserController : Controller
    {
        private readonly UserManager<UserAccount> _userManager;

        private readonly ApplicationDbContext _db;

        private readonly ISlaveManagerSocket _slmsocket;

        private readonly ITokenGenerator _jwt;

        public UserController(ApplicationDbContext db, 
                            UserManager<UserAccount> userManager,
                            ISlaveManagerSocket slm,
                            ITokenGenerator jwt)
        {
            _slmsocket = slm;
            _db = db;
            _userManager = userManager;
            _jwt = jwt;
        }





        /// <summary>
        /// Get list of available slave device, contain device information
        /// </summary>
        /// <returns></returns>
        [HttpGet("FetchSlave")]
        public async Task<IActionResult> UserGetCurrentAvailableDevice()
        {

            List<SlaveDeviceInformation> resp = new List<SlaveDeviceInformation>();
            var stateList = await _slmsocket.GetSystemSlaveState();

            foreach (var i in stateList)
            {
                if (String.Equals(i.SlaveServiceState, SlaveServiceState.Open))
                {
                    // Add Device Information to open device Id list;
                    var slave = _db.Devices.Find(i.SlaveID);

                    var device_infor = new SlaveDeviceInformation(slave)
                    {
                        serviceState = SlaveServiceState.Open
                    };
                    resp.Add(device_infor);
                }
            }
            return Ok(resp);
        }



        /// <summary>
        /// Get list of available slave device, contain device information
        /// </summary>
        /// <returns></returns>
        [HttpGet("FetchSession")]
        public async Task<IActionResult> UserGetCurrentSesssion()
        {
            int ClientId = _jwt.GetUserFromHttpRequest(User);
            var session = _db.RemoteSessions.Where(s => s.ClientId == ClientId &&
                                                  !s.EndTime.HasValue).ToList();
            
            var ret = new List<SlaveDeviceInformation>();

            foreach (var i in session)
            {
                var device_infor = new SlaveDeviceInformation(i.Slave);

                var Query = await _slmsocket.GetSlaveState(i.Slave.ID);
                device_infor.SessionClientID = i.SessionClientID;
                device_infor.serviceState = Query.SlaveServiceState;
                ret.Add(device_infor);                
            }

            // search for remote session with client id and endtime equal null
            return Ok(ret);
        }


        [HttpGet("GetInfor")]
        public async Task<IActionResult> UserGetInfor()
        {
            int ClientId = _jwt.GetUserFromHttpRequest(User);
            var account = await _userManager.FindByIdAsync(ClientId.ToString());

            return Ok(account);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetSession")]
        public async Task<IActionResult> UserGetSession()
        {
            int ClientId = _jwt.GetUserFromHttpRequest(User);

            //get session in recent 7 days
            var sessions = _db.RemoteSessions.Where( o => o.ClientId == ClientId &&
                                                     o.EndTime.HasValue &&
                                                     o.StartTime.Value.AddDays(7) >  DateTime.Now);

            var ret = new List<GetSessionResponse>();
            if(sessions == null)
            {
                return Ok(ret);
            }
            foreach(var item in sessions)
            {
                var i = new GetSessionResponse();
                i.DayofWeek = item.StartTime.Value.DayOfWeek;
                i.SessionTime = (item.EndTime - item.StartTime).Value.TotalMinutes;
                ret.Add(i);
            }            
            return Ok(ret);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost("SetInfor")]
        public async Task<IActionResult> SetAccountInfor([FromBody]UserInforModel infor)
        {
            int ClientId = _jwt.GetUserFromHttpRequest(User);

            var account = await _userManager.FindByIdAsync(ClientId.ToString());

            var result =  await _userManager.SetUserNameAsync(account, infor.UserName);
            if(result.Succeeded)
            {
                return Ok();
            }
            else
            {
                return BadRequest(result.Errors.ToList());
            }
        }
    }
}
