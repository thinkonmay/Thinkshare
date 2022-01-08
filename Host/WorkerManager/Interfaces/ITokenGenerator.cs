using WorkerManager.Models;
using System.Threading.Tasks;

namespace WorkerManager.Interfaces
{
    public interface ITokenGenerator
    {
        Task<string> GenerateWorkerToken(ClusterWorkerNode user);

        Task<ClusterWorkerNode?> ValidateToken(string token);
    }
}
