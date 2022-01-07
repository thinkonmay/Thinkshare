using SharedHost.Auth;
using SharedHost.Models.Message;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace SystemHub.Interfaces
{
    public interface IUserSocketPool
    {
        Task AddtoPool(AuthenticationResponse resp, WebSocket session);

        Task Handle(WebSocket ws);
        Task SendMessage(WebSocket ws, string msg);
        Task Close(WebSocket ws);

        Task BroadcastClientEvent(EventModel data);
        Task BroadcastClientEventById(int iD, EventModel data);
        Task BroadcastAdminEvent(EventModel data);
    }
}
