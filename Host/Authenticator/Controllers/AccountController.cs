﻿using Microsoft.AspNetCore.Authorization;
using MersenneTwister;
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
using SharedHost;
using SharedHost.Models.ResponseModel;
using Google.Apis.Auth;
using SharedHost.Auth;

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
        
        private readonly SystemConfig _config;
        public AccountController(
            UserManager<UserAccount> userManager,
            SignInManager<UserAccount> signInManager,
            ITokenGenerator tokenGenerator,
            ApplicationDbContext db,
            SystemConfig config)
        {
            _config = config;
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
                    
                    if(user.DefaultSetting == null)
                    {
                        user.DefaultSetting = new DeviceCap {
                            device = DeviceType.WEBAPP;
                            videoCodec = Codec.CODEC_H264;
                            audioCodec = Codec.OPUS_ENC;
                            mode = QoEMode.HIGH_CONST;
                            screenHeight = 1920;
                            screenWidth = 1080;                          
                        }
                        await _userManager.UpdateAsync(user);
                    }
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

            var ret =  new List<IdentityError>
            {
                new IdentityError{
                   Code = "Models Error",
                   Description = "Invalid Register Models"
            }};
            return AuthResponse.GenerateFailure(model.Email, ret);
        }



        [HttpPost]
        [Route("ExchangeToken")]
        public async Task<IActionResult> Request(AuthenticationRequest request)
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
                        UserName = Randoms.Next().ToString(),
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

                string token = await _tokenGenerator.GenerateJwt(user);
                var resp = new AuthenticationRequest
                {
                    token = token,
                    Validator = "https://host.thinkmay.net/"
                };
                return Ok(resp);
            }
            catch (Exception ex)
            {
                Serilog.Log.Debug(ex.Message);
                return BadRequest();
            }
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
            return Ok(new UserInforModel(account));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [User]
        [HttpPost("SetInfor")]
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
            if(infor.DefaultSetting != null)
            {
                if(infor.DefaultSetting.device != null){
                    account.DefaultSetting.device = infor.DefaultSetting.device;
                }                
                if(infor.DefaultSetting.audioCodec != null){
                    account.DefaultSetting.audioCodec = infor.DefaultSetting.audioCodec;
                }
                if(infor.DefaultSetting.videoCodec != null){
                    account.DefaultSetting.videoCodec = infor.DefaultSetting.videoCodec;
                }
                if(infor.DefaultSetting.mode != null){
                    account.DefaultSetting.mode = infor.DefaultSetting.mode;
                }
                if(infor.DefaultSetting.screenWidth != null){
                    account.DefaultSetting.screenWidth = infor.DefaultSetting.screenWidth;
                }
                if(infor.DefaultSetting.screenHeight != null){
                    account.DefaultSetting.screenHeight = infor.DefaultSetting.screenHeight;
                }
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









        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [User]
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
        
    }
}
