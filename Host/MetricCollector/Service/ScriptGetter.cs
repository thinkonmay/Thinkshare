using MetricCollector.Interface;
using System.Linq;
using System.Threading.Tasks;
using MetricCollector.Model;
using System.Collections.Generic;
using Newtonsoft.Json;
using RestSharp;
using Microsoft.Extensions.Options;
using SharedHost;
using SharedHost.Models.Shell;
using System;
using Elasticsearch.Net;
using Nest;
using Newtonsoft.Json;

namespace MetricCollector.Service
{
    public class ScriptGetter : IScriptGetter
    {
        private readonly RestClient _client;

        public ScriptGetter(IOptions<SystemConfig> config)
        {
            _client = new RestClient("http://localhost:9200");
        }

        public async Task<List<CPUDataModel>> GetCPU(int DeviceID)
        {
            var query = new {
                match = new {
                log = "Request"
                }
            };
            var must = new Object[]{query};


            var dictionary = new Dictionary<string,Object>();
            dictionary.Add("bool",new { must });

            var response = await _client.ExecuteAsync(new RestRequest("logstash*/_search",Method.Post)
                .AddJsonBody(new 
                {
                    size = 10,
                    query = dictionary
                }));
            var result = JsonConvert.DeserializeObject<ElasticSearchModel>(response.Content);

            return null;
        }

        public Task<List<GPUDataModel>> GetGPU(int DeviceID)
        {
            return null;
        }

        public Task<List<RAMDataModel>> GetRAM(int DeviceID)
        {
            return null;
        }

        public Task<List<StorageDataModel>> GetStorage(int DeviceID)
        {
            return null;
        }
    }
}
