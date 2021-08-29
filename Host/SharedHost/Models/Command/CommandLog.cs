using System;
using System.ComponentModel.DataAnnotations.Schema;
using SharedHost.Models.Device;


namespace SharedHost.Models.Command
{
    public class CommandLog
    {
        /// <summary>
        /// ID is primary key for command log
        /// </summary>
        /// [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// each commandline out put will be attached with a process ID,
        /// process id is a number ranged from 2 to 8
        /// </summary>
        public int ProcessID { get; set; }

        /// <summary>
        /// together with order and ID, 
        /// Emitted time define unique identification of CommandLine
        /// </summary>
        public DateTime Time { get; set; }//port to string for compability with postgresql timestamp

        /// <summary>
        /// Output of an commandline
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// link commandlog table to slave table
        /// </summary>
        public virtual Slave Slave { get; set; }
    }
}