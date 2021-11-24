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
        Task SessionTerminate(int slaveid);

        Task SessionDisconnect(int slaveid);

        Task SessionReconnect(int ID, SessionBase session);
        
        Task SessionInitialize(int ID, string token, SessionBase session);

        Task AssignGlobalID(int ClusterID, int GlobalID, int PrivateID);
    }
}
