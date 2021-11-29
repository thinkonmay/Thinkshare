using System.Threading.Tasks;
using SharedHost.Models.Device;

namespace WorkerManager.Interfaces
{
    public interface IWorkerNodePool
    {
        bool Start();
        bool Stop();
    }
}
