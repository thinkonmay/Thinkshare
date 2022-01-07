using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace SharedHost.Models.Auth
{
    public class AuthResponse
    {
        public string? UserName { get; set; }
        public List<IdentityError>? Errors {get;set;}
        public string? Token { get; set; }
        public DateTime? ValidUntil { get; set; }


        public static AuthResponse GenerateSuccessful(string? username, string? token, DateTime? expiry)
        {
            return new AuthResponse()
            {
                Errors = null, 
                UserName = username,
                Token = token,
                ValidUntil = expiry,
            };
        }

        public static AuthResponse GenerateFailure(string username, IEnumerable<IdentityError> errcode)
        {
            return new AuthResponse()
            {
                Errors = errcode.ToList(),
                UserName = username
            };
        }
    }
}
