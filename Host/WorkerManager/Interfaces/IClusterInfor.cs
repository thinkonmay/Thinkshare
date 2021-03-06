using System.Threading.Tasks;
using SharedHost.Models.Cluster;


namespace WorkerManager.Interfaces 
{
    public interface IClusterInfor
    {
        Task<bool> IsRegistered();

        Task<GlobalCluster> Infor();
    }
}