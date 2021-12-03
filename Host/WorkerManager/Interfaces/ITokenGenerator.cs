using DbSchema.LocalDb.Models;
using System.Threading.Tasks;

namespace WorkerManager.Interfaces
{
    public interface ITokenGenerator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<string> GenerateWorkerToken(ClusterWorkerNode user);

        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<ClusterWorkerNode?> ValidateToken(string token);
    }
}
