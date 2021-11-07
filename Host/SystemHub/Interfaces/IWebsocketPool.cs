using SharedHost.Auth;
using SharedHost.Models.Hub;
using System.Net.WebSockets;

namespace SystemHub.Interfaces
{
    public interface IWebsocketPool
    {
        public void AddtoPool(AuthenticationResponse resp, WebSocket session);
        public void BroadcastClientEventById(int UserID, EventModel data);
        public void BroadcastClientEvent(EventModel data);
        public void BroadcastManagerEventByID(int ManagerID, EventModel data);
        public void BroadcastAdminEvent(EventModel data);

    }
}
