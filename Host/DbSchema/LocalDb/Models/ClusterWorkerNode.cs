using Newtonsoft.Json;
using System.Threading;
using System.Collections.Generic;
using DbSchema.SystemDb;
using System.Threading.Tasks;
using System;
using SharedHost.Models.Device;
using RestSharp;
using SharedHost.Models.Shell;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SharedHost.Models.Session;
using System.Net;

namespace DbSchema.LocalDb.Models
{
    public class ClusterWorkerNode 
    {
        [Key]
        public int ID{ get; set; }

        [Required]
        public string PrivateIP { get; set; }

        [Required]
        public string CPU { get; set; }

        [Required]
        public string GPU { get; set; }

        [Required]
        public int RAMcapacity { get; set; }

        [Required]
        public string OS { get; set; }

        [Required]
        public DateTime Register { get;set; }

        [NotMapped]
        [Required]
        public int agentFailedPing {get;set;}

        [NotMapped]
        [Required]
        public int sessionFailedPing {get;set;}
        
        [NotMapped]
        [JsonIgnore]
        public RestClient _coreClient { get; set; }

        [NotMapped]
        [JsonIgnore]
        public RestClient _agentClient { get; set; }

        [JsonIgnore]
        public virtual IList<Log> Logs {get;set;}

        [JsonIgnore]
        public virtual IList<ShellSession> Shells {get;set;}

        void RestoreWorkerNode ()
        {
            _coreClient = new RestClient("http://"+PrivateIP.ToString()+":3330/cluster");
            _agentClient = new RestClient("http://"+PrivateIP.ToString()+":2220/cluster");
        }



        /*state dependent method*/
        public async Task<bool> SessionInitialize(string token)
        {
            Serilog.Log.Information("Intializing worker "+ID.ToString()+" on IP address "+PrivateIP);
            RestoreWorkerNode();
            var request = new RestRequest("Initialize")
                .AddHeader("Authorization", token);
            request.Method = Method.POST;

            var result = await _agentClient.ExecuteAsync(request);
            return (result.StatusCode == HttpStatusCode.OK);
        }

        public async Task<bool> SessionTerminate(string token)
        {
            Serilog.Log.Information("Terminating worker "+ID.ToString()+" on IP address "+PrivateIP);
            RestoreWorkerNode();
            var request = new RestRequest("Terminate")
                .AddHeader("Authorization", token);
            request.Method = Method.POST;

            var result = await _agentClient.ExecuteAsync(request);
            return (result.StatusCode == HttpStatusCode.OK);
        }

        public async Task<bool> SessionDisconnect(string token)
        {
            Serilog.Log.Information("Disconnect worker "+ID.ToString()+" on IP address "+PrivateIP);
            RestoreWorkerNode();
            var request = new RestRequest("Terminate")
                .AddHeader("Authorization", token);
            request.Method = Method.POST;

            var result = await _agentClient.ExecuteAsync(request);
            return (result.StatusCode == HttpStatusCode.OK);
        }

        public async Task<bool> SessionReconnect(string token)
        {
            Serilog.Log.Information("Reconnect worker "+ID.ToString()+" on IP address "+PrivateIP);
            RestoreWorkerNode();
            var request = new RestRequest("Initialize")
                .AddHeader("Authorization", token);
            request.Method = Method.POST;

            var result = await _agentClient.ExecuteAsync(request);
            return (result.StatusCode == HttpStatusCode.OK);
        }






        public async Task<bool> PingWorker(Module module)
        {
            using (var timeoutCancellation = new CancellationTokenSource())
            {
                var originalTask = Ping(module);
                var delayTask = Task.Delay(TimeSpan.FromMilliseconds(100));
                var completedTask = await Task.WhenAny(originalTask, delayTask);
                // Cancel timeout to stop either task:
                // - Either the original task completed, so we need to cancel the delay task.
                // - Or the timeout expired, so we need to cancel the original task.
                // Canceling will not affect a task, that is already completed.
                timeoutCancellation.Cancel();
                if (completedTask == originalTask)
                {
                    // original task completed
                    return await originalTask;
                }
                else
                {
                    // timeout
                    return false;
                }
            }
        } 

        async Task<bool> Ping(Module module)
        {
            IRestResponse result;
            var request = new RestRequest("ping");
            request.Method = Method.POST;

            RestoreWorkerNode();
            if(module == Module.CORE_MODULE)
            {
                result = await _coreClient.ExecuteAsync(request);
            }
            else if (module == Module.AGENT_MODULE)
            {
                result = await _agentClient.ExecuteAsync(request);
            }
            else
            {
                return false;
            }

            if(result.StatusCode == HttpStatusCode.OK) { 
                return true; 
            } else { 
                return false; 
            }
        }





        public async Task GetWorkerMetric(ClusterDbContext db, List<ScriptModel> models)
        {
            foreach (var model in models)
            {
                var request = new RestRequest("Shell");
                request.AddParameter("application/json", model.Script, ParameterType.RequestBody);

                request.Method = Method.POST;
                RestoreWorkerNode();
                var result = await _agentClient.ExecuteAsync(request);
                if(result.StatusCode == HttpStatusCode.OK) 
                { 
                    var session = new ShellSession();                
                    session.Model  = model;
                    session.Output = result.Content;

                    var device = db.Devices.Find(ID);
                    device.Shells.Add(session);
                    db.Update(device);

                    await db.SaveChangesAsync();
                }
            }
        }
    }
}
