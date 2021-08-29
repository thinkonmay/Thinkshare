using System.Net.WebSockets;
using System.Threading.Tasks;

namespace SlaveManager.Interfaces
{
    public interface IWebSocketConnection
    {
        /// <summary>
        /// close websocket connection
        /// </summary>
        /// <param name="ws"></param>
        /// <returns></returns>
        Task Close(WebSocket ws);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ws"></param>
        /// <returns></returns>
        public Task KeepReceiving(WebSocket ws);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ws"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task Send(WebSocket ws, string message);
    }
}
