﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Signalling.Models;
using Signalling.Interfaces;
using System.Net;
using SharedHost;
using Newtonsoft.Json;
using SharedHost.Models.Session;
using RestSharp;

namespace Signalling.Controllers
{
    [ApiController]
    [Produces("application/json")]
    public class SessionsController : ControllerBase
    {
        private readonly ISessionQueue Queue;

        private readonly RestClient Authenticator;

        public SessionsController(ISessionQueue queue,SystemConfig config)
        {
            Authenticator = new RestClient(config.Authenticator + "/Token");
            Queue = queue;
        }


        [HttpGet("Handshake")]
        public async Task<IActionResult> Get(string token)
        {
            var context = ControllerContext.HttpContext;
            if (context.WebSockets.IsWebSocketRequest)
            {
                var request = new RestRequest("Session")
                    .AddQueryParameter("token", token);
                request.Method = Method.POST;

                var result = await Authenticator.ExecuteAsync(request);
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    var accession = JsonConvert.DeserializeObject<SessionAccession>(result.Content);
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    await Queue.Handle(accession, webSocket);
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
