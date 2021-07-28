using System.Net.WebSockets;
using System.Threading.Tasks;

namespace SlaveManager.Interfaces
{
    public interface IWebSocketConnection : IConnection
    {
        public Task Close(WebSocket ws);
    }
}
