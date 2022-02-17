using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using SharedHost.Models.Device;
using RestSharp;
using SharedHost.Models.Shell;
using System.Net;

namespace WorkerManager.Models
{
    public class ClusterWorkerNode 
    {
        public int ID{ get; set; }

        public WorkerRegisterModel model {get;set;}

        public int agentFailedPing {get;set;}

        /*state dependent method*/
        public async Task<bool> SessionInitialize(string token)
        {
            var request = new RestRequest(model.AgentUrl + "/Initialize")
                .AddHeader("Authorization", token);
            request.Method = Method.POST;

            var result = await (new RestClient()).ExecuteAsync(request);
            return (result.StatusCode == HttpStatusCode.OK);
        }

        public async Task<bool> SessionReconnect(string token)
        {
            var request = new RestRequest(model.AgentUrl + "/Initialize")
                .AddHeader("Authorization", token);
            request.Method = Method.POST;

            var result = await (new RestClient()).ExecuteAsync(request);
            return (result.StatusCode == HttpStatusCode.OK);
        }

        public async Task<bool> SessionTerminate(string token)
        {
            var request = new RestRequest(model.AgentUrl + "/Terminate")
                .AddHeader("Authorization", token);
            request.Method = Method.POST;

            var result = await (new RestClient()).ExecuteAsync(request);
            return (result.StatusCode == HttpStatusCode.OK);
        }

        public async Task<bool> SessionDisconnect(string token)
        {
            var request = new RestRequest(model.AgentUrl + "/Terminate")
                .AddHeader("Authorization", token);
            request.Method = Method.POST;

            var result = await (new RestClient()).ExecuteAsync(request);
            return (result.StatusCode == HttpStatusCode.OK);
        }







        public async Task<bool> PingWorker()
        {
            try
            {
                var client = new RestClient();
                client.Timeout = (int)TimeSpan.FromSeconds(1).TotalMilliseconds;
                var request = new RestRequest($"{model.AgentUrl}/ping");
                var result = await client.ExecuteAsync(request,Method.POST);
                return result.StatusCode == HttpStatusCode.OK;
            }
            catch
            {
                return false;
            }
        } 

        public async Task<IRestResponse> Execute(string script)
        {
            var client = new RestClient();
            client.Timeout = (int)TimeSpan.FromSeconds(5).TotalMilliseconds;
            return await client.ExecuteAsync(
                          new RestRequest($"{model.AgentUrl}/Shell")
                          .AddBody(script),Method.POST);
        }
    }
}
