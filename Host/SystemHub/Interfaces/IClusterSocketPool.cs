using SharedHost.Auth;
using SharedHost.Models.Cluster;
using SharedHost.Models.Hub;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace SystemHub.Interfaces
{
    public interface IClusterSocketPool
    {
        void AddtoPool(ClusterCredential resp, WebSocket session);
    }
}
