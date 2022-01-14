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
            Serilog.Log.Information("Intializing worker "+ID.ToString());
            return (result.StatusCode == HttpStatusCode.OK);
        }

        public async Task<bool> SessionReconnect(string token)
        {
            var request = new RestRequest(model.AgentUrl + "/Initialize")
                .AddHeader("Authorization", token);
            request.Method = Method.POST;

            var result = await (new RestClient()).ExecuteAsync(request);
            Serilog.Log.Information("Reconnect worker "+ID.ToString());
            return (result.StatusCode == HttpStatusCode.OK);
        }

        public async Task<bool> SessionTerminate(string token)
        {
            var request = new RestRequest(model.AgentUrl + "/Terminate")
                .AddHeader("Authorization", token);
            request.Method = Method.POST;

            var result = await (new RestClient()).ExecuteAsync(request);
            Serilog.Log.Information("Terminating worker "+ID.ToString());
            return (result.StatusCode == HttpStatusCode.OK);
        }

        public async Task<bool> SessionDisconnect(string token)
        {
            var request = new RestRequest(model.AgentUrl + "/Terminate")
                .AddHeader("Authorization", token);
            request.Method = Method.POST;

            var result = await (new RestClient()).ExecuteAsync(request);
            Serilog.Log.Information("Disconnect worker "+ID.ToString());
            return (result.StatusCode == HttpStatusCode.OK);
        }







        public async Task<bool> PingWorker()
        {
            using (var timeoutCancellation = new CancellationTokenSource())
            {
                var originalTask = Ping();
                var delayTask = Task.Delay(TimeSpan.FromMilliseconds(1000));
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

        async Task<bool> Ping()
        {
            IRestResponse result;
            var request = new RestRequest(model.AgentUrl + "/ping");
            result = await (new RestClient()).ExecuteAsync(request,Method.GET);

            if(result.StatusCode == HttpStatusCode.OK) { 
                return true; 
            } else { 
                return false; 
            }
        }





        public async Task<List<ShellSession>?> GetWorkerMetric(List<ScriptModel> scriptModels)
        {
            var sessions = new List<ShellSession>();
            foreach (var item in scriptModels)
            {
                var request = new RestRequest(model.AgentUrl + "/Shell")
                    .AddParameter("application/json", item.Script, ParameterType.RequestBody);

                request.Method = Method.POST;
                var result = await (new RestClient()).ExecuteAsync(request);
                if(result.StatusCode == HttpStatusCode.OK) 
                { 
                    var session = new ShellSession();                
                    session.Model  = item;
                    session.Output = result.Content;

                    sessions.Add(session);
                }
            }
            return sessions;
        }
    }
}
