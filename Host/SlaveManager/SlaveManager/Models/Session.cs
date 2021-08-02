using SharedHost.Models;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SlaveManager.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Session : SessionBase
    {
        public int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int SessionSlaveID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int SessionClientID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? EndTime { get; set; }
    }
}