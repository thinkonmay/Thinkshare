using SharedHost.Models.AWS;
using System.Threading.Tasks;

namespace AutoScaling.Interfaces
{
    public interface IEC2Service
    {
        Task<EC2Instance> LaunchInstances();
        Task<bool> EC2TerminateInstances(string ID);
        Task<ClusterInstance> SetupCoturnService();

    }
}