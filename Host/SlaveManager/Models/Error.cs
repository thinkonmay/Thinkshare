using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlaveManager.Models
{
    public class GeneralError
    {
        public GeneralError()
        { }

        public GeneralError(GeneralErrorAbsTime abs)
        {
            ErrorTime = new DateTime(1970, 1, 1).AddMilliseconds(abs.ErrorTime);

            Id = abs.Id;

            ErrorMessage = abs.ErrorMessage;

            Machine = abs.Machine;
        }

        public DateTime ErrorTime { get; set; }

        public int Id { get; set; }

        public string ErrorMessage { get; set; }

        public virtual Slave Machine {get;set;}
    }

    public class GeneralErrorAbsTime
    {

        public int ErrorTime { get; set; }

        public int Id { get; set; }

        public string ErrorMessage { get; set; }

        public virtual Slave Machine { get; set; }
    }


    public class SessionCoreExitBase
    {
        public int Id { get; set; }

        public int ExitCode { get; set; }

        public string CoreState { get; set; }

        public string PipelineState { get; set; }

        public string PeerCallState { get; set; }
            
        public string Message { get; set; } 


    }

    public class SessionCoreExitAbsTime : SessionCoreExitBase
    {
        public int ExitTime { get; set; }
    }

    public class SessionCoreExit : SessionCoreExitBase
    {
        public SessionCoreExit()
        { }

        public SessionCoreExit(SessionCoreExitAbsTime abs )
        {
            ExitTime = new DateTime(1970, 1, 1).AddMilliseconds(abs.ExitTime);
            Id = abs.Id;
            ExitCode = abs.Id;
            CoreState = abs.CoreState;
            PipelineState = abs.PipelineState;
            PeerCallState = abs.PeerCallState;
            Message = abs.Message;
        }

        public DateTime ExitTime { get; set; }
    }


}
