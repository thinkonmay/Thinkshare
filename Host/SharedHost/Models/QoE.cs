namespace SharedHost.Models
{
    public class QoE
    {
        public QoE() { }
        public QoE(ClientDeviceCapabilities cap)
        {

            ScreenHeight = cap.screenHeight;
            ScreenWidth = cap.screenWidth;
            VideoCodec = cap.videoCodec;
            AudioCodec = cap.audioCodec;
            QoEMode = cap.mode;
        }
        public int ScreenWidth { get; set; }

        public int ScreenHeight { get; set; }

        public int Framerate { get; set; }

        public Codec AudioCodec { get; set; }

        public Codec VideoCodec { get; set; }

        public QoEMode QoEMode { get; set; }
    }

    public enum Codec
    {
        CODEC_H265,
        CODEC_H264,
        CODEC_VP9,
        OPUS_ENC,
        AAC_ENC
    }

    public enum QoEMode
    {
        AUDIO_PIORITY,
        VIDEO_PIORITY
    }


    public class ErrorMessage
    {
        public const string    UNKNOWN_ERROR =                       "Unknown error";
        public const string    DATA_CHANNEL_ERROR =                  "Datachannel error";
        public const string    UNKNOWN_MESSAGE =                     "Unknown message";
        public const string    SIGNALLING_ERROR =                    "Signalling error";
        public const string    UNKNOWN_SESSION_CORE_EXIT =           "Unknown session core exit";



        public const string    UNDEFINED_ERROR =                "UndefinedError";
        public const string    AGENT_STATE_CONFLICT =           "Agent state conflict";
        public const string    CURRUPTED_CONFIG_FILE =          "Corrupted file";
        public const string    ERROR_FILE_OPERATION =           "Error file operation";
        public const string    ERROR_PROCESS_OPERATION =        "Error process operation";
    }
}
