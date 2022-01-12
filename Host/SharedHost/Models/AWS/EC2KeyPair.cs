using System.ComponentModel.DataAnnotations;

namespace SharedHost.Models.AWS
{
    public class EC2KeyPair
    {
        [Key]
        public int ID { get; set; }
        public string Name { set; get; }
        public string PrivateKey { get; set; }
    }
}

