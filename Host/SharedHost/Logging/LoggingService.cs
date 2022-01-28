using RestSharp;
using SharedHost.Models.Logging;
using SharedHost;
using System.Net;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System;

namespace SharedHost.Logging
{
    public interface ILog
    {
        void Information(string information);
        void Error(string message, Exception exception);
        void Warning(string message);
    }

    public class Log : ILog
    {
        private RestClient _client;

        private string podName;

        private bool ElasticSearch;

        const string InforIndex = "infor";

        const string WarningIndex = "warning";

        const string ErrorIndex = "error";

        const string FatalIndex = "fatal";


        public Log(IOptions<SystemConfig> config)
        {
            _client = new RestClient(config.Value.ElasticSearch);
            podName = System.Environment.GetEnvironmentVariable("COMPUTERNAME");
            
            Task.Run(async ()=>
            {
                try
                {
                    if( (await _client.ExecuteAsync(new RestRequest("/",Method.GET))).StatusCode != HttpStatusCode.OK) 
                    {
                        ElasticSearch = false;
                        return;
                    }

                    if( (await _client.ExecuteAsync(new RestRequest(ErrorIndex,Method.GET))).StatusCode == HttpStatusCode.NotFound) 
                    {
                        var result = await _client.ExecuteAsync(new RestRequest(ErrorIndex,Method.PUT));
                    }
                    if( (await _client.ExecuteAsync(new RestRequest(InforIndex,Method.GET))).StatusCode == HttpStatusCode.NotFound)
                    {
                        var result = await _client.ExecuteAsync(new RestRequest(InforIndex,Method.PUT));
                    }
                    if( (await _client.ExecuteAsync(new RestRequest(WarningIndex,Method.GET))).StatusCode == HttpStatusCode.NotFound)
                    {
                        var result = await _client.ExecuteAsync(new RestRequest(WarningIndex,Method.PUT));
                    }
                    ElasticSearch = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fail to connect to elasticsearch: {ex.Message}");
                    Console.WriteLine($"Using default console log");
                    ElasticSearch = false;
                }
            }).Wait();
        }

        public void Information(string information)
        {
            if (ElasticSearch)
            {
                Task.Run(async () => 
                {
                    await _client.ExecuteAsync(new RestRequest($"{InforIndex}/_doc",Method.POST)
                        .AddJsonBody(new GenericLogModel{
                            timestamp = DateTime.Now,
                            Source = podName,
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
            if (ElasticSearch)
            {
                Task.Run(async () => 
                {
                    await _client.ExecuteAsync(new RestRequest($"{ErrorIndex}/_doc",Method.POST)
                        .AddJsonBody(new ErrorLogModel{
                            timestamp = DateTime.Now,
                            Source = podName,
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
            if (ElasticSearch)
            {
                Task.Run(async () => 
                {
                    await _client.ExecuteAsync(new RestRequest($"{InforIndex}/_doc",Method.POST)
                        .AddJsonBody(new GenericLogModel{
                            timestamp = DateTime.Now,
                            Source = podName,
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
            Console.WriteLine($"[FATAL]: {ex.Message} : {ex.StackTrace}");
            try
            {
                Task.Run(async () => 
                {
                    var result = await (new RestClient()).ExecuteAsync(
                        new RestRequest($"{FatalIndex}/_doc",Method.POST)
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
            }
        }
    }
}