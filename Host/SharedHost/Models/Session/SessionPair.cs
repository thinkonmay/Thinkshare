using System;
using System.Collections.Generic;
using System.Text;

namespace SharedHost.Models.Session
{
    public class SessionPair
    {
        public int SessionClientID { get; set; }

        public int SessionSlaveID { get; set; }
    }
}
