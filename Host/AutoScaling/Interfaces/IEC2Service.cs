using SharedHost.Models.AWS;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AutoScaling.Interfaces
{
    public interface IEC2Service
    {
        Task<bool> TerminateInstance(ClusterInstance instance);
        Task<ClusterInstance> SetupManagedCluster();
        Task<ClusterInstance> SetupCoturnService();
    }
}