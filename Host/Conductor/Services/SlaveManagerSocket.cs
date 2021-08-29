using SharedHost.Models.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedHost.Models.Device;
using RestSharp;
using Newtonsoft;
using Newtonsoft.Json;
using System.Net;
using Conductor.Interfaces;
using SharedHost;

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
            _pool = new RestClient("http://" + _config.BaseUrl + ":" + _config.SlaveManagerPort + "/Pool");
            _session = new RestClient("http://" + _config.BaseUrl + ":" + _config.SlaveManagerPort + "/Session");
            _shell = new RestClient("http://" + _config.BaseUrl + ":" + _config.SlaveManagerPort + "/Shell");
        }



        public async Task<bool> AddSlaveId(int SlaveID)
        {
            /*generate rest post to signalling server*/
            var get_req = new RestRequest("Add")
                .AddParameter("SlaveID", SlaveID.ToString());

            var result = _pool.Post(get_req);
            if(result.IsSuccessful)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<SlaveQueryResult> GetSlaveState(int SlaveID)
        {
            /*generate rest post to signalling server*/
            var get_req = new RestRequest("Query")
                .AddParameter("SlaveID", SlaveID.ToString());

            var result = await _pool.ExecuteAsync(get_req);
            return JsonConvert.DeserializeObject<SlaveQueryResult>(result.Content);

        }

        public async Task<List<SlaveQueryResult>> GetSystemSlaveState()
        {
            /*generate rest post to signalling server*/
            var get_req = new RestRequest("SystemQuery");
            var result = await _pool.ExecuteAsync(get_req);
            return JsonConvert.DeserializeObject<List<SlaveQueryResult>>(result.Content);
        }


        public async Task<bool> SearchForSlaveID(int SlaveID)
        {
            /*generate rest post to signalling server*/
            var get_req = new RestRequest("Query")
                .AddParameter("SlaveID", SlaveID.ToString());

            var result = _pool.Get(get_req);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }






        public async Task<bool> DisconnectSlave(int SlaveID)
        {
            /*generate rest post to signalling server*/
            var get_req = new RestRequest("Disconnect")
                .AddParameter("SlaveID", SlaveID.ToString());

            _pool.Delete(get_req);
            return true;
        }

        public async Task<bool> RejectSlave(int SlaveID)
        {
            /*generate rest post to signalling server*/
            var get_req = new RestRequest("Reject")
                .AddParameter("SlaveID", SlaveID.ToString());

            _pool.Delete(get_req);
            return true;
        }





        public async Task InitializeCommandLineSession(int SlaveID, int ProcessID)
        {
            /*generate rest post to signalling server*/
            var get_req = new RestRequest("Initialize")
                .AddParameter("SlaveID", SlaveID.ToString())
                .AddParameter("ProcessID", ProcessID.ToString());

            _shell.Post(get_req);
        }

        public async Task TerminateCommandLineSession(int SlaveID, int ProcessID)
        {
            /*generate rest post to signalling server*/
            var get_req = new RestRequest("Terminate")
                .AddParameter("SlaveID", SlaveID.ToString())
                .AddParameter("ProcessID", ProcessID.ToString());

            _shell.Post(get_req);
        }

        public async Task SendCommand(ForwardCommand command)
        {
            /*generate rest post to signalling server*/
            var get_req = new RestRequest("ForwardCommand")
                .AddJsonBody(command);

            _shell.Post(get_req);
        }










        public async Task<bool> RemoteControlReconnect(int SlaveID)
        {
            /*generate rest post to signalling server*/
            var get_req = new RestRequest("Reconnect")
                .AddParameter("SlaveID", SlaveID.ToString());

            var result = _session.Post(get_req);
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
            var get_req = new RestRequest("Disconnect")
                .AddParameter("SlaveID", SlaveID.ToString());

            var result = _session.Post(get_req);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> SessionInitialize(SlaveSession session)
        {
            /*generate rest post to signalling server*/
            var get_req = new RestRequest("Initialize")
                .AddJsonBody(session);

            var result = _session.Post(get_req);
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
            var get_req = new RestRequest("Terminate")
                .AddParameter("SlaveID", SlaveID.ToString());

            var result = _session.Post(get_req);
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
