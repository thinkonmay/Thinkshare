using Newtonsoft.Json;
using System.Threading.Tasks;
using System;
using SharedHost.Models.Device;
using WorkerManager.Services;
using SharedHost;
using RestSharp;
using SharedHost.Models.Shell;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SharedHost.Models.Session;
using System.Net;

namespace WorkerManager.SlaveDevices
{
    public class ClusterWorkerNode 
    {
        [Key]
        public int PrivateID { get; set; }

        public int? GlobalID { get;set; }

        public string PrivateIP { get; set; }

        public string CPU { get; set; }

        public string GPU { get; set; }

        public int RAMcapacity { get; set; }

        public string OS { get; set; }

        [Required]
        public DateTime Register { get;set; }

        [Required]
        public string _workerState { get; set; }

        [Required]
        public string agentUrl { get; set; }

        [Required]
        public string coreUrl { get; set; }

        public string? RemoteToken { get; set; }

        public string? SignallingUrl { get; set; }

        public virtual QoE? QoE { get; set; }

        

        [NotMapped]
        public RestClient _coreClient { get; set; }

        [NotMapped]
        public RestClient _agentClient { get; set; }
        public int RAMCapacity { get; internal set; }

        public void RestoreWorkerNode ()
        {
            _coreClient = new RestClient(coreUrl);
            _agentClient = new RestClient(agentUrl);
        }



        /*state dependent method*/
        public async Task SessionInitialize()
        {
            var request = new RestRequest("Initialize");
            request.Method = Method.POST;

            var result = await _agentClient.ExecuteAsync(request);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                _workerState = WorkerState.OnSession;
            }
        }

        public async Task SessionTerminate()
        {
            var request = new RestRequest("Terminate");
            request.Method = Method.POST;

            var result = await _agentClient.ExecuteAsync(request);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                _workerState = WorkerState.Open;
            }
        }

        public async Task SessionDisconnect()
        {
            var request = new RestRequest("Disconnect");
            request.Method = Method.POST;

            var result = await _agentClient.ExecuteAsync(request);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                _workerState = WorkerState.OffRemote;
            }
        }

        public async Task SessionReconnect()
        {
            var request = new RestRequest("Reconnect");
            request.Method = Method.POST;

            var result = await _agentClient.ExecuteAsync(request);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                _workerState = WorkerState.OnSession;
            }
        }


        public async Task<string?> PingWorker(ShellSession session)
        {
            var request = new RestRequest("Shell")
                .AddJsonBody(session.Script);
            request.Method = Method.POST;

            var result = await _agentClient.ExecuteAsync(request);
            if(result.StatusCode == HttpStatusCode.OK)
            {
                return result.Content;
            }
            else
            {
                return null;
            }
        }
    }
}
