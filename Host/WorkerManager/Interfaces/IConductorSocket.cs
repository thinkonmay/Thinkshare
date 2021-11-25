using System.Threading.Tasks;
using SharedHost.Models.Device;
using SharedHost.Models.Shell;
using System.Collections.Generic;
using WorkerManager.Models;

namespace WorkerManager.Interfaces
{
    /// <summary>
    /// Admin is an singleton object responsible for handle event related to system management 
    /// , included : report slave error, report session core terminattion, report new registered slave device....
    /// and redirect those event to be  save in database or report to admin (connected to admin hub)
    /// /// </summary>
    public interface IConductorSocket
    {
        /// <summary>
        /// Report new slave registration to the system
        /// </summary>
        /// <param name="information"></param>
        /// <returns></returns>
        Task ReportWorkerRegistered(ClusterWorkerNode information);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        Task WorkerStateSyncing(int WorkerID, string WorkerState);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<List<ScriptModel>> GetDefaultModel();
    }
}