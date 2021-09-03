using Microsoft.AspNetCore.Mvc;
using Conductor.Interfaces;
using SharedHost.Models.Error;
using System.Threading.Tasks;
using SharedHost.Models.Command;
using SharedHost.Models.Device;
using SharedHost.Models.Auth;
using Microsoft.AspNetCore.Identity;
using Conductor.Models.User;
using System.Linq;
using Conductor.Services;
using Conductor.Data;
using System.Collections.Generic;

namespace Conductor.Controllers
{
    [Route("/System")]
    [ApiController]
    [Produces("application/json")]
    public class SystemController : ControllerBase
    {
        private readonly IAdmin _admin;

        private readonly SignInManager<UserAccount> _signInManager;

        private readonly UserManager<UserAccount> _userManager;

        private readonly ApplicationDbContext _db;

        public SystemController(IAdmin admin,
                                    ApplicationDbContext db,
                                    UserManager<UserAccount> userManager,
                                    SignInManager<UserAccount> signInManager)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
            _admin = admin;
        }

        [HttpPost("SeedDevices")]
        public async Task<IActionResult> SeedDevices([FromBody] LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, true, false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    string rolename = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
                    if(rolename == DataSeeder.ADMIN)
                    {
                        List<int> ret = new List<int>();
                        var SlaveList = _db.Devices.ToList();
                        foreach(var i in SlaveList)
                        {
                            ret.Add(i.ID);
                        }
                        return Ok(ret);
                    }
                }

            }
            return BadRequest();
        }

        [HttpPost("SeedSessions")]
        public async Task<IActionResult> SeedSessions([FromBody] LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, true, false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    string rolename = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
                    if (rolename == DataSeeder.ADMIN)
                    {
                        var currentSession = _db.RemoteSessions.Where(o => !o.EndTime.HasValue).ToList();
                        return Ok(currentSession);
                    }
                }

            }
            return BadRequest();
        }
    }
}