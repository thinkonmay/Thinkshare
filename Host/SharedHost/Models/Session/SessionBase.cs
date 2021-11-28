using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using SharedHost.Models.Device;

namespace SharedHost.Models.Session
{
    public class SessionClient
    {
        [Required]
        public string SignallingUrl { get; set; }

        [Required]
        public string turnIP { get; set; }

        [Required]
        public string turnUser { get; set; }

        [Required]
        public string turnPassword { get; set; }

        [Required]
        public List<string> STUNlist { get; set; }

        [Required]
        public Codec AudioCodec { get; set; }

        [Required]
        public Codec VideoCodec { get; set; }
    }

    public class SessionWorker
    {
        [Required]
        public string SignallingUrl { get; set; }

        [Required]
        public string turnConnection { get; set; }

        [Required]
        public List<string> STUNlist { get; set; }

        [Required]
        public int ScreenWidth { get; set; }

        [Required]
        public int ScreenHeight { get; set; }

        [Required]
        public Codec AudioCodec { get; set; }

        [Required]
        public Codec VideoCodec { get; set; }

        [Required]
        public QoEMode QoEMode { get; set; }
    }
}
