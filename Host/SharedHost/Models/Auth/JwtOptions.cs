using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace SharedHost.Models.Auth
{
    public class JwtOptions
    {
        public string Key { get; set; }

        public int GetUserFromHttpRequest(ClaimsPrincipal User)
        {
            ClaimsPrincipal principal = User as ClaimsPrincipal;  
            if (principal != null)  
            {  
                foreach (Claim claim in principal.Claims)  
                {  
                    if(claim.Type == "id")
                    {
                        return Int32.Parse(claim.Value);
                    }
                }  
            }
            return -1;
        }

        
        public bool IsAdmin(ClaimsPrincipal User)
        {
            ClaimsPrincipal principal = User as ClaimsPrincipal;  
            IEnumerable<Claim> roleClaims = User.FindAll(ClaimTypes.Role);
            IEnumerable<string> roles = roleClaims.Select(r => r.Value);

            if(roles.Contains<string>("Administrator"))
            {
                return true;
            }
            return false;
        }
    }
}
