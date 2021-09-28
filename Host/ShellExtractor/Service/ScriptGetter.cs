using ShellExtractor.Interface;
using ShellExtractor.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using SharedHost;
using SharedHost.Models.Shell;

namespace ShellExtractor.Service
{
    public class ScriptGetter : IScriptGetter
    {
        private readonly RestClient _conductor;
        public ScriptGetter(SystemConfig config)
        {
            _conductor = new RestClient(config.Conductor + "/Shell");
        }

        public List<CPUDataModel> GetCPU(int DeviceID)
        {
            List<CPUDataModel> ret = new List<CPUDataModel>();

            var request = new RestRequest("GetModelHistory")
                .AddQueryParameter("DeviceID",DeviceID.ToString())
                .AddQueryParameter("modelID", ScriptModelEnum.GET_CPU.ToString());

            var result = _conductor.Get(request);
            if(result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = JsonConvert.DeserializeObject<List<ShellSession>>(result.Content);
                foreach(var item in content)
                {
                    var data = new CPUDataModel();
                    data.ProcessList = JsonConvert.DeserializeObject<List<CPUmetricModel>>(item.Output);
                    data.Time = item.Time;
                    ret.Add(data);                
                }
                return ret;
            }
            else
            {
                return null;
            }
        }

        public List<GPUDataModel> GetGPU(int DeviceID)
        {
            List<GPUDataModel> ret = new List<GPUDataModel>();

            var request = new RestRequest("GetModelHistory")
                .AddQueryParameter("DeviceID",DeviceID.ToString())
                .AddQueryParameter("modelID", ScriptModelEnum.GET_GPU.ToString());

            var result = _conductor.Get(request);
            if(result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = JsonConvert.DeserializeObject<List<ShellSession>>(result.Content);
                foreach(var item in content)
                {
                    var data = new GPUDataModel();
                    data.Result = JsonConvert.DeserializeObject<GPUmetricModel>(item.Output);
                    data.Time = item.Time;
                    ret.Add(data);
                }
                return ret;
            }
            else
            {
                return null;
            }
        }

        public List<RAMDataModel> GetRAM(int DeviceID)
        {
            var ret = new List<RAMDataModel>();

            var request = new RestRequest("GetModelHistory")
                .AddQueryParameter("DeviceID", DeviceID.ToString())
                .AddQueryParameter("modelID", ScriptModelEnum.GET_RAM.ToString());

            var result = _conductor.Get(request);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = JsonConvert.DeserializeObject<List<ShellSession>>(result.Content);
                foreach (var item in content)
                {
                    var data = new RAMDataModel();
                    data.ProcessList = JsonConvert.DeserializeObject<List<RAMmetricModel>>(item.Output);
                    data.Time = item.Time;
                    ret.Add(data);
                }
                return ret;
            }
            else
            {
                return null;
            }
        }

        public List<StorageDataModel> GetStorage(int DeviceID)
        {
            List<StorageDataModel> ret = new List<StorageDataModel>();

            var request = new RestRequest("GetModelHistory")
                .AddQueryParameter("DeviceID", DeviceID.ToString())
                .AddQueryParameter("modelID", ScriptModelEnum.GET_STORAGE.ToString());

            var result = _conductor.Get(request);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = JsonConvert.DeserializeObject<List<ShellSession>>(result.Content);
                foreach (var item in content)
                {
                    var data = new StorageDataModel();
                    data.StorageList = JsonConvert.DeserializeObject<List<StoragemetricModel>>(item.Output);
                    data.Time = item.Time;
                    ret.Add(data);
                }
                return ret;
            }
            else
            {
                return null;
            }
        }
    }
}
