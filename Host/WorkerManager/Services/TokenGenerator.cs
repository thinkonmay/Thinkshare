using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WorkerManager.Interfaces;
using SharedHost.Models.Auth;
using SharedHost.Models.User;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WorkerManager.SlaveDevices;
using WorkerManager.Data;

namespace WorkerManager.Services
{
    public class TokenGenerator : ITokenGenerator
    {
        private readonly ClusterDbContext _db;

        private readonly JwtOptions _jwt;

        public TokenGenerator(IOptions<JwtOptions> options, ClusterDbContext db)
        {
            _db = db;
            _jwt = options.Value;
        }

        public async Task<string> GenerateWorkerToken(ClusterWorkerNode node)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim("CPU", node.CPU));
            claims.Add(new Claim("GPU", node.GPU));
            claims.Add(new Claim("OS", node.OS));
            claims.Add(new Claim("RAMcapacity", node.RAMcapacity.ToString()));



            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwt.Key);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("ip", node.PrivateIP.ToString()) }),
                Expires = DateTime.Now.AddHours(10),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Claims = claims.ToDictionary(k => k.Type, v => (object)v.Value)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


    


        public async Task<ClusterWorkerNode?> ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwt.Key);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;

                var ip = jwtToken.Claims.First(x => x.Type == "ip").Value;
                var node = _db.Devices.Where(o => o.PrivateIP == ip).FirstOrDefault();
                return node;
            }
            catch 
            {
                return null;
            }
        }
    }
}
