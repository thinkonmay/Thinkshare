using SharedHost.Models.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedHost.Models.Device;
using SharedHost.Models.Shell;
using RestSharp;

namespace Conductor.Interfaces
{
    public interface IWorkerCommnader
    {
        Task SessionTerminate(int ID);

        Task SessionDisconnect(int ID);

        Task SessionReconnect(int ID);
        
        Task SessionInitialize(int ID, string token);

        Task AssignGlobalID(int ClusterID, int GlobalID, int PrivateID);

        Task<string> GetWorkerState(int WorkerID);
    }
}
