using System.Net.WebSockets;
using System.Threading.Tasks;

namespace SystemHub.Interfaces
{
    public interface IWebSocketHandler
    {
        Task Handle(WebSocket ws);
        void SendMessage(WebSocket ws, string msg);
        Task Close(WebSocket ws);
    }
}
