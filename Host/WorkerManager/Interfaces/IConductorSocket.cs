using System.Threading.Tasks;

namespace WorkerManager.Interfaces
{
    public interface IConductorSocket
    {
        Task<bool> Start();

        bool Initialized { get; set; }
    }
}
