﻿using SlaveManager.Interfaces;
using System;
using System.Threading.Tasks;
using SharedHost.Models.Device;
using SharedHost.Models.Error;
using RestSharp;
using System.Net;
using SharedHost;

namespace SlaveManager.Services
{


    public class ConductorSocket : IConductorSocket
    {
        private readonly RestClient _error;

        private readonly RestClient _session;

        private readonly RestClient _device;

        private readonly RestClient _shell;

        public ConductorSocket(SystemConfig config)
        {
            _error =    new RestClient(config.Conductor + "/Error");
            _session =  new RestClient(config.Conductor + "/ReportSession");
            _device =   new RestClient(config.Conductor + "/ReportDevices");
            _shell =    new RestClient(config.Conductor + "/ReportShell");
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
                await ReportError(error);
            }
        }













        public async Task ReportShellSessionTerminated(ForwardCommand command)
        {
            var request = new RestRequest("Terminated")
                    .AddQueryParameter("SlaveID", command.SlaveID.ToString())
                    .AddQueryParameter("ProcessID", command.ProcessID.ToString());
            request.Method = Method.POST;

            var reply = await _shell.ExecuteAsync(request);
            if (reply.StatusCode != HttpStatusCode.OK)
            {
                var error = new ReportedError()
                {
                    Module = (int)Module.HOST_MODULE,
                    ErrorMessage = "Unable to process request",
                    SlaveID = command.SlaveID
                };
                await ReportError(error);
            }
        }

        public async Task LogSlaveCommandLine(ReceiveCommand result)
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
                await ReportError(error);
            }
        }



















        /// <summary>
        /// Report all slave error to admin
        /// </summary>
        public async Task ReportError(ReportedError err)
        {
            /*generate rest post to signalling server*/
            var request = new RestRequest("Report")
                .AddJsonBody(err);
            request.Method = Method.POST;
            await _error.ExecuteAsync(request);
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
                await ReportError(error);
            }
        }
    }
}