using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using SharedHost.Models.Device;
using RestSharp;
using SharedHost.Models.Shell;
using System.Net;

namespace WorkerManager.Models
{
    public class ClusterWorkerNode 
    {
        public int ID{ get; set; }

        public WorkerRegisterModel model {get;set;}
    }
}
