using MersenneTwister;
using SharedHost.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SlaveManager.Models
{
    /// <summary>
    /// Slavedevice is the cloud computer device that allow user to access 
    /// </summary>
    public class Slave
    {
        public Slave()
        { }

        public Slave(SlaveDeviceInformation information)
        {
            CPU = information.CPU;
            GPU = information.GPU;
            RAMcapacity = information.RAMcapacity;
            OS = information.OS;
            ID = information.ID;
        }


        /// <summary>
        /// Each slave device defined with an unique ID 
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }


        public DateTime? Register { get; set; }


        /// <summary>
        /// slave device hardware configuration
        /// </summary>
        public string CPU { get; set; }
        public string GPU { get; set; }
        public int RAMcapacity { get; set; }
        public string OS { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual ICollection<CommandLog> CommandLogs { get; set; }

        /// <summary>
        /// (nullable) if slave is in a session, 
        /// </summary>
        public virtual ICollection<Session> servingSession { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual ICollection<GeneralError> GeneralErrors { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public virtual ICollection<SessionCoreExit> SessionCoreExits { get; set; }
    }
}