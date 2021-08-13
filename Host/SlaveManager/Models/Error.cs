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

        public GeneralError(GeneralErrorAbsTime abs)
        {
            //ErrorTime = new DateTime(1970, 1, 1).AddMilliseconds(abs.ErrorTime).ToString("dd/MM/yyyy HH:mm:ss"); //port to string for compability with postgresql;

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

    public class SessionCoreExitAbsTime 
    {
        public int ExitTime { get; set; }

        public int ExitCode { get; set; }

        public string CoreState { get; set; }

        public string PipelineState { get; set; }

        public string PeerCallState { get; set; }

        public string Message { get; set; }
    }

    public class SessionCoreExit
    {
        public SessionCoreExit()
        { }

        public SessionCoreExit(SessionCoreExitAbsTime abs )
        {
            //ExitTime = new DateTime(1970, 1, 1).AddMilliseconds(abs.ExitTime).ToString("dd/MM/yyyy HH:mm:ss"); //port to string for compability with postgresql;
            ExitCode = abs.ExitCode;
            CoreState = abs.CoreState;
            PipelineState = abs.PipelineState;
            PeerCallState = abs.PeerCallState;
            Message = abs.Message;
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime ExitTime { get; set; }

        public int ExitCode { get; set; }

        public string CoreState { get; set; }

        public string PipelineState { get; set; }

        public string PeerCallState { get; set; }

        public string Message { get; set; }
    }


}
