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
        /// together with order and ID, 
        /// Emitted time define unique identification of CommandLine
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Output of an commandline
        /// </summary>
        public string Command { get; set; }
    }
}