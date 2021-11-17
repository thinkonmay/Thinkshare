using SharedHost.Models.Device;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharedHost.Models.Session
{
    public class SessionAccession
    {
        public Module Module { get; set; }

        public int ClientID { get; set; }

        public int WorkerID { get; set; }

        public int ID { get; set; }
    }
}
