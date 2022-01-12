using System.ComponentModel.DataAnnotations;

namespace Conductor.Models
{
    public class StorageBlock
    {
        [Key]
        public int StorageID { get; set; }

        public string DriveName { get; set; }

        public int Capacity { get; set; }
    }
}