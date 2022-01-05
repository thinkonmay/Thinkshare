using SharedHost.Auth;
using SharedHost.Models.Cluster;
using SharedHost.Models.Message;
using System.Net.WebSockets;
using System.Threading.Tasks;
using SharedHost.Models.Device;

namespace SystemHub.Interfaces
{
    public interface IClusterSocketPool
    {
        Task AddtoPool(ClusterCredential resp, WebSocket session);

        Task SendToNode(Message message);

        Task SendToCluster(int ClusterID, Message message);
    }
}
