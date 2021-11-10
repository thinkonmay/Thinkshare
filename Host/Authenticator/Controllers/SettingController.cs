
using Microsoft.AspNetCore.Authorization;
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

namespace Authenticator.Controllers
{
    [ApiController]
    [Route("/Setting")]
    public class SettingController : ControllerBase
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly ApplicationDbContext _db;
        public SettingController(
            UserManager<UserAccount> userManager,
            ApplicationDbContext db)
        {
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
            var account = await _userManager.FindByIdAsync(UserID.ToString());

            return Ok(account.DefaultSetting);
        }


        
        [User]
        [HttpPost("Set")]
        public async Task<IActionResult> SetDefaultSetting([FromBody] DeviceCap capability)
        {
            var UserID = HttpContext.Items["UserID"];
            var account = await _userManager.FindByIdAsync(UserID.ToString());

            account.DefaultSetting = capability;
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}
