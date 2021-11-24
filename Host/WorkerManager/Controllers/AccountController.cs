using SharedHost.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkerManager.Interfaces;
using System;
using System.Net;
using RestSharp;
using System.Threading.Tasks;
using WorkerManager.SlaveDevices;
using SharedHost.Models.Device;
using Newtonsoft.Json;
using WorkerManager.Data;

namespace WorkerManager.Controllers
{
    [ApiController]
    [Route("Account")]
    [Produces("application/json")]
    public class AccountController : ControllerBase
    {
        private readonly ITokenGenerator _tokenGenerator;

        private readonly ClusterDbContext _db;

        public AccountController(IWorkerNodePool slavePool, ClusterDbContext db, ITokenGenerator token)
        {
            _db = db;
            _tokenGenerator = token;
        }

        
        [HttpGet("Login")]
        public async Task<IActionResult> Login(ClusterConfig config)
        {
            var inforRequest = new RestClient(config.HostURL+"/GetInfor");

            var request = new RestRequest("GetInfor")
                .AddHeader("Authorization","Bearer "+ config.token);
            request.Method = Method.GET;

            var result = await inforRequest.ExecuteAsync(request);
            if(result.StatusCode == HttpStatusCode.OK)
            {
                var infor = JsonConvert.DeserializeObject<UserInforModel>(result.Content);
                config.ClusterOwnerUser = infor.UserName;
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
