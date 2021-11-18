using SharedHost.Models.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedHost.Models.Device;
using RestSharp;
using Newtonsoft;
using SharedHost.Models.Shell;
using Newtonsoft.Json;
using System.Net;
using Conductor.Interfaces;
using SharedHost;
using DbSchema.SystemDb.Data;

namespace Conductor.Services
{
    public class SlaveManagerSocket : ISlaveManagerSocket
    {
        private readonly SystemConfig _config;

        private readonly RestClient _pool;

        private readonly RestClient _session;

        private readonly RestClient _shell;

        public SlaveManagerSocket(SystemConfig config)
        {
            _config = config;

            _pool =     new RestClient( "/Pool");
            _session =  new RestClient( "/Session");
            _shell =    new RestClient( "/Shell");
        }


        public async Task<bool> SearchForSlaveID(int SlaveID)
        {
            /*generate rest post to signalling server*/
            var request = new RestRequest("Query")
                .AddQueryParameter("SlaveID", SlaveID.ToString());
            request.Method = Method.GET;

            var result = await _pool.ExecuteAsync(request);
            if (result.StatusCode == HttpStatusCode.OK)
            {  return true; }
            else
            {  return false; }
        }






        public async Task<bool> DisconnectSlave(int SlaveID)
        {
            /*generate rest post to signalling server*/
            var request = new RestRequest("Disconnect")
                .AddQueryParameter("SlaveID", SlaveID.ToString());
            request.Method = Method.DELETE;

            var reply = await _pool.ExecuteAsync(request);
            if (reply.StatusCode == HttpStatusCode.OK)
            { return true; }
            else 
            { return false; }
        }

        public async Task<bool> RejectSlave(int SlaveID)
        {
            /*generate rest post to signalling server*/
            var request = new RestRequest("Reject")
                .AddQueryParameter("SlaveID", SlaveID.ToString());
            request.Method = Method.DELETE;

            var reply = await _pool.ExecuteAsync(request);
            if (reply.StatusCode == HttpStatusCode.OK)
            { return true; }
            else
            { return false; }
        }









        public async Task<bool> RemoteControlReconnect(int SlaveID)
        {
            /*generate rest post to signalling server*/
            var request = new RestRequest("Reconnect")
                .AddQueryParameter("SlaveID", SlaveID.ToString());
            request.Method = Method.POST;

            var result = await _session.ExecuteAsync(request);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> RemoteControlDisconnect(int SlaveID)
        {
            /*generate rest post to signalling server*/
            var request = new RestRequest("Disconnect")
                .AddQueryParameter("SlaveID", SlaveID.ToString());
            request.Method = Method.POST;

            var result = await _session.ExecuteAsync(request);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> SessionInitialize(int ID, string token, SessionBase session)
        {
            /*generate rest post to signalling server*/
            var request = new RestRequest("Initialize")
                .AddJsonBody(session);
            request.Method = Method.POST;

            var result = await _session.ExecuteAsync(request);
            if(result.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> SessionTerminate(int SlaveID)
        {
            /*generate rest post to signalling server*/
            var request = new RestRequest("Terminate")
                .AddQueryParameter("SlaveID", SlaveID.ToString());
            request.Method = Method.POST;

            var result = await _session.ExecuteAsync(request);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
