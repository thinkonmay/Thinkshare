using RestSharp;
using System.Threading;
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
        private string IPaddress;

        public Log(IOptions<ClusterConfig> config)
        {
            _config = config.Value;
            _client = new RestClient($"https://{_config.Domain}{_config.LogUrl}");
            GetClusterInfor();
        }

        async Task GetClusterInfor ()
        {
            var result = (
                await (new RestClient()).ExecuteAsync(
                    new RestRequest("http://icanhazip.com",Method.GET))
            ).Content.Replace("\\r\\n", "").Replace("\\n", "").Trim();
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
                        Source = "ClusterIP : " + IPaddress
                    }));
            });
        }

        public void Worker(string information, string WorkerID)
        {
            Console.WriteLine($"worker log output {information}");
            Task.Run(async () => 
            {
                await _client.ExecuteAsync(
                    new RestRequest("Worker",Method.POST)
                    .AddJsonBody(new GenericLogModel{
                        timestamp = DateTime.Now,
                        Type = "Worker",
                        Log = information,
                        Source = $"workerID : {WorkerID.ToString()}"
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
                        Source = "ClusterIP : " + IPaddress
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