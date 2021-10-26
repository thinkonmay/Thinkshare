using MersenneTwister;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Conductor.Interfaces;
using SharedHost.Models.Auth;
using SharedHost.Models.User;
using Conductor.Services;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Conductor.Controllers
{
    [Authorize]
    [ApiController]
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
        [Route("Account/Login")]
        public async Task<AuthResponse> Login([FromBody] LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, true, false);
                if (result.Succeeded)
                {
                    UserAccount user = await _userManager.FindByNameAsync(model.UserName);
                    
                    string token = await _tokenGenerator.GenerateJwt(user);
                    return AuthResponse.GenerateSuccessful(model.UserName, token, DateTime.Now.AddHours(1));
                }
                else
                {
                    return AuthResponse.GenerateFailure(model.UserName, "Wrong username or password", -2);
                }
            }
            return AuthResponse.GenerateFailure(model.UserName, "Invalid login model", -1);
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("AdminLogin")]
        public async Task<AuthResponse> AdminLogin([FromBody] LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, true, false);
                if (result.Succeeded)
                {
                    UserAccount user = await _userManager.FindByNameAsync(model.UserName);
                    var role = await _userManager.GetRolesAsync(user);

                    // check admin role
                    if(role.Contains("Administrator"))
                    {
                        string token = await _tokenGenerator.GenerateJwt(user);
                        return AuthResponse.GenerateSuccessful(model.UserName, token, DateTime.Now.AddHours(1));
                    }
                    else
                    {
                        return AuthResponse.GenerateFailure(model.UserName, "You are not admin", -1);
                    }
                }
                else
                {
                    return AuthResponse.GenerateFailure(model.UserName, "Wrong username or password", -2);
                }
            }

            return AuthResponse.GenerateFailure(model.UserName, "Invalid login model", -1);
        }

        /// <summary>
        /// register new account with server
        /// </summary>
        /// <param name="model">register model</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("Account/Register")]
        public async Task<AuthResponse> Register([FromBody] RegisterModel model)
        {

            if (ModelState.IsValid)
            {
                var user = new UserAccount()
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FullName = model.FullName,
                    DateOfBirth = model.DateOfBirth,
                    PhoneNumber = model.PhoneNumber,
                    Jobs = model.Jobs
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    UserAccount u = await _userManager.FindByEmailAsync(model.Email);
                    await _userManager.AddToRoleAsync(u, AccountSeeder.USER);
                    string token = await _tokenGenerator.GenerateJwt(u);
                    return AuthResponse.GenerateSuccessful(model.UserName, token, DateTime.Now);
                }
                else
                {
                    List<String> error_list = new List<String>();
                    foreach(var i in result.Errors)
                    {
                        error_list.Add(i.Description);
                    } 
                    return AuthResponse.GenerateFailure(model.Email,JsonConvert.SerializeObject(error_list), -2);
                }
            }

            return AuthResponse.GenerateFailure(model.Email, "Invalid Register model", -1);
        }

        /// <summary>
        /// add role to specific user
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="Role"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        [HttpPost("Account/GrantRole")]
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
        [HttpGet("Account/GetInfor")]
        public async Task<IActionResult> GetInfor()
        {
            int UserID = _tokenGenerator.GetUserFromHttpRequest(User);
            var account = await _userManager.FindByIdAsync(UserID.ToString());
            return Ok(account);
        }



        [AllowAnonymous]
        [HttpPost]
        [HttpGet]
        [Route("Account/ExternalLogin")]
        public IActionResult ExternalLogin()
        {            
            string redirectUrl = Url.Action("signin-google");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            return new ChallengeResult("Google", properties);
        }   



        [AllowAnonymous]
        [HttpPost]
        [HttpGet]
        [Route("signin-google")]
        public async Task<IActionResult>  ExternalLoginCallback()
        {
            // ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();
            // if (info != null)
            // {
                return Redirect("https://service.thinkmay.net/login");
            // }

            // // If the user already has a login (i.e if there is a record in AspNetUserLogins
            // // table) then sign-in the user with this external login provider
            // var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider,
            //     info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            // if (signInResult.Succeeded)
            // {
            //     string email = info.Principal.FindFirstValue(ClaimTypes.Email);
            //     UserAccount account = await _userManager.FindByEmailAsync(email);

            //     await _signInManager.SignInAsync(account, isPersistent: false);
            //     await _userManager.AddLoginAsync(account, info);
                 
            //     string token = await _tokenGenerator.GenerateJwt(account);
            //     return  Redirect($"https://service.thinkmay.net/dashboard?token={token}");
            // }
            // // If there is no record in AspNetUserLogins table, the user may not have
            // // a local account
            // else
            // {
            //     // Get the email claim value
            //     var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            //     if (email != null)
            //     {
            //         // Create a new user without password if we do not have a user already
            //         var user = await _userManager.FindByEmailAsync(email);

            //         if (user == null)
            //         {
            //             user = new UserAccount 
            //             {
            //                 UserName = info.Principal.FindFirstValue(ClaimTypes.GivenName),
            //                 Email = info.Principal.FindFirstValue(ClaimTypes.Email)
            //             };

            //             if(info.Principal.FindFirstValue(ClaimTypes.MobilePhone) != null)
            //             {
            //                 user.PhoneNumber = info.Principal.FindFirstValue(ClaimTypes.MobilePhone);
            //             }
            //             if(info.Principal.FindFirstValue(ClaimTypes.DateOfBirth) != null)
            //             {
            //             }
            //             if(info.Principal.FindFirstValue(ClaimTypes.Gender) != null)
            //             {
            //                 user.Gender = info.Principal.FindFirstValue(ClaimTypes.Gender); 
            //             }
            //             if(info.Principal.FindFirstValue(ClaimTypes.Surname) != null)
            //             {
            //                 user.FullName = 
            //                     $"{info.Principal.FindFirstValue(ClaimTypes.GivenName)} "+ 
            //                     $"{info.Principal.FindFirstValue(ClaimTypes.Surname)}";
            //             }

            //             await _userManager.CreateAsync(user);
            //         }

            //         // Add a login (i.e insert a row for the user in AspNetUserLogins table)
            //         await _userManager.AddLoginAsync(user, info);
            //         await _signInManager.SignInAsync(user, isPersistent: false);

            //         string token = await _tokenGenerator.GenerateJwt(user);
            //         return Redirect($"https://service.thinkmay.net/dashboard?token={token}");
            //     }

            //     // If we cannot find the user email we cannot continue
            //     return Redirect("https://service.thinkmay.net/login");
            // }
        }
    }
}
