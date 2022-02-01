using RestSharp;
using SharedHost;
using System.Net;
using SharedHost.Models.Cluster;
using WorkerManager.Interfaces;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using SharedHost.Models.Logging;
using Newtonsoft.Json;
using System;

namespace WorkerManager.Services
{
    public class Log : ILog
    {
        private RestClient _client;
        private readonly ClusterConfig _config;
        private readonly LocalStateStore _cache;
        private GlobalCluster _infor;

        public Log(IOptions<ClusterConfig> config,
                   LocalStateStore cache)
        {
            _config = config.Value;
            _cache = cache;
            _client = new RestClient(_config.LogUrl);
        }

        async Task GetClusterInfor ()
        {
            var cluster = await _cache.GetClusterInfor();

            var request = new RestRequest(_config.ClusterInforUrl)
                .AddHeader("Authorization",cluster.ClusterToken);
            request.Method = Method.GET;

            var result = await _client.ExecuteAsync(request);
            _infor = JsonConvert.DeserializeObject<GlobalCluster>(result.Content);
        }

        public void Information(string information)
        {
            Console.WriteLine(information);
            Task.Run(async () => 
            {
                await _client.ExecuteAsync(
                    new RestRequest("Infor",Method.POST)
                    .AddJsonBody(new GenericLogModel{
                        timestamp = DateTime.Now,
                        Type = "Infor",
                        Log = information,
                        Source = _infor.Name
                    }));
            });
        }

        public void Worker(string information, string WorkerID)
        {
            Console.WriteLine(information);
            Task.Run(async () => 
            {
                await _client.ExecuteAsync(
                    new RestRequest("Worker",Method.POST)
                    .AddJsonBody(new GenericLogModel{
                        timestamp = DateTime.Now,
                        Type = "Worker",
                        Log = information,
                        Source = _infor.Name
                    }));
            });
        }

        public void Error(string message, Exception exception)
        {
            Console.WriteLine($"{message} : {exception.Message} at {exception.StackTrace}");
            Task.Run(async () => 
            {
                await _client.ExecuteAsync(
                    new RestRequest("Error",Method.POST)
                    .AddJsonBody(new ErrorLogModel{
                        timestamp = DateTime.Now,
                        StackTrace = exception.StackTrace,
                        Message = exception.Message,
                        Log = message,
                        Source = _infor.Name
                    }));
            });
        }

        public void Warning(string message)
        {
            Console.WriteLine(message);
            Task.Run(async () => 
            {
                await _client.ExecuteAsync(
                    new RestRequest("Infor",Method.POST)
                    .AddJsonBody(new GenericLogModel{
                        timestamp = DateTime.Now,
                        Type = "Warning",
                        Log = message,
                        Source = _infor.Name
                    }));
            });
        }

        public static void Fatal(string message,Exception ex)
        {
            Console.WriteLine($"[FATAL]: {ex.Message} : {ex.StackTrace}");
            Task.Run(async () => 
            {
                var result = await (new RestClient()).ExecuteAsync(
                    new RestRequest($"Fatal",Method.POST)
                        .AddJsonBody(new ErrorLogModel{
                            timestamp = DateTime.Now,
                            StackTrace = ex.StackTrace,
                            Message = ex.Message,
                            Log = message,
                        }));
                var _ = result.StatusCode != HttpStatusCode.OK ? throw new Exception() : 0;
            }).Wait();
        }
    }
}