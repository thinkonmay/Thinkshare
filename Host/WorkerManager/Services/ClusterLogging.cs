using RestSharp;
using SharedHost;
using System.Net;
using WorkerManager.Interfaces;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using SharedHost.Models.Logging;
using System;

namespace WorkerManager.Services
{
    public class Log : ILog
    {
        private RestClient _client;

        private readonly ClusterConfig _config;

        public Log(IOptions<ClusterConfig> config)
        {
            _config = config.Value;
        }

        public void Information(string information)
        {
            Console.WriteLine(information);
            Task.Run(async () => 
            {
                await _client.ExecuteAsync(
                    new RestRequest($"Log/Cluster",Method.POST)
                    .AddJsonBody(new GenericLogModel{
                        timestamp = DateTime.Now,
                        Type = "Infor",
                        Log = information,
                    }));
            });
        }

        public void Worker(string information)
        {
            Console.WriteLine(information);
            Task.Run(async () => 
            {
                await _client.ExecuteAsync(
                    new RestRequest($"Worker/Cluster",Method.POST)
                    .AddJsonBody(new GenericLogModel{
                        timestamp = DateTime.Now,
                        Type = "Worker",
                        Log = information,
                    }));
            });
        }

        public void Error(string message, Exception exception)
        {
            Console.WriteLine($"{message} : {exception.Message} at {exception.StackTrace}");
            Task.Run(async () => 
            {
                await _client.ExecuteAsync(
                    new RestRequest($"Error/Cluster",Method.POST)
                    .AddJsonBody(new ErrorLogModel{
                        timestamp = DateTime.Now,
                        StackTrace = exception.StackTrace,
                        Message = exception.Message,
                        Log = message,
                    }));
            });
        }

        public void Warning(string message)
        {
            Console.WriteLine(message);
            Task.Run(async () => 
            {
                await _client.ExecuteAsync(
                    new RestRequest($"Log/Cluster",Method.POST)
                    .AddJsonBody(new GenericLogModel{
                        timestamp = DateTime.Now,
                        Type = "Warning",
                        Log = message,
                    }));
            });
        }

        public static void Fatal(string message,string source, Exception ex)
        {
            Console.WriteLine($"[FATAL]: {ex.Message} : {ex.StackTrace}");
            Task.Run(async () => 
            {
                var result = await (new RestClient()).ExecuteAsync(
                    new RestRequest($"Fatal/Cluster",Method.POST)
                        .AddJsonBody(new ErrorLogModel{
                            timestamp = DateTime.Now,
                            Source = source,
                            StackTrace = ex.StackTrace,
                            Message = ex.Message,
                            Log = message,
                        }));
                var _ = result.StatusCode != HttpStatusCode.OK ? throw new Exception() : 0;
            }).Wait();
        }
    }
}