using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WorkerManager.Interfaces;
using SharedHost.Models.Auth;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WorkerManager.Models;


namespace WorkerManager.Services
{
    public class TokenGenerator : ITokenGenerator
    {
        private readonly JwtOptions _jwt;

        private readonly ILocalStateStore _cache; 

        public TokenGenerator(IOptions<JwtOptions> options, 
                              ILocalStateStore cache)
        {
            _jwt = options.Value;
            _cache = cache;
        }

        public async Task<string> GenerateWorkerToken(ClusterWorkerNode node)
        {
            var claims = new List<Claim>();

            claims.Add(new Claim("CPU", node.model.CPU));
            claims.Add(new Claim("GPU", node.model.GPU));
            claims.Add(new Claim("OS", node.model.OS));
            claims.Add(new Claim("RAMcapacity", node.model.RAMcapacity.ToString()));

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwt.Key);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("ID", node.ID.ToString()) }),
                Expires = DateTime.Now.AddDays(30),
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
                }, out SecurityToken validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;

                var ID = Int32.Parse(jwtToken.Claims.First(x => x.Type == "ID").Value.ToString());
                var node = await _cache.GetWorkerInfor(ID);
                return node;
            }
            catch 
            {
                return null;
            }
        }
    }
}
