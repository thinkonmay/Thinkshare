using SlaveManager.Interfaces;
using System;
using System.Threading.Tasks;
using SharedHost.Models.Device;
using SharedHost.Models.Error;
using System.Collections.Generic;
using SharedHost.Models.Shell;
using RestSharp;
using System.Net;
using SharedHost;
using Newtonsoft.Json;

namespace SlaveManager.Services
{


    public class ConductorSocket : IConductorSocket
    {
        private readonly RestClient _session;

        private readonly RestClient _device;

        private readonly RestClient _shell;

        private readonly RestClient _scriptmodel;

        public ConductorSocket(SystemConfig config)
        {
            _session =  new RestClient(config.Conductor + "/ReportSession");
            _device =   new RestClient(config.Conductor + "/ReportDevices");
            _shell =    new RestClient(config.Conductor + "/ReportShell");
            _scriptmodel = new RestClient(config.Conductor + "/Shell");
        }










        /// <summary>
        /// report new slave available to admin and save change in database
        /// </summary>
        public async Task<bool> ReportSlaveRegistered(SlaveDeviceInformation information)
        {
            /*generate rest post to signalling server*/
            var request = new RestRequest("Registered")
                .AddJsonBody(information);
            request.Method = Method.POST;

            
            var reply = await _device.ExecuteAsync(request);
            if (reply.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public async Task ReportSlaveDisconnected(int SlaveID)
        {
            /*generate rest post to signalling server*/
            var request = new RestRequest("Disconnected")
                .AddQueryParameter("SlaveID",SlaveID.ToString());
            request.Method = Method.POST;


            var reply = await _device.ExecuteAsync(request);
            if (reply.StatusCode != HttpStatusCode.OK)
            {
                var error = new ReportedError()
                {
                    Module = (int)Module.HOST_MODULE,
                    ErrorMessage = "Unable to process request",
                    SlaveID = SlaveID
                };
                System.Console.WriteLine(JsonConvert.SerializeObject(error));
            }
        }











        public async Task LogShellOutput(ShellOutput result)
        {
            /*generate rest post to signalling server*/
            var request = new RestRequest("Output")
                .AddJsonBody(result);
            request.Method = Method.POST;

            var reply = await _shell.ExecuteAsync(request);
            if(reply.StatusCode != HttpStatusCode.OK)
            {
                var error = new ReportedError()
                {
                    Module = (int)Module.HOST_MODULE,
                    ErrorMessage = "Unable to process request",
                    SlaveID = result.SlaveID
                };
                System.Console.WriteLine(JsonConvert.SerializeObject(error));
            }
        }






        public async Task<List<ScriptModel>> GetDefaultModel()
        {
            var request = new RestRequest("GetModel");
            request.Method = Method.GET;

            var result = await _scriptmodel.ExecuteAsync(request);
            if(result.StatusCode != HttpStatusCode.OK)
            {
                var error = new ReportedError()
                {
                    Module = (int)Module.HOST_MODULE,
                    ErrorMessage = "Unable to process request",
                    SlaveID = 0
                };
                System.Console.WriteLine(JsonConvert.SerializeObject(error));
            }
            return JsonConvert.DeserializeObject<List<ScriptModel>>(result.Content);
        }






        /// <summary>
        /// Report session state change to user 
        /// </summary>
        public async Task ReportRemoteControlDisconnected(int SlaveID)
        {
            var request = new RestRequest("Disconnected")
                .AddQueryParameter("SlaveID",SlaveID.ToString());
            request.Method = Method.POST;

            var reply = await _session.ExecuteAsync(request);
            if (reply.StatusCode != HttpStatusCode.OK)
            {
                var error = new ReportedError()
                {
                    Module = (int)Module.HOST_MODULE,
                    ErrorMessage = "Unable to process request",
                    SlaveID = SlaveID
                };
                System.Console.WriteLine(JsonConvert.SerializeObject(error));
            }
        }
    }
}
