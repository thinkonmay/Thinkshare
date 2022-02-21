using System.Collections.Generic;



namespace SharedHost.Models.Device
{
    public enum DeviceType
    {
        WEB_APP,
        WINDOW_APP,
        LINUX_APP,
        MAC_OS_APP,
        ANDROID_APP,
        IOS_APP,

        MAX_DEVICE
    }

    public enum CoreEngine
    {
        GSTREAMER,
        CHROME,

        MAX_ENGINE
    }

    public enum Codec
    {
        CODEC_H265,
        CODEC_H264,
        CODEC_VP8,
        CODEC_VP9,

        OPUS_ENC,
        AAC_ENC,

        MAX_CODEC
    }

    public enum QoEMode
    {
        ULTRA_LOW_CONST = 1,
        LOW_CONST,
        MEDIUM_CONST,
        HIGH_CONST,
        VERY_HIGH_CONST,
        ULTRA_HIGH_CONST,

        MAX_MODE
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


        public static UserSetting Validate(UserSetting old, UserSetting setting)
        {
            UserSetting result = old;
            List<DeviceType> allowedDevice = new List<DeviceType> { DeviceType.WEB_APP , DeviceType.WINDOW_APP };
            List<Codec> allowedVideoCodec  = new List<Codec> { Codec.CODEC_H264, Codec.CODEC_H265 };
            List<Codec> allowedAudioCodec  = new List<Codec> { Codec.OPUS_ENC };

            if(setting.engine < CoreEngine.MAX_ENGINE)
                result.engine = setting.engine;
            if(setting.mode   < QoEMode.MAX_MODE)
                result.mode   = setting.mode;
            if(allowedDevice.Contains(setting.device))
                result.device = setting.device;
            if(allowedAudioCodec.Contains(setting.audioCodec))
                result.audioCodec = setting.audioCodec;
            if(allowedVideoCodec.Contains(setting.videoCodec))
                result.videoCodec = setting.videoCodec;
            
            return result;
        }
    }
}