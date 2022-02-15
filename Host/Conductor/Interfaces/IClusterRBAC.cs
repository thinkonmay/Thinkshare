using System.Threading.Tasks;
using SharedHost.Models.Cluster;
using System.Collections.Generic;
using SharedHost.Models.Device;

namespace Conductor.Interfaces
{
    public interface IClusterRBAC
    {
        Task<bool> IsAllowedCluster(int UserID, int ClusterID);

        Task<bool> IsAllowedWorker(int UserID, int WorkerID);

        Task<List<WorkerNode>> AllowedWorker(int UserID);

        Task<List<GlobalCluster>> AllowedCluster(int UserID);
    }
}
