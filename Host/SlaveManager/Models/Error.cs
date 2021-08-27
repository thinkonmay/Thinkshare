using SlaveManager.SlaveDevices;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SlaveManager.Models
{
    public class GeneralError
    {
        public GeneralError()
        { }

        public GeneralError(GeneralErrorAbsTime abs, Slave _Slave)
        {		
            DateTime ErrorTime = DateTime.Parse(abs.ErrorTime);

            ErrorMessage = abs.ErrorMessage;

            Machine = _Slave;
        }

        public DateTime ErrorTime { get; set; }


        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string ErrorMessage { get; set; }

        public virtual Slave Machine {get;set;}
    }

    public class GeneralErrorAbsTime
    {
        public string ErrorTime { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class SessionCoreExitAbsTime 
    {
        public string ExitTime { get; set; }

        public int ExitCode { get; set; }

        public string CoreState { get; set; }

        public string PipelineState { get; set; }

        public string PeerCallState { get; set; }

        public string SignallingState {get;set;}

        public string Message { get; set; }
    }

    public class SessionCoreExit
    {
        public SessionCoreExit()
        { }

        public SessionCoreExit(SessionCoreExitAbsTime abs, Slave _Slave)
        {
            ExitTime = DateTime.Parse(abs.ExitTime);
            ExitCode = abs.ExitCode;
            CoreState = abs.CoreState;
            PipelineState = abs.PipelineState;
            PeerCallState = abs.PeerCallState;
            Message = abs.Message;
            Slave = _Slave;
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime ExitTime { get; set; }

        public int ExitCode { get; set; }

        public string CoreState { get; set; }

        public string PipelineState { get; set; }

        public string PeerCallState { get; set; }

        public string? Message { get; set; }

        public virtual Slave Slave {get;set;}
    }


}
