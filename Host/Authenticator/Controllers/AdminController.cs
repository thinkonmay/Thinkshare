
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
    [Route("/Admin")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly ApplicationDbContext _db;
        public AdminController(
            UserManager<UserAccount> userManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }



        /// <summary>
        /// add role to specific user
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="Role"></param>
        /// <returns></returns>
        [Admin]
        [HttpPost("GrantRole")]
        public async Task<IActionResult> GrantRole(string UserEmail, string Role)
        {
            var account = await _userManager.FindByEmailAsync(UserEmail);
            await _userManager.AddToRoleAsync(account, Role);
            return Ok();
        }

    }
}
