using SharedHost.Auth;
using SharedHost.Models.Hub;
using System.Net.WebSockets;

namespace SystemHub.Interfaces
{
    public interface IWebsocketPool
    {
        void AddtoPool(AuthenticationResponse resp, WebSocket session);
        void BroadcastClientEventById(int UserID, EventModel data);
        void BroadcastClientEvent(EventModel data);
        void BroadcastManagerEventByID(int ManagerID, EventModel data);
        void BroadcastAdminEvent(EventModel data);

    }
}
