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
using Microsoft.Extensions.Options;

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
            IOptions<SystemConfig> config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenGenerator = tokenGenerator;
            _config = config.Value;
        }

        /// <summary>
        /// login to server with email/username and password
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Challenge/User")]
        public async Task<IActionResult> Challene([FromBody] AuthenticationRequest request)
        {
            if (ModelState.IsValid)
            {
                var account = await _tokenGenerator.ValidateUserToken(request.token);
                var roles = await _userManager.GetRolesAsync(account);
                var resp = new AuthenticationResponse
                { 
                    UserID = account.Id.ToString(),
                    IsAdmin = (roles).Contains(RoleSeeding.ADMIN),
                    IsManager = (roles).Contains(RoleSeeding.MOD),
                    IsUser = (roles).Contains(RoleSeeding.USER),
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
        /// <param name="token"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Challenge/Session")]
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



        [HttpPost]
        [Route("Challenge/Cluster")]
        public async Task<IActionResult> ClusterChallange(string token)
        {
            return Ok(await _tokenGenerator.ValidateClusterToken(token));
        }

        [HttpPost]
        [Route("Grant/Cluster")]
        public async Task<IActionResult> GrantCluster(string UserID, string ClusterName, int ClusterID)
        {
            return Ok(await _tokenGenerator.GenerateClusterJwt(UserID,ClusterName,ClusterID));
        }

        /// <summary>
        /// /// </summary>
        /// login to server with email/username and password
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Grant/Session")]
        public async Task<IActionResult> SessionGrant([FromBody] SessionAccession access)
        {
            if (ModelState.IsValid)
            {
                var token = await _tokenGenerator.GenerateSessionJwt(access);
                return Ok(new AuthenticationRequest
                {
                    token = token,
                    Validator = _config.Authenticator
                });
            }
            else
            {
                return BadRequest();
            }
        }
    }
}