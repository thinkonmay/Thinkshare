﻿using SharedHost.Models.Device;

namespace SharedHost.Models.Session
{
    /// <summary>
    /// Contain session initialize request that user send to host
    /// in order to start remote control session
    /// </summary>
    public class ClientRequest
    {
        /// <summary>
        /// slave id that match with requested user
        /// </summary>
        public int SlaveId { get; set; }

        /// <summary>
        /// quality of experience 
        /// </summary>
        public DeviceCap cap { get; set; }
    }
}
