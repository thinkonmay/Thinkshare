using SharedHost.Models.Auth;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;
using SlaveManager.Interfaces;
using SharedHost;

namespace SlaveManager.Controllers
{
    [Route("/Admin")]
    [ApiController]
    [Produces("application/json")]
    public class AdminController : Controller
    {

        private readonly RestClient _conductor;

        private readonly ISlavePool _slavePool;

        private readonly SystemConfig _config;

        public AdminController(SystemConfig config, ISlavePool slavePool)
        {
            _config = config;
            _slavePool = slavePool;
            _conductor = new RestClient("http://" + config.BaseUrl + ":" + config.SlaveManagerPort + "/SlaveManager");
        }


        [HttpPost("SystemStart")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var request = new RestRequest("SystemStart").
                AddJsonBody(model);
            request.Method = Method.GET;

            var result = await _conductor.ExecuteAsync(request);
            if (result.IsSuccessful)
            {
                var SlaveIDList = JsonConvert.DeserializeObject<List<int>>(result.Content);
                foreach (var i in SlaveIDList)
                {
                    _slavePool.AddSlaveId(i);
                }
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
