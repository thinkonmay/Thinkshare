using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SharedHost.Models.User;
using SharedHost.Models.Device;
using System;
using System.Threading.Tasks;
using SharedHost.Auth;
using DbSchema.SystemDb.Data;
using DbSchema.CachedState;
using SharedHost.Logging;


namespace Conductor.Controllers
{
    [ApiController]
    [Route("/Setting")]
    public class SettingController : ControllerBase
    {
        private readonly GlobalDbContext _db;

        private readonly IGlobalStateStore _cache;

        private readonly ILog _log;

        public SettingController( IGlobalStateStore cache,
                                  ILog log,
                                  GlobalDbContext db)
        {
            _cache = cache;
            _log = log;
            _db = db;
        }



        [User]
        [HttpGet("Get")]
        public async Task<IActionResult> GetDefaultSetting()
        {
            var UserID = Int32.Parse(HttpContext.Items["UserID"].ToString());
            var result = await _cache.GetUserSetting(UserID);
            return Ok(result);
        }


        
        [User]
        [HttpPost("Set")]
        public async Task<IActionResult> SetDefaultSetting([FromBody] UserSetting body)
        {
            if (!ModelState.IsValid)
                return BadRequest("Wrong setting model");

            var UserID = Int32.Parse(HttpContext.Items["UserID"].ToString());

            var old = await _cache.GetUserSetting(UserID);
            var newSetting = UserSetting.Validate(old,body);

            _log.Information($"Set new setting: {JsonConvert.SerializeObject(newSetting)}");
            await _cache.SetUserSetting(UserID, newSetting);
            return Ok();
        }
    }
}
