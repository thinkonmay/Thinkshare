using System.Threading.Tasks;

namespace WorkerManager.Interfaces
{
    public interface IConductorSocket
    {
        Task<bool> Start();

        Task<bool> Stop();
    }
}
