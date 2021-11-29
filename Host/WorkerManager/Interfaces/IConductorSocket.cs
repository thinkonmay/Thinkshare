using System.Threading.Tasks;
using System.Collections.Generic;
using SharedHost.Models.Shell;

namespace WorkerManager.Interfaces
{
    public interface IConductorSocket
    {
        Task<bool> Start();

        Task<List<ScriptModel>> GetDefaultModel();

        Task WorkerStateSyncing(int WorkerID, string WorkerState);
        bool Initialized { get; set; }
    }
}
