using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Amazon.EC2.Model;
using AutoScaling.Interfaces;
using System;
using SharedHost.Models.AWS;
using DbSchema.SystemDb.Data;
using SharedHost.Models.Auth;
using SharedHost.Models.Cluster;
using SharedHost.Models.Device;
using SharedHost.Logging;
using Newtonsoft.Json;
using System.Net;
using RestSharp;
using System.Collections.Generic;
using System.Threading;



namespace AutoScaling.Controllers
{
    
    [Route("/Instance")]
    [Produces("application/json")]
    public class InstanceController : Controller
    {
        private readonly IEC2Service  _ec2;

        private readonly GlobalDbContext _db;
        
        private readonly ILog _log;

        public InstanceController(IEC2Service ec2,
                                  ILog log,
                                  GlobalDbContext db)
        {
            _db = db;
            _log = log;
            _ec2 = ec2;
        }


        [HttpPost("Managed")]
        public async Task<IActionResult> ManagedInstance(string region,
                                                         string name,
                                                         int OwnerID,
                                                         [FromBody] LoginModel model)
        {
            var instance = await _ec2.SetupManagedCluster(region);
            var cluster = new GlobalCluster
            {
                Name = name,
                Register = DateTime.Now,

                instance = instance,
                WorkerNode = new List<WorkerNode>(),
                
                OwnerID = OwnerID,
            };

            _db.Clusters.Add(cluster);
            await _db.SaveChangesAsync();
            _log.Information($"Attemp to login automatically to cluster {name}");
            await AutoLogin(instance.IPAdress,name,model);
            return Ok(cluster);
        }

        [HttpGet("Coturn")]
        public async Task<IActionResult> CoturnInstance(string region)
        {
            var instance = await _ec2.SetupCoturnService(region);
            _db.Instances.Add(instance);
            await _db.SaveChangesAsync();

            return Ok(instance);
        }

        async Task AutoLogin(string IP, 
                             string Name, 
                             LoginModel model)
        {
            while(true)
            {
                var result = await (new RestClient()).ExecuteAsync(
                    new RestRequest($"http://{IP}:5000/Owner/Login",Method.POST)
                        .AddQueryParameter("ClusterName", Name)
                        .AddJsonBody(model));

                if(result.StatusCode == HttpStatusCode.OK)
                {
                    var response = JsonConvert.DeserializeObject<AuthResponse>(result.Content);
                    if(response.Token  != null &&
                       response.Errors != null)
                    {
                        _log.Information($"Login to cluster automatically success");
                        return;
                    }
                }

                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
        }
    }
}
