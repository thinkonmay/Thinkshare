using System.Net.WebSockets;
using System.Threading.Tasks;

namespace SlaveManager.Interfaces
{
    public interface IWebSocketConnection : IConnection
    {
        /// <summary>
        /// close websocket connection
        /// </summary>
        /// <param name="ws"></param>
        /// <returns></returns>
        Task Close(WebSocket ws);
    }
}
