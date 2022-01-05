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
    [Route("/Manager")]
    public class ManagerController : ControllerBase
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly GlobalDbContext _db;
        public ManagerController(
            UserManager<UserAccount> userManager,
            GlobalDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }



        [User]
        [HttpPost("Request")]
        public async Task<IActionResult> Request(string Description)
        {
            var UserID = HttpContext.Items["UserID"];
            var account = await _userManager.FindByIdAsync(UserID.ToString());

            Serilog.Log.Information(UserID.ToString() + " want to elevate to manager, Description: "+Description);
            await _userManager.AddToRoleAsync(account,RoleSeeding.MOD);
            return Ok();
        }
    }
}
