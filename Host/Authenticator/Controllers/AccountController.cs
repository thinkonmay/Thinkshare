using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Authenticator.Interfaces;
using SharedHost.Models.Auth;
using SharedHost.Models.User;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DbSchema.DbSeeding;
using SharedHost.Auth;
using System.Linq;
using DbSchema.SystemDb.Data;
using SharedHost;
using SharedHost.Models.ResponseModel;
using Google.Apis.Auth;
using DbSchema.CachedState;
using SharedHost.Logging;
using Microsoft.Extensions.Options;

namespace Authenticator.Controllers
{
    [ApiController]
    [Route("/Account")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly SignInManager<UserAccount> _signInManager;
        private readonly ITokenGenerator _tokenGenerator;
        private readonly IGlobalStateStore _cache;
        private readonly GlobalDbContext _db;
        private readonly SystemConfig _config;
        private readonly ILog _log;

        public AccountController(
            UserManager<UserAccount> userManager,
            SignInManager<UserAccount> signInManager,
            IGlobalStateStore cache,
            ILog log,
            ITokenGenerator tokenGenerator,
            GlobalDbContext db,
            IOptions<SystemConfig> config)
        {
            _cache = cache;
            _log = log;
            _config = config.Value;
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenGenerator = tokenGenerator;
            _db = db;
        }

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
                    string token = await _tokenGenerator.GenerateUserJwt(user);
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

            return AuthResponse.GenerateFailure(model.UserName, 
                new IdentityError
                {
                   Code = "Models Error",
                   Description = "Invalid Register Models"
                });
        }



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
                    PhoneNumber = model.PhoneNumber,
                };
                if(model.DateOfBirth != null)
                {
                    user.DateOfBirth = model.DateOfBirth;
                }
                if(model.Jobs != null)
                {
                    user.Jobs = model.Jobs;
                }

                UserAccount userWithEmail = await _userManager.FindByEmailAsync(model.Email);
                if(userWithEmail != null)
                {
                    var errors = new List<IdentityError>();
                    errors.Add(new IdentityError{
                        Code = "Invalid email",
                        Description = "This email has been registered as an account"
                    });

                    return new AuthResponse
                    {
                        Errors = errors,
                        UserName = model.UserName,
                    };
                }

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    UserAccount u = await _userManager.FindByEmailAsync(model.Email);
                    await _userManager.AddToRoleAsync(u, RoleSeeding.USER);
                    string token = await _tokenGenerator.GenerateUserJwt(u);
                    return AuthResponse.GenerateSuccessful(model.UserName, token, DateTime.Now);
                }
                else
                {
                    return AuthResponse.GenerateFailure(model.Email,result.Errors );
                }
            }

            return AuthResponse.GenerateFailure(model.Email, 
                new IdentityError
                {
                   Code = "Models Error",
                   Description = "Invalid Register Models"
                });
        }



        [HttpPost]
        [Route("ExchangeToken")]
        public async Task<AuthResponse> Request(AuthenticationRequest request)
        {

            try
            {
                var validationSettings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new string[] { _config.GoogleOauthID }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(request.token, validationSettings);
                var user = await _userManager.FindByEmailAsync(payload.Email);
                if (user == null)
                {
                    user = new UserAccount
                    {
                        UserName = payload.Email,
                        Email = payload.Email,
                        Avatar = payload.Picture,
                        FullName = payload.Name,
                        DateOfBirth = DateTime.Now,
                        PhoneNumber = "0123456789",
                        Jobs = "DefaultJob"
                    };

                    var result = await _userManager.CreateAsync(user);
                    if (result.Succeeded)
                    {
                        UserAccount u = await _userManager.FindByEmailAsync(payload.Email);
                        await _userManager.AddToRoleAsync(u, RoleSeeding.USER);
                    }
                }

                // Add a login (i.e insert a row for the user in AspNetUserLogins table)
                await _signInManager.SignInAsync(user, isPersistent: false);

                string token = await _tokenGenerator.GenerateUserJwt(user);
                return AuthResponse.GenerateSuccessful(user.UserName,token,DateTime.Now.AddDays(30));
            }
            catch (Exception ex)
            {
                _log.Error("Error oauth login",ex);
                return AuthResponse.GenerateFailure("Unknown",new IdentityError{
                    Code = ex.Message,
                    Description  = "Unknown error"
                });
            }
        }







        [User]
        [HttpGet("Infor")]
        public async Task<IActionResult> GetInfor()
        {
            var UserID = HttpContext.Items["UserID"];
            var account = await _userManager.FindByIdAsync(UserID.ToString());
            return Ok(new  UserInforModel
            {
                UserName = account.UserName,
                FullName = account.FullName,
                Jobs = account.Jobs,
                PhoneNumber = account.PhoneNumber,
                Gender = account.Gender,
                DateOfBirth = account.DateOfBirth,
                Avatar = account.Avatar
            });
        }


        [User]
        [HttpPost("Infor")]
        public async Task<IActionResult> SetAccountInfor([FromBody] UserInforModel infor)
        {
            var UserID = HttpContext.Items["UserID"];
            var account = await _userManager.FindByIdAsync(UserID.ToString());

            if(infor.Avatar != null)
            {
                account.Avatar = infor.Avatar;
            }
            if(infor.DateOfBirth != null)
            {
                account.DateOfBirth = infor.DateOfBirth;
            }

            if(infor.FullName != null)
            {
                account.FullName = infor.FullName;
            }
            if(infor.Gender != null)
            {
                account.Gender = infor.Gender;
            }
            if(infor.Jobs != null)
            {
                account.Jobs = infor.Jobs;
            }
            if(infor.PhoneNumber != null)
            {
                account.PhoneNumber = infor.Jobs;
            }
            
            await _userManager.UpdateAsync(account);
            if(infor.UserName != null)
            {
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
            return Ok();
        }






        [User]
        [HttpGet("Roles")]
        public async Task<IActionResult> UserGetRoles()
        {
            return Ok(new 
            {
                IsAdmin   = (string)HttpContext.Items["IsAdmin"],
                IsManager = (string)HttpContext.Items["IsManager"],
                IsUser    = (string)HttpContext.Items["IsUser"],
            });
        }


        [User]
        [HttpGet("History")]
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


        [User]
        [HttpPost("Password/Update")]
        public async Task<IActionResult> UserGetRoles([FromBody] UpdatePasswordModel model)
        {
            IdentityResult result;
            var UserID = HttpContext.Items["UserID"];
            var account = await _userManager.FindByIdAsync(UserID.ToString());
            var hasPassword = await _userManager.HasPasswordAsync(account);

            if(hasPassword)
                result = await _userManager.ChangePasswordAsync(account,model.Old,model.New);
            else
                result = await _userManager.AddPasswordAsync(account,model.New);

            return Ok(result);
        }
    }
}
