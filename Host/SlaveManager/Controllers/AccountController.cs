using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SlaveManager.Interfaces;
using SlaveManager.Models.Auth;
using SlaveManager.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlaveManager.Controllers
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
                    return AuthResponse.GenerateSuccessful(model.Email, token, DateTime.UtcNow.AddDays(7));
                }
            }

            return AuthResponse.GenerateFailure(model.Email, "Login failed", -1);
        }

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
                    string token = await _tokenGenerator.GenerateJwt(u);
                    return AuthResponse.GenerateSuccessful(model.Email, token, DateTime.UtcNow.AddDays(7));
                }
            }

            return AuthResponse.GenerateFailure(model.Email, "Register failed", -1);
        }
    }
}
