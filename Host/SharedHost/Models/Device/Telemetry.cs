using System;
using System.Collections.Generic;
using System.Text;

namespace SharedHost.Models.Device
{
    public class Telemetry
    {
        public int SlaveId { get; set; }
        public int Time { get; set; }

        public int CPUusage;
        public int GPUusage;
        public int RAMusage;
    }
}
