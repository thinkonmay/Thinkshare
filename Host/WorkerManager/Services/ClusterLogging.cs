
using RestSharp;
using SharedHost;
using System.Net;
using WorkerManager.Interfaces;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using WorkerManager;
using SharedHost.Models.Logging;
using System;

namespace WorkerManager.Services
{
    public class Log : ILog
    {
        private RestClient _client;

        private readonly ClusterConfig _config;

        private bool CentralizedLog;

        public Log(IOptions<ClusterConfig> config)
        {
            _config = config.Value;
            _client = new RestClient();
            Task.Run(async () =>
            {
                try
                {
                    if( (await _client.ExecuteAsync(new RestRequest("/",Method.GET))).StatusCode != HttpStatusCode.OK) 
                    {
                        CentralizedLog = false;
                        return;
                    }
                    CentralizedLog = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fail to connect to CentralizedLog: {ex.Message}");
                    Console.WriteLine($"Using default console log");
                    CentralizedLog = false;
                }
            }).Wait();
        }

        public void Information(string information)
        {
            if (CentralizedLog)
            {
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
            else
            {
                Console.WriteLine(information);
            }
        }

        public void Error(string message, Exception exception)
        {
            if (CentralizedLog)
            {
                Task.Run(async () => 
                {
                    await _client.ExecuteAsync(
                        new RestRequest($"Log/Cluster",Method.POST)
                        .AddJsonBody(new ErrorLogModel{
                            timestamp = DateTime.Now,
                            StackTrace = exception.StackTrace,
                            Message = exception.Message,
                            Log = message,
                        }));
                });
            }
            else
            {
                Console.WriteLine($"{message} : {exception.Message} at {exception.StackTrace}");
            }
        }

        public void Warning(string message)
        {
            if (CentralizedLog)
            {
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
            else
            {
                Console.WriteLine(message);
            }
        }

        public static void Fatal(string message,string source, Exception ex)
        {
            try
            {
                Task.Run(async () => 
                {
                    var result = await (new RestClient()).ExecuteAsync(
                        new RestRequest($"Log/Cluster",Method.POST)
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
            catch 
            {
                Console.WriteLine("Fail to report Fatal error");
                Console.WriteLine($"[FATAL]: {ex.Message} : {ex.StackTrace}");
            }
        }
    }
}