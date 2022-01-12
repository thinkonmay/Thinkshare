using System.Threading.Tasks;
using SharedHost.Models.Cluster;


namespace WorkerManager.Interfaces 
{
    public interface IClusterInfor
    {
        Task<bool> IsPrivate();

        Task<bool> IsSelfHost();

        Task<bool> IsRegistered();

        Task<GlobalCluster> Infor();
    }
}