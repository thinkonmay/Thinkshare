using SharedHost.Models.Device;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedHost.Models.Error
{
    public class GeneralError
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime ErrorTime { get; set; }

        public int Module { get; set; }

        public string ErrorMessage { get; set; }

        public virtual Slave Machine {get;set;}
    }
}
