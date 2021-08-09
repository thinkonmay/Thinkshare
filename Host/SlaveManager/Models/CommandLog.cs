using MersenneTwister;
using SharedHost.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SlaveManager.Models
{
    public class CommandLog
    {
        public CommandLog()
        { }

        public CommandLog(Slave _Slave, ReceiveCommand receive)
        {
            ID = Randoms.Next();
            ProcessID = receive.ProcessID;
            Command = receive.Command;
            Time = new DateTime(1970, 1, 1).AddMilliseconds(receive.Time);
            Slave = _Slave;
        }


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
        public DateTime Time { get; set; }

        /// <summary>
        /// Output of an commandline
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// link commandlog table to slave table
        /// </summary>
        public virtual Slave  Slave { get; set; }
    }
}