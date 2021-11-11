using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Authenticator.Interfaces;
using SharedHost.Models.User;
using SharedHost.Models.Auth;
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
        private readonly SystemConfig _config;

        public TokenController(
            UserManager<UserAccount> userManager,
            SignInManager<UserAccount> signInManager,
            ITokenGenerator tokenGenerator,
            SystemConfig config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenGenerator = tokenGenerator;
            _config = config;
        }

        /// <summary>
        /// login to server with email/username and password
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
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
                    ValidatedBy = _config.Authenticator
                };

                return Ok(resp);
            }
            else
            {
                return BadRequest(); 
            }
        }

        /// <summary>
        /// login to server with email/username and password
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Grant")]
        public async Task<IActionResult> Request(ExternalLoginModel request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                user = new UserAccount 
                {
                    UserName = request.UserName,
                    Email = request.Email,
                    Avatar = request.Picture
                };

                await _userManager.CreateAsync(user);
            }

            // Add a login (i.e insert a row for the user in AspNetUserLogins table)
            await _signInManager.SignInAsync(user, isPersistent: false);

            string token = await _tokenGenerator.GenerateJwt(user);
            var resp = new AuthenticationRequest
            { 
                token = token 
            };
            return Ok(resp);
        }
    }
}