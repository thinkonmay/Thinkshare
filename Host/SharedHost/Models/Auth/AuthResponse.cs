using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharedHost.Models.Auth
{
    public class AuthResponse
    {
        public int ErrorCode { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public DateTime ValidUntil { get; set; }


        public static AuthResponse GenerateSuccessful(string username, string token, DateTime expiry)
        {
            return new AuthResponse()
            {
                ErrorCode = 0,
                UserName = username,
                Message = "Login successful",
                Token = token,
                ValidUntil = expiry,
            };
        }

        public static AuthResponse GenerateFailure(string username, string msg, int errcode)
        {
            return new AuthResponse()
            {
                ErrorCode = errcode,
                Message = msg,
                UserName = username
            };
        }
    }
}
