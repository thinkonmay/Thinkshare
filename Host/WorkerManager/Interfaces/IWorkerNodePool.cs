using System.Threading.Tasks;
using System.Net.WebSockets;

namespace WorkerManager.Interfaces
{
    public interface IWorkerNodePool
    {
        bool Start();
        bool Stop();

        Task HandleWorkerConnection(int ID, WebSocket ws);
        Task SendRequest(int ID, string URL, string Data);
    }
}
