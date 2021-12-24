using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using SharedHost.Models.Device;

namespace SharedHost.Models.Session
{
    public class SessionClient
    {
        [Required]
        public string signallingurl { get; set; }

        [Required]
        public string turnip { get; set; }

        [Required]
        public string turnuser { get; set; }

        [Required]
        public string turnpassword{ get; set; }

        [Required]
        public string turn{ get; set; }

        [Required]
        public List<string> stuns { get; set; }

        [Required]
        public Codec audiocodec { get; set; }

        [Required]
        public Codec videocodec { get; set; }
    }

    public class SessionWorker
    {
        [Required]
        public string signallingurl { get; set; }

        [Required]
        public string turn { get; set; }

        [Required]
        public List<string> stuns { get; set; }

        [Required]
        public int screenwidth { get; set; }

        [Required]
        public int screenheight { get; set; }

        [Required]
        public Codec audiocodec { get; set; }

        [Required]
        public Codec videocodec { get; set; }

        [Required]
        public QoEMode mode { get; set; }

        [Required]
        public DeviceType clientdevice { get; set; }

        [Required]
        public CoreEngine clientengine { get; set; }
    }
}
