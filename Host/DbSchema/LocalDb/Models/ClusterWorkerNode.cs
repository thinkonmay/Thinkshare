using Newtonsoft.Json;
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

        public void RestoreWorkerNode ()
        {
            _coreClient = new RestClient("http://"+PrivateIP.ToString()+":3330/cluster");
            _agentClient = new RestClient("http://"+PrivateIP.ToString()+":2220/cluster");
        }



        /*state dependent method*/
        public async Task<bool> SessionInitialize()
        {
            var request = new RestRequest("Initialize");
            request.Method = Method.POST;

            var result = await _agentClient.ExecuteAsync(request);
            return (result.StatusCode == HttpStatusCode.OK);
        }

        public async Task<bool> SessionTerminate()
        {
            var request = new RestRequest("Terminate");
            request.Method = Method.POST;

            var result = await _agentClient.ExecuteAsync(request);
            return (result.StatusCode == HttpStatusCode.OK);
        }

        public async Task<bool> SessionDisconnect()
        {
            var request = new RestRequest("Disconnect");
            request.Method = Method.POST;

            var result = await _agentClient.ExecuteAsync(request);
            return (result.StatusCode == HttpStatusCode.OK);
        }

        public async Task<bool> SessionReconnect()
        {
            var request = new RestRequest("Reconnect");
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
            var request = new RestRequest("ping");
            request.Method = Method.POST;

            var result = await _agentClient.ExecuteAsync(request);
            if(result.StatusCode == HttpStatusCode.OK) 
            { 
                GetWorkerMetric(db,model_list);
                return true; 
            }
            else 
            { 
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
