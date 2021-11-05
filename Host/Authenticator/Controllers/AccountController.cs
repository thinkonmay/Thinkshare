using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Authenticator.Interfaces;
using SharedHost.Models.Auth;
using SharedHost.Models.User;
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
    [Route("/Account")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly SignInManager<UserAccount> _signInManager;
        private readonly ITokenGenerator _tokenGenerator;
        private readonly ApplicationDbContext _db;
        public AccountController(
            UserManager<UserAccount> userManager,
            SignInManager<UserAccount> signInManager,
            ITokenGenerator tokenGenerator,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenGenerator = tokenGenerator;
            _db = db;
        }

        /// <summary>
        /// login to server with email/username and password
        /// </summary>
        /// <param name="model">login model</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Login")]
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
                    var error =  new List<IdentityError> ();
                    error.Add(new IdentityError{
                        Code = "Login fail",
                        Description = "Wrong user or password" 
                    });
                    return AuthResponse.GenerateFailure(model.UserName, error);
                }
            }
            var ret =  new List<IdentityError> ();
            ret.Add(new IdentityError{
                Code = "Models Error",
                Description = "Invalid Login Models" 
            });
            return AuthResponse.GenerateFailure(model.UserName, ret);
        }


        [HttpPost]
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
                        var role_err=  new List<IdentityError> ();
                        role_err.Add(new IdentityError{
                            Code = "Wrong role",
                            Description = "You are not admin" 
                        });
                        return AuthResponse.GenerateFailure(model.UserName, role_err);
                    }
                }
                else
                {
                    var wrong =  new List<IdentityError> ();
                    wrong.Add(new IdentityError{
                        Code = "Login fail",
                        Description = "Account not exist" 
                    });
                    return AuthResponse.GenerateFailure(model.UserName, wrong);
                }
            }

            var ret =  new List<IdentityError> ();
            ret.Add(new IdentityError{
                Code = "Models Error",
                Description = "Invalid Register Models" 
            });
            return AuthResponse.GenerateFailure(model.UserName, ret);
        }

        /// <summary>
        /// register new account with server
        /// </summary>
        /// <param name="model">register model</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Register")]
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
                    await _userManager.AddToRoleAsync(u, RoleSeeding.USER);
                    string token = await _tokenGenerator.GenerateJwt(u);
                    return AuthResponse.GenerateSuccessful(model.UserName, token, DateTime.Now);
                }
                else
                {
                    return AuthResponse.GenerateFailure(model.Email,result.Errors );
                }
            }

            var ret =  new List<IdentityError> ();
            ret.Add(new IdentityError{
               Code = "Models Error",
               Description = "Invalid Register Models" 
            });
            return AuthResponse.GenerateFailure(model.Email, ret);
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



        /// <summary>
        /// get personal information of user
        /// </summary>
        /// <returns></returns>
        [User]
        [HttpGet("GetInfor")]
        public async Task<IActionResult> GetInfor()
        {
            var UserID = HttpContext.Items["UserID"];
            var account = await _userManager.FindByIdAsync(UserID.ToString());
            return Ok(account);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost("SetInfor")]
        public async Task<IActionResult> SetAccountInfor([FromBody] UserInforModel infor)
        {
            var UserID = HttpContext.Items["UserID"];
            var account = await _userManager.FindByIdAsync(UserID.ToString());

            var result = await _userManager.SetUserNameAsync(account, infor.UserName);
            if (result.Succeeded)
            {
                return Ok();
            }
            else
            {
                return BadRequest(result.Errors.ToList());
            }
        }






        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetSession")]
        public async Task<IActionResult> UserGetSession()
        {
            var UserID = HttpContext.Items["UserID"];
            //get session in recent 7 days
            var sessions = _db.RemoteSessions.Where(o => o.ClientId == Int32.Parse(UserID.ToString()) &&
                                                    o.EndTime.HasValue &&
                                                    o.StartTime.Value.AddDays(7) > DateTime.Now);

            var ret = new List<GetSessionResponse>();
            if (sessions == null)
            {
                return Ok(ret);
            }
            foreach (var item in sessions)
            {
                var i = new GetSessionResponse();
                i.DayofWeek = item.StartTime.Value.DayOfWeek;
                i.SessionTime = (item.EndTime - item.StartTime).Value.TotalMinutes;
                ret.Add(i);
            }
            return Ok(ret);
        }































































































        [AllowAnonymous]
        [HttpGet]
        [Route("Account/ExternalLogin")]
        public IActionResult ExternalLogin()
        {            
            string redirectUrl = Url.Action("signin-google");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            return new ChallengeResult("Google", properties);
        }   



        [AllowAnonymous]
        [HttpGet]
        [Route("signin-google")]
        public async Task<IActionResult>  ExternalLoginCallback()
        {
            ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();
            if (info != null)
            {
                return Redirect("https://service.thinkmay.net/login");
            }

            // If the user already has a login (i.e if there is a record in AspNetUserLogins
            // table) then sign-in the user with this external login provider
            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider,
                info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (signInResult.Succeeded)
            {
                string email = info.Principal.FindFirstValue(ClaimTypes.Email);
                UserAccount account = await _userManager.FindByEmailAsync(email);

                await _signInManager.SignInAsync(account, isPersistent: false);
                await _userManager.AddLoginAsync(account, info);
                 
                string token = await _tokenGenerator.GenerateJwt(account);
                return  Redirect($"https://service.thinkmay.net/dashboard?token={token}");
            }
            // If there is no record in AspNetUserLogins table, the user may not have
            // a local account
            else
            {
                // Get the email claim value
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);

                if (email != null)
                {
                    // Create a new user without password if we do not have a user already
                    var user = await _userManager.FindByEmailAsync(email);

                    if (user == null)
                    {
                        user = new UserAccount 
                        {
                            UserName = info.Principal.FindFirstValue(ClaimTypes.GivenName),
                            Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                        };

                        if(info.Principal.FindFirstValue(ClaimTypes.MobilePhone) != null)
                        {
                            user.PhoneNumber = info.Principal.FindFirstValue(ClaimTypes.MobilePhone);
                        }
                        if(info.Principal.FindFirstValue(ClaimTypes.DateOfBirth) != null)
                        {
                        }
                        if(info.Principal.FindFirstValue(ClaimTypes.Gender) != null)
                        {
                            user.Gender = info.Principal.FindFirstValue(ClaimTypes.Gender); 
                        }
                        if(info.Principal.FindFirstValue(ClaimTypes.Surname) != null)
                        {
                            user.FullName = 
                                $"{info.Principal.FindFirstValue(ClaimTypes.GivenName)} "+ 
                                $"{info.Principal.FindFirstValue(ClaimTypes.Surname)}";
                        }

                        await _userManager.CreateAsync(user);
                    }

                    // Add a login (i.e insert a row for the user in AspNetUserLogins table)
                    await _userManager.AddLoginAsync(user, info);
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    string token = await _tokenGenerator.GenerateJwt(user);
                    return Redirect($"https://service.thinkmay.net/dashboard?token={token}");
                }

                // If we cannot find the user email we cannot continue
                return Redirect("https://service.thinkmay.net/login");
            }
        }
    }
}
