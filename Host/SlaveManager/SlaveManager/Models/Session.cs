using System;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;


namespace SlaveManager.Models
{
    public class Session
    {
        public int ClientID { get; set; }

        public int SlaveID { get; set; }

        public int SessionSlaveID { get; set; }

        public int SessionClientID { get; set; }

        public string SignallingUrl { get; set; }

        public string StunServer { get; set; }

        public bool ClientOffer { get; set; }

        public QoE QoE { get; set; }
    }

    public class QoE
    {
        public int ScreenWidth { get; set; }

        public int ScreenHeight { get; set; }

        public int Framerate { get; set; }

        public int Bitrate { get; set; }

        public Codec AudioCodec { get; set; }

        public Codec VideoCodec { get; set; }

        public QoEMode mode { get; set; }
    }

    public enum Codec
    {
        CODEC_NVH265,
        CODEC_NVH264,
        CODEC_VP9,

        CODEC_OPUS_ENC
    }

    public enum QoEMode
    {
        AUDIO_PIORITIZE,
        VIDEO_PIORITIZE
    }


}