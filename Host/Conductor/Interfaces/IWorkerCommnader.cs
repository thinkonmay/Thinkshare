using System.Threading.Tasks;

namespace Conductor.Interfaces
{
    public interface IWorkerCommnader
    {
        Task SessionTerminate(int ID);

        Task SessionDisconnect(int ID);

        Task SessionReconnect(int ID);
        
        Task SessionInitialize(int ID, string token);

        Task<string> GetWorkerState(int WorkerID);
    }
}
