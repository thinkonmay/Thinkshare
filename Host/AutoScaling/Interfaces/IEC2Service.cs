using SharedHost.Models.AWS;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AutoScaling.Interfaces
{
    public interface IEC2Service
    {
        Task<EC2Instance> LaunchInstances();
        Task<bool> EC2TerminateInstances(string ID);
        Task<ClusterInstance> SetupClusterManager();
        Task<List<string>?> AccessEC2Instance (string IP, List<string> commands);
    }
}