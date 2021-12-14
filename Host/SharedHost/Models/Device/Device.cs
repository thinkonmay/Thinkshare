using Conductor.Models;
using SharedHost.Models.Session;

namespace SharedHost.Models.Device
{
    public enum DeviceType
    {
        WEB_APP,
        WINDOW_APP,
        LINUX_APP,
        MAC_OS_APP,
        ANDROID_APP,
        IOS_APP
    }

    public enum CoreEngine
    {
        GSTREAMER,
        CHROME,
    }

    public enum Codec
    {
        CODEC_H265,
        CODEC_H264,
        CODEC_VP8,
        CODEC_VP9,

        OPUS_ENC,
        AAC_ENC
    }

    public enum QoEMode
    {
        ULTRA_LOW_CONST = 1,
        LOW_CONST,
        MEDIUM_CONST,
        HIGH_CONST,
        VERY_HIGH_CONST,
        ULTRA_HIGH_CONST,

        SEGMENTED_ADAPTIVE_BITRATE,
        NON_OVER_SAMPLING_ADAPTIVE_BITRATE,
        OVER_SAMPLING_ADAPTIVE_BITRATE,
        PREDICTIVE_ADAPTIVE_BITRATE
    }

    public class UserSetting
    {
        public DeviceType device {get;set;}

        public CoreEngine engine {get;set;}

        public Codec audioCodec { get; set; }

        public Codec videoCodec { get; set; }

        public QoEMode mode { get; set; }

        public int screenWidth { get; set; }

        public int screenHeight { get; set; }
    }
}