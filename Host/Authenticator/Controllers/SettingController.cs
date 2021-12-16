
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Authenticator.Interfaces;
using SharedHost.Models.Auth;
using SharedHost.Models.User;
using SharedHost.Models.Device;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using DbSchema.DbSeeding;
using SharedHost.Auth.ThinkmayAuthProtocol;
using System.Linq;
using DbSchema.SystemDb.Data;
using SharedHost.Models.ResponseModel;
using DbSchema.CachedState;

namespace Authenticator.Controllers
{
    [ApiController]
    [Route("/Setting")]
    public class SettingController : ControllerBase
    {
        private readonly UserManager<UserAccount> _userManager;

        private readonly GlobalDbContext _db;

        private readonly IGlobalStateStore _cache;

        public SettingController(
            UserManager<UserAccount> userManager,
            IGlobalStateStore cache,
            GlobalDbContext db)
        {
            _cache = cache;
            _userManager = userManager;
            _db = db;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [User]
        [HttpGet("Get")]
        public async Task<IActionResult> GetDefaultSetting()
        {
            var UserID = HttpContext.Items["UserID"];
            var result = await _cache.GetUserSetting(Int32.Parse((string)UserID));
            Serilog.Log.Information("User setting received with value:"+ JsonConvert.SerializeObject(result));
            return Ok(result);
        }


        
        [User]
        [HttpPost("Set")]
        public async Task<IActionResult> SetDefaultSetting([FromBody] UserSetting body)
        {
            var UserID = HttpContext.Items["UserID"];
            Serilog.Log.Information("Set new setting: "+ JsonConvert.SerializeObject(body));
            await _cache.SetUserSetting(Int32.Parse((string)UserID), body);
            return Ok();
        }
    }
}
