using SharedHost.Models.Cluster;
using System.Net.WebSockets;
using System.Threading.Tasks;
using SharedHost.Models.Device;

namespace SystemHub.Interfaces
{
    public interface IClusterSocketPool
    {
        Task AddtoPool(ClusterCredential resp, WebSocket session);

        Task SendToCluster(int ClusterID, Message message);
    }
}
