using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Amazon.EC2.Model;
using AutoScaling.Interfaces;
using System;
using SharedHost.Models.AWS;
using DbSchema.SystemDb.Data;



namespace AutoScaling.Controllers
{
    
    [Route("/Instance")]
    [Produces("application/json")]
    public class InstanceController : Controller
    {
        private readonly IEC2Service  _ec2;

        private readonly GlobalDbContext _db;

        public InstanceController(IEC2Service ec2,
                                  GlobalDbContext db)
        {
            _db = db;
            _ec2 = ec2;
        }


        [HttpGet("Managed")]
        public async Task<IActionResult> ManagedInstance(string region)
        {
            var instance = await _ec2.SetupManagedCluster(region);
            _db.Instances.Add(instance);
            await _db.SaveChangesAsync();

            return Ok(instance);
        }

        [HttpGet("Coturn")]
        public async Task<IActionResult> CoturnInstance(string region)
        {
            var instance = await _ec2.SetupCoturnService(region);
            _db.Instances.Add(instance);
            await _db.SaveChangesAsync();

            return Ok(instance);
        }

        [HttpPost("Terminate")]
        public async Task<IActionResult> Cluster(int ID)
        {
            var instance = _db.Instances.Find(ID);
            instance.End = DateTime.Now;
            instance.portForwards.ForEach(x => x.End = DateTime.Now);
            _db.Update(instance);
            await _db.SaveChangesAsync();

            return Ok(await _ec2.TerminateInstance(instance));
        }
    }
}
