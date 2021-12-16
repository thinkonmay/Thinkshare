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

        public string? RemoteToken { get; set; }

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

        public void RestoreWorkerNode ()
        {
            _coreClient = new RestClient("http://"+PrivateIP.ToString()+":3330/cluster");
            _agentClient = new RestClient("http://"+PrivateIP.ToString()+":2220/cluster");
        }



        /*state dependent method*/
        public async Task<bool> SessionInitialize(string token)
        {
            var request = new RestRequest("Initialize")
                .AddHeader("Authorization", token);
            request.Method = Method.POST;

            var result = await _agentClient.ExecuteAsync(request);
            return (result.StatusCode == HttpStatusCode.OK);
        }

        public async Task<bool> SessionTerminate(string token)
        {
            var request = new RestRequest("Terminate")
                .AddHeader("Authorization", token);
            request.Method = Method.POST;

            var result = await _agentClient.ExecuteAsync(request);
            return (result.StatusCode == HttpStatusCode.OK);
        }

        public async Task<bool> SessionDisconnect(string token)
        {
            var request = new RestRequest("Disconnect")
                .AddHeader("Authorization", token);
            request.Method = Method.POST;

            var result = await _agentClient.ExecuteAsync(request);
            return (result.StatusCode == HttpStatusCode.OK);
        }

        public async Task<bool> SessionReconnect(string token)
        {
            var request = new RestRequest("Reconnect")
                .AddHeader("Authorization", token);
            request.Method = Method.POST;

            var result = await _agentClient.ExecuteAsync(request);
            return (result.StatusCode == HttpStatusCode.OK);
        }




        public async Task<bool> PingSession()
        {
            var request = new RestRequest("ping");
            request.Method = Method.POST;

            var result = await _agentClient.ExecuteAsync(request);
            if(result.StatusCode == HttpStatusCode.OK) { return true; }
            else { return false; }
        }


        public async Task<bool> PingWorker(ClusterDbContext db, List<ScriptModel> model_list)
        {
            using (var timeoutCancellation = new CancellationTokenSource())
            {
                var originalTask = Ping(db,model_list);
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

        async Task<bool> Ping(ClusterDbContext db, List<ScriptModel> model_list)
        {
            var request = new RestRequest("ping");
            request.Method = Method.POST;

            var result = await _agentClient.ExecuteAsync(request);
            if(result.StatusCode == HttpStatusCode.OK) { 
                return true; 
            } else { 
                return false; 
            }

        }





        private async Task GetWorkerMetric(ClusterDbContext db, List<ScriptModel> models)
        {
            foreach (var model in models)
            {
                var session = new ShellSession { Script = model.Script, ModelID = model.ID };
                var request = new RestRequest("Shell")
                    .AddJsonBody(session.Script);
                request.Method = Method.POST;

                var result = await _agentClient.ExecuteAsync(request);
                if(result.StatusCode == HttpStatusCode.OK) 
                { 
                    session.Output = result.Content;
                    session.WorkerID = ID;
                    session.Time = DateTime.Now;
                    db.CachedSession.Add(session);
                }
            }
        }
    }
}
