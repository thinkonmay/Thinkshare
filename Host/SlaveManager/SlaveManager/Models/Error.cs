using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlaveManager.Models
{
    public class AgentError
    {
        public int Time { get; set; }

        public string AgentState { get; set; }

        public string ErrorMessage { get; set; }

        public virtual Slave slave {get;set;}
    }

    public class SessionCoreExitAbsTime 
    {
        public int ExitCode { get; set; }

        public int ExitTime { get; set; }

        public string CoreState { get; set; }

        public string PipelineState { get; set; }

        public string PeerCallState { get; set; }

        public string Message { get; set; } 
    }

    public class SessionCoreExit
    {
        public int ExitCode { get; set; }

        public DateTime ExitTime { get; set; }

        public string CoreState { get; set; }

        public string PipelineState { get; set; }

        public string PeerCallState { get; set; }

        public string Message { get; set; }
    }


}
