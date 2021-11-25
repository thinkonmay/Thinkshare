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
using SharedHost.Models.Session;
using SharedHost.Models.Cluster;

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
        [Route("Challenge")]
        public async Task<IActionResult> Challene([FromBody] AuthenticationRequest request)
        {
            if (ModelState.IsValid)
            {
                var account = await _tokenGenerator.ValidateUserToken(request.token);
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
        [Route("GrantSession")]
        public async Task<IActionResult> SessionGrant(SessionAccession access)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _tokenGenerator.GenerateSessionJwt(access));
            }
            else
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// login to server with email/username and password
        /// </summary>
        /// <param name="token"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ChallengeSession")]
        public async Task<IActionResult> SessionChallenge(string token)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _tokenGenerator.ValidateSessionToken(token));
            }
            else
            {
                return BadRequest();
            }
        }



        /// <summary>
        /// login to server with email/username and password
        /// </summary>
        /// <param name="token"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ChallengeCluster")]
        public async Task<IActionResult> ClusterChallange(string token)
        {
            return Ok(await _tokenGenerator.ValidateClusterToken(token));
        }
    }
}