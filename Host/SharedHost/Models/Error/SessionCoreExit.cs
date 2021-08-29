using System;
using System.Collections.Generic;
using System.Text;

namespace SharedHost.Models.Error
{
    public class SessionCoreExitAbsTime
    {
        public string ExitTime { get; set; }

        public int ExitCode { get; set; }

        public string CoreState { get; set; }

        public string PipelineState { get; set; }

        public string PeerCallState { get; set; }

        public string SignallingState { get; set; }

        public string Message { get; set; }
    }
}
