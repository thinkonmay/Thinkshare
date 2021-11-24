using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkerManager.Interfaces;
using System;
using System.Net;
using System.Threading.Tasks;
using WorkerManager.SlaveDevices;
using SharedHost.Models.Device;
using WorkerManager.Data;
using WorkerManager.Middleware;
using System.Linq;
using SharedHost.Models.Auth;
using RestSharp;
using Newtonsoft.Json;
using WorkerManager.Models;
using SharedHost.Models.Cluster;

namespace WorkerManager.Controllers
{
    [ApiController]
    [Route("/Owner")]
    [Produces("application/json")]
    public class OwnerController : ControllerBase
    {
        private readonly ITokenGenerator _tokenGenerator;

        private readonly ClusterDbContext _db;

        private readonly RestClient _login;

        private readonly RestClient _cluster;

        public OwnerController(IWorkerNodePool slavePool, ClusterDbContext db, ITokenGenerator token)
        {
            _db = db;
            _tokenGenerator = token;
            _login = new RestClient("https://host.thinkmay.net/Account");
            _cluster = new RestClient("https://host.thinkmay.net/Cluster");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            if(_db.Owner.Any())
            {
                return BadRequest("Owner already exist");
            }

            var request = new RestRequest("Login")
                 .AddJsonBody(login);
            request.Method = Method.POST;

            var result = await _login.ExecuteAsync(request);
            var jsonresult = JsonConvert.DeserializeObject<AuthResponse>(result.Content);
            if(jsonresult.Errors == null)
            {
                _db.Owner.Add(new OwnerCredential 
                {   Name = jsonresult.UserName, 
                    Description = null, 
                    token = jsonresult.Token 
                });
                await _db.SaveChangesAsync();
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="isPrivate"></param>
        /// <param name="TURN"></param>
        /// <returns></returns>
        [Owner]
        [HttpPost("Register")]
        public async Task<IActionResult> Register(bool isPrivate, string TURN)
        {
            var request = new RestRequest("Register")
                .AddQueryParameter("Private", isPrivate.ToString())
                .AddQueryParameter("TURN", TURN.ToString());
            request.Method = Method.POST;

            var result = await _cluster.ExecuteAsync(request);

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var cluster = new LocalCluster
                {
                    Token = result.Content,
                    Private = isPrivate,
                    TurnUrl = TURN,
                    Register = DateTime.Now
                };

                _db.Clusters.Add(cluster);
                await _db.SaveChangesAsync();
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        [Owner]
        [HttpPost("SetTURN")]
        public async Task<IActionResult> setturn(string TURN)
        {
            _db.Clusters.First().TurnUrl = TURN;
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}
