using SharedHost.Models;
using System.ComponentModel.DataAnnotations;

namespace SlaveManager.Models
{
    public class CommandLog : CommandResult
    {
        /// <summary>
        /// ID is slave id of the slave device that emmit this command line
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// each commandline out put will be attached with a process ID,
        /// process id is a number ranged from 2 to 8
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// together with order and ID, 
        /// Emitted time define unique identification of CommandLine
        /// </summary>
        public int Time { get; set; }

        /// <summary>
        /// Output of an commandline
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// link commandlog table to slave table
        /// </summary>
        public virtual Slave Machine { get; set; }
    }
}
