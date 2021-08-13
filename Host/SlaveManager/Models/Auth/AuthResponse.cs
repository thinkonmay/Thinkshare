using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlaveManager.Models.Auth
{
    public class AuthResponse
    {
        public int ErrorCode { get; set; }
        public string UserEmail { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public string ValidUntil { get; set; }

        public static AuthResponse GenerateSuccessful(string email, string token, DateTime expiry)
        {
            return new AuthResponse()
            {
                ErrorCode = 0,
                UserEmail = email,
                Message = "Login successful",
                Token = token,
                ValidUntil = expiry.ToString("dd/MM/yyyy HH:mm:ss"); //port to string for compability with postgresql
            };
        }

        public static AuthResponse GenerateFailure(string email, string msg, int errcode)
        {
            return new AuthResponse()
            {
                ErrorCode = errcode,
                Message = msg,
                UserEmail = email
            };
        }
    }
}
