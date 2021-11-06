using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Authenticator.Interfaces;
using SharedHost.Models.User;
using SharedHost.Auth;
using System.Threading.Tasks;
using System.Linq;
using DbSchema.DbSeeding;
using SharedHost;
using System;

namespace Authenticator.Controllers
{
    [ApiController]
    [Route("/Token")]
    public class TokenController : ControllerBase
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly SignInManager<UserAccount> _signInManager;
        private readonly ITokenGenerator _tokenGenerator;
        private readonly SystemConfig config;

        public TokenController(
            UserManager<UserAccount> userManager,
            SignInManager<UserAccount> signInManager,
            ITokenGenerator tokenGenerator,
            SystemConfig config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenGenerator = tokenGenerator;
        }

        /// <summary>
        /// login to server with email/username and password
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Challange")]
        public async Task<IActionResult> Validate([FromBody] AuthenticationRequest request)
        {
            if (ModelState.IsValid)
            {
                var account = await _tokenGenerator.ValidateToken(request.token);
                var resp = new AuthenticationResponse
                { 
                    UserID = await _userManager.GetUserIdAsync(account),
                    IsAdmin = (await _userManager.GetRolesAsync(account)).Contains(RoleSeeding.ADMIN),
                    IsManager = (await _userManager.GetRolesAsync(account)).Contains(RoleSeeding.MOD),
                    IsUser = (await _userManager.GetRolesAsync(account)).Contains(RoleSeeding.USER),
                    ValidatedBy = config.Authenticator
                };

                return Ok(resp);
            }
            else
            {
                return BadRequest(); 
            }
        }
    }
}