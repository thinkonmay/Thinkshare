using SharedHost.Models;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SlaveManager.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Session : SystemSession
    {
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