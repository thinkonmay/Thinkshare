namespace SharedHost.Models.Session
{
    public class SessionMetric
    {
        public int FrameRate { get; set; }
        public int AudioLatency { get; set; }
        public int VideoLatency { get; set; }
        public int AudioBitrate { get; set; }
        public int VideoBitrate { get; set; }
        public int TotalBandwidth { get; set; }
        public int PacketsLost { get; set; }
    }




    public class ErrorMessage
    {
        public const string    UNKNOWN_ERROR =                       "Unknown error";
        public const string    DATA_CHANNEL_ERROR =                  "Datachannel error";
        public const string    UNKNOWN_MESSAGE =                     "Unknown message";
        public const string    SIGNALLING_ERROR =                    "Signalling error";
        public const string    UNKNOWN_SESSION_CORE_EXIT =           "Unknown session core exit";



        public const string    UNDEFINED_ERROR =                     "UndefinedError";
        public const string    AGENT_STATE_CONFLICT =                "Agent state conflict";
        public const string    CURRUPTED_CONFIG_FILE =               "Corrupted file";
        public const string    ERROR_FILE_OPERATION =                "Error file operation";
        public const string    ERROR_PROCESS_OPERATION =             "Error process operation";
    }
}
