using SharedHost.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Signalling.Models
{
    public class Session
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int SlaveId { get; set; }
        public DateTime Created { get; set; }

        /*provide both created time and closed time for better management*/

        public Session(SessionRequest req)
        {
            ClientId = req.ClientId;
            SlaveId = req.SlaveId;
        }
    }
}
