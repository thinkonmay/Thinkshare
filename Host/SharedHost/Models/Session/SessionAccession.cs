﻿using SharedHost.Models.Device;

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
