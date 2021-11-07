using Microsoft.AspNetCore.SignalR;
using Conductor.Services;
using System.Threading.Tasks;
using SharedHost.Models.Device;
using SharedHost.Models.Shell;
using SharedHost.Auth.ThinkmayAuthProtocol;
using SharedHost.Models.Hub;
using RestSharp;
using Newtonsoft.Json;
using SharedHost;

namespace SignalRChat.Hubs
{
    public interface IAdminHub
    {
        Task ReportSlaveRegistered(SlaveDeviceInformation information);

        Task LogShellOutput(ShellOutput output);
    }

    public class AdminHub : IAdminHub
    {
        private readonly SystemConfig _config;

        private readonly RestClient _NotificationHub;

        public AdminHub(SystemConfig config)
        {
            _config = config;
            _NotificationHub = new RestClient(config.SystemHub + "/Event");
        }


        public async Task LogShellOutput(ShellOutput output)
        {
            var data = new EventModel
            {
                EventName = "LogShellOutput",
                Message = JsonConvert.SerializeObject(output)
            };

            /*generate rest post to signalling server*/
            var request = new RestRequest("Client")
                .AddJsonBody(data);
            request.Method = Method.POST;
            await _NotificationHub.ExecuteAsync(request);
        }

        public async Task ReportSlaveRegistered(SlaveDeviceInformation information)
        {
            var data = new EventModel
            {
                EventName = "LogShellOutput",
                Message = JsonConvert.SerializeObject(information)
            };

            /*generate rest post to signalling server*/
            var request = new RestRequest("Client")
                .AddJsonBody(data);
            request.Method = Method.POST;
            await _NotificationHub.ExecuteAsync(request);
        }
    }
}