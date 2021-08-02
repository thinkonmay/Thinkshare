using System.Net.WebSockets;
using System.Threading.Tasks;

namespace SlaveManager.Interfaces
{
    public interface IConnection
    {
        public Task<bool> KeepReceiving(WebSocket ws);
        public Task Send(WebSocket ws, string message);
    }
}
