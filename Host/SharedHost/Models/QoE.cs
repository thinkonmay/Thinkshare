namespace SharedHost.Models
{
    public class QoE
    {
        public QoE() { }
        public QoE(ClientDeviceCapabilities cap)
        {

            ScreenHeight = cap.screenHeight;
            ScreenWidth = cap.screenWidth;
            Bitrate = cap.bitrate;
            VideoCodec = cap.videoCodec;
            AudioCodec = cap.audioCodec;
            QoEMode = cap.mode;
        }
        public int ScreenWidth { get; set; }

        public int ScreenHeight { get; set; }

        public int Framerate { get; set; }

        public int Bitrate { get; set; }

        public Codec AudioCodec { get; set; }

        public Codec VideoCodec { get; set; }

        public QoEMode QoEMode { get; set; }
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
