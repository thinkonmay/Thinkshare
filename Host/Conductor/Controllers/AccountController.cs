using MersenneTwister;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Conductor.Interfaces;
using SharedHost.Models.Auth;
using Conductor.Models.User;
using Conductor.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Conductor.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly SignInManager<UserAccount> _signInManager;
        private readonly ITokenGenerator _tokenGenerator;

        public AccountController(
            UserManager<UserAccount> userManager,
            SignInManager<UserAccount> signInManager,
            ITokenGenerator tokenGenerator)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenGenerator = tokenGenerator;
        }

        /// <summary>
        /// login to server with email/username and password
        /// </summary>
        /// <param name="model">login model</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("Login")]
        public async Task<AuthResponse> Login([FromBody] LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, true, false);
                if (result.Succeeded)
                {
                    UserAccount user = await _userManager.FindByEmailAsync(model.Email);
                    string token = await _tokenGenerator.GenerateJwt(user);
                    return AuthResponse.GenerateSuccessful(model.Email, token, DateTime.Now.AddHours(1));
                }
            }
            return AuthResponse.GenerateFailure(model.Email, "Login failed", -1);
        }

        /// <summary>
        /// register new account with server
        /// </summary>
        /// <param name="model">register model</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("Register")]
        public async Task<AuthResponse> Register([FromBody] RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new UserAccount()
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    DateOfBirth = model.DateOfBirth
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    UserAccount u = await _userManager.FindByEmailAsync(model.Email);
                    await _userManager.AddToRoleAsync(u, DataSeeder.USER);
                    string token = await _tokenGenerator.GenerateJwt(u);
                    return AuthResponse.GenerateSuccessful(model.Email, token, DateTime.Now);
                }
            }

            return AuthResponse.GenerateFailure(model.Email, "Register failed", -1);
        }

        /// <summary>
        /// add role to specific user
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="Role"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        [HttpPost("GrantRole")]
        public async Task<IActionResult> GrantRole(int UserID, string Role)
        {
            var account = await _userManager.FindByIdAsync(UserID.ToString());
            await _userManager.AddToRoleAsync(account, Role);
            return Ok();
        }



        /// <summary>
        /// get personal information of user
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("GetInfor")]
        public async Task<IActionResult> GetInfor()
        {
            int ClientId = _tokenGenerator.GetUserFromHttpRequest(User);
            var account = await _userManager.FindByIdAsync(UserID.ToString());
            return Ok(account);
        }


        /// <summary>
        /// get personal information of user
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles= "Administrator")]
        [HttpGet("GetInfor")]
        public async Task<IActionResult> GetInfor()
        {
            return(_userManager.Where(o=>_userManager.GetRoles(o).FirstOrDefault() == "User"));
        }
    }
}
