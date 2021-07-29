using SlaveManager.Models.User;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace SlaveManager.Models
{
    public class Client
    {
        /// <summary>
        /// Each client indentified by unnique ClientID
        /// </summary>
        [Key]
        public int ClientID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public UserAccount account { get; set; }

        /// <summary>
        /// One Client can obtain a number of session at the same time
        /// </summary>
        public virtual ICollection<Session> currentSession { get; set; }

        /// <summary>
        /// each client can have a specific number of cloud storage at the same time
        /// </summary>
        public virtual ICollection<StorageBlock> currentStorage { get; set; }


    }
}