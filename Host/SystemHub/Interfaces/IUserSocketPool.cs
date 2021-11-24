using SharedHost.Auth;
using SharedHost.Models.Hub;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace SystemHub.Interfaces
{
    public interface IUserSocketPool
    {
        void AddtoPool(AuthenticationResponse resp, WebSocket session);

        Task Handle(WebSocket ws);
        void SendMessage(WebSocket ws, string msg);
        Task Close(WebSocket ws);

        void BroadcastClientEvent(EventModel data);
        void BroadcastClientEventById(int iD, EventModel data);
        void BroadcastAdminEvent(EventModel data);
    }
}
