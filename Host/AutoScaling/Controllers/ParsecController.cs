using SharedHost.Models.Device;
using Microsoft.AspNetCore.Mvc;
using SharedHost.Models.AWS;
using DbSchema.SystemDb.Data;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SharedHost.Models.User;
using SharedHost.Auth.ThinkmayAuthProtocol;
using SharedHost;
using RestSharp;
using SharedHost.Models.Session;
using Newtonsoft.Json;
using System.Linq;
using AutoScaling.Interfaces;
using Microsoft.Extensions.Options;

namespace AutoScaling.Controllers
{
    /// <summary>
    /// Routes used by user to fetch information about the system
    /// </summary>
    [ApiController]
    [Route("/Parsec")]
    [Produces("application/json")]
    public class ParsecController : Controller
    {
        public ParsecController()
        {
        }


        [HttpGet]
        public async Task<IActionResult> setupParsec()
        {
            var result = await ((new RestClient()).ExecuteAsync(
                new RestRequest(ParsecAPI.authAPI,Method.POST)
                    .AddJsonBody(new ParsecLoginModel{
                        email = "huyhoangdo0205@gmail.com",
                        password = "V6ej5R-MEq_ba9f"
                    })));
            return Ok(JsonConvert.DeserializeObject<ParsecLoginResponse>(result.Content));
        }
    }
}