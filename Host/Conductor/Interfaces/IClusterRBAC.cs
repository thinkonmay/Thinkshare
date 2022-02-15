using System.Threading.Tasks;
using SharedHost.Models.Cluster;
using System.Collections.Generic;
using SharedHost.Models.Device;

namespace Conductor.Interfaces
{
    public interface IClusterRBAC
    {
        bool IsAllowed(int UserID, GlobalCluster Cluster);

        bool IsAllowed(int UserID, WorkerNode Worker);

        Task<List<WorkerNode>> AllowedWorker(int UserID);

        Task<List<GlobalCluster>> AllowedCluster(int UserID);
    }
}
