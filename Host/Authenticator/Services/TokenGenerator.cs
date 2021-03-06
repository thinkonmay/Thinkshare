using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Authenticator.Interfaces;
using SharedHost.Models.Auth;
using SharedHost.Models.User;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using SharedHost.Models.Session;
using SharedHost.Models.Cluster;
using DbSchema.SystemDb.Data;
using SharedHost.Models.Device;

namespace Authenticator.Services
{
    public class TokenGenerator : ITokenGenerator
    {

        private readonly JwtOptions _jwt;

        private readonly UserManager<UserAccount> _userManager;

        private readonly GlobalDbContext _db;

        public TokenGenerator(IOptions<JwtOptions> options, 
                              UserManager<UserAccount> userManager,
                              GlobalDbContext db )
        {
            _db = db;
            _jwt = options.Value;
            _userManager = userManager;
        }

        public async Task<string> GenerateUserJwt(UserAccount user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwt.Key);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
                Expires = DateTime.Now.AddDays(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public Task<UserAccount?> ValidateUserToken(string token)
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

                var id = jwtToken.Claims.First(x => x.Type == "id").Value;
                var account = _userManager.FindByIdAsync(id);
                return account;
            }
            catch 
            {
                return null;
            }
        }

























        public async Task<string> GenerateSessionJwt(SessionAccession accession)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwt.Key);


            var claims = new List<Claim>();
            claims.Add(new Claim("ClientID", accession.ClientID.ToString()));
            claims.Add(new Claim("WorkerID", accession.WorkerID.ToString()));
            claims.Add(new Claim("Module",   ((int) accession.Module).ToString()));
            claims.Add(new Claim("id",       accession.ID.ToString()));




            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(),
                Expires = DateTime.Now.AddDays(3),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Claims = claims.ToDictionary(k => k.Type, v => (object)v.Value)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<SessionAccession> ValidateSessionToken(string token)
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

                var accession = new SessionAccession
                {
                    ID =       Int32.Parse(jwtToken.Claims.First(x => x.Type ==         "id").Value),
                    ClientID = Int32.Parse(jwtToken.Claims.First(x => x.Type ==         "ClientID").Value),
                    WorkerID = Int32.Parse(jwtToken.Claims.First(x => x.Type ==         "WorkerID").Value),
                    Module = (Module)Int32.Parse(jwtToken.Claims.First(x => x.Type ==   "Module").Value),

                };
                return accession;
            }
            catch
            {
                return null;
            }
        }
























        public async Task<string> GenerateClusterJwt(GlobalCluster Cluster)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwt.Key);
            var claims = new List<Claim>();

            claims.Add(new Claim("Name", Cluster.Name.ToString()));
            claims.Add(new Claim("Owner", Cluster.OwnerID.ToString()));
            claims.Add(new Claim("ID", Cluster.ID.ToString()));




            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(),
                Expires = DateTime.Now.AddDays(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Claims = claims.ToDictionary(k => k.Type, v => (object)v.Value)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<ClusterCredential?> ValidateClusterToken(string token)
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
                    ValidateAudience = false
                }, out SecurityToken validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;


                var ID    = jwtToken.Claims.First(x => x.Type == "ID").Value;
                var Name  = jwtToken.Claims.First(x => x.Type == "Name").Value;
                var Owner = jwtToken.Claims.First(x => x.Type == "Owner").Value;


                var ret = new ClusterCredential
                {
                    ID = Int32.Parse(ID),
                    OwnerID = Int32.Parse(Owner), 
                    ClusterName = Name,
                };
                return ret;
            }
            catch
            {
                return null;
            }
        }
    }
}
